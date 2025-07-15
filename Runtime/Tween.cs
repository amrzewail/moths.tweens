using Moths.Tweens.Memory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Tweens
{
    public enum UpdateType
    {
        Update,
        FixedUpdate,
        UnscaledUpdate,
        UnscaledFixedUpdate,
    };

    public unsafe partial struct Tween<TContext, TValue> where TValue : unmanaged
    {
        internal struct SharedData
        {
            public float time;
            public float duration;

            public int tweenIndex;
            public bool isPlaying;
            public bool isPaused;

            public Tween<TContext, TValue> tween;
        };

        internal struct ManagedData
        {
            public TContext context;
            public AnimationCurve curve;
            public bool hasLink;
            public object obj;
            public Action<TContext, TValue> onValueChange;
            public Action<TContext> onComplete;
        }

        private Ptr<SharedData> _data;
        private ManagedData Managed => _tweens[_data.Pointer->tweenIndex].managed;

        //private ManagedPtr<TContext> _context;
        private TValue _startValue;
        private TValue _endValue;
        private delegate*<TValue, TValue, float, Ease, TValue> _updater;
        private Ptr<CancellationToken> _cts;
        private Ptr<CancellationToken.State> _cancellationState;
        private Ease _ease;
        private UpdateType _updateType;

        //private ManagedPtr<AnimationCurve> _curve;
        //private ManagedPtr<object> _obj;

        //private ManagedPtr<Action<TContext, TValue>> _onValueChange;
        //private ManagedPtr<Action<TContext>> _onComplete;

        public bool IsPlaying => !_data.IsNull() && _data.Value.isPlaying;
        public bool IsPaused => IsPlaying ? _data.Value.isPaused : false;
        public float Time => Mathf.Max(IsPlaying ? _data.Value.time : 0, 0);
        public float Duration => _data.IsNull() ? 0 : _data.Value.duration;
        public float Value => Duration > 0 ? Mathf.Clamp01(Time / Duration) : 1;
        public TValue EasedValue => _updater(_startValue, _endValue, Value, _ease);

        public static Tween<TContext, TValue> Create(TweenBuilder<TContext, TValue> builder)
        {
            var tween = new Tween<TContext, TValue>();

            //tween._context = new ManagedPtr<TContext>(builder.Context);
            tween._startValue = builder.StartValue;
            tween._endValue = builder.EndValue;
            tween._updater = builder.Updater;
            tween._updateType = builder.UpdateType;

            tween._cts = builder.Cts;
            if (!tween._cts.IsNull()) tween._cancellationState = tween._cts.Pointer->Create();
            tween._ease = builder.Ease;
            //if (builder.Curve != null) tween._curve = new ManagedPtr<AnimationCurve>();
            //if (builder.Obj != null) tween._obj = new ManagedPtr<object>(builder.Obj);
            //if (builder.OnValueChange != null) tween._onValueChange = new ManagedPtr<Action<TContext, TValue>>(builder.OnValueChange);
            //if (builder.OnComplete != null) tween._onComplete = new ManagedPtr<Action<TContext>>(builder.OnComplete);

            ManagedData managed = new ManagedData();
            managed.context = builder.Context;
            managed.curve = builder.Curve;
            managed.obj = builder.Obj;
            managed.hasLink = managed.obj != null;
            managed.onValueChange = builder.OnValueChange;
            managed.onComplete = builder.OnComplete;

            tween._data = Allocator.Malloc();
            tween._data.Pointer->duration = builder.Duration;
            tween._data.Pointer->time = -builder.Delay;

            tween._data.Pointer->isPaused = false;
            tween._data.Pointer->isPlaying = false;
            tween._data.Pointer->tweenIndex = -1;

            tween._data.Pointer->tween = tween;

            tween._data.Pointer->tweenIndex = AllocateTween(tween._data, managed);
            
            return tween;
        }

        public void Play()
        {
            if (_data.IsNull()) return;
            if (_data.Pointer->tweenIndex == -1)
            {
                Dispose();
                return;
            }
            if (IsPaused) _data.Pointer->isPaused = false;
            if (IsPlaying) return;
            _data.Pointer->isPlaying = true;
            ListenUpdate();
            if (!_cancellationState.IsNull()) _cancellationState.Pointer->count++;
        }

        public void Pause()
        {
            if (IsPaused) return;
            _data.Pointer->isPaused = true;
        }

        public void Cancel()
        {
            if (!IsPlaying) return;

            UnlistenUpdate();
            Dispose();
        }

        public void Complete()
        {
            if (!IsPlaying) return;
            var managed = Managed;
            managed.onComplete?.Invoke(managed.context);
            //_onComplete.Value?.Invoke(_context.Value);
            Cancel();
        }

        private void Dispose()
        {
            Allocator.Free(_data);

            if (!_cancellationState.IsNull())
            {
                _cancellationState.Pointer->count--;
                if (_cancellationState.Pointer->count == 0)
                {
                    if (!_cts.IsNull()) _cts.Pointer->Dispose();
                }
            }
        }

        private void ListenUpdate()
        {
            StartTween(_data.Pointer->tweenIndex);
        }

        private void UnlistenUpdate()
        {
            RemoveTween(_data.Pointer->tweenIndex);
        }

        private unsafe void Update()
        {
            var managed = Managed;

            if (managed.hasLink && managed.obj == null)
            {
                Cancel();
                return;
            }

            if (!_cancellationState.IsNull() && _cancellationState.Pointer->isCancelled)
            {
                Cancel();
                return;
            }

            SharedData* data = _data.Pointer;

            if (data->isPaused) return;

            float deltaTime = _updateType switch
            {
                UpdateType.Update => Tweener.DeltaTime,
                UpdateType.FixedUpdate => Tweener.DeltaTime,
                UpdateType.UnscaledUpdate => Tweener.UnscaledDeltaTime,
                UpdateType.UnscaledFixedUpdate => Tweener.UnscaledDeltaTime,
                _ => Tweener.DeltaTime
            };

            data->time += deltaTime;
            
            // wait for delay
            if (data->time < 0) return;

            float value = data->duration > 0 ? Mathf.Clamp01(data->time / data->duration) : 1;

            if (managed.curve != null)
            {
                var curve = managed.curve;
                if (curve != null ) value = curve.Evaluate(value);
            }

            if (_updater != null)
            {
                var currentValue = _updater(_startValue, _endValue, value, _ease);

                managed.onValueChange?.Invoke(managed.context, currentValue);
            }

            if (data->time >= data->duration) Complete();
        }
    }
}
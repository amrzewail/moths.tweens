using Moths.Tweens.Memory;
using System;
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

    public unsafe partial struct Tween<TContext, TValue>
    {
        internal struct SharedData
        {
            public float time;
            public float duration;

            public int updateIndex;
            public bool isPlaying;
            public bool isPaused;
        };

        private static Allocator<SharedData> Allocator;

        private Ptr<SharedData> _data;

        private TContext _context;
        private TValue _currentValue;
        private TValue _startValue;
        private TValue _endValue;
        private delegate*<TValue, TValue, float, Ease, TValue> _updater;
        private Ptr<CancellationToken> _cts;
        private Ptr<CancellationToken.State> _cancellationState;
        private Ease _ease;
        private AnimationCurve _curve;
        private UpdateType _updateType;
        private UnityEngine.Object _obj;

        private Action<TContext, TValue> _onValueChange;
        private Action<TContext> _onComplete;

        public bool IsPlaying => !_data.IsNull() && _data.Value.isPlaying;
        public bool IsPaused => IsPlaying ? _data.Value.isPaused : false;
        public float Time => Mathf.Max(IsPlaying ? _data.Value.time : 0, 0);
        public float Duration => _data.IsNull() ? 0 : _data.Value.duration;
        public float Value => Duration > 0 ? Mathf.Clamp01(Time / Duration) : 1;
        public TValue EasedValue => _updater(_startValue, _endValue, Value, _ease);

        public static Tween<TContext, TValue> Create(TweenBuilder<TContext, TValue> builder)
        {
            var tween = new Tween<TContext, TValue>();

            tween._context = builder.Context;
            tween._startValue = builder.StartValue;
            tween._endValue = builder.EndValue;
            tween._updater = builder.Updater;
            tween._cts = builder.Cts;
            if (!tween._cts.IsNull()) tween._cancellationState = tween._cts.Pointer->Create();
            tween._ease = builder.Ease;
            tween._curve = builder.Curve;
            tween._updateType = builder.UpdateType;
            tween._obj = builder.Obj;
            tween._onValueChange = builder.OnValueChange;
            tween._onComplete = builder.OnComplete;

            tween._data = Allocator.Malloc();
            tween._data.Pointer->duration = builder.Duration;
            tween._data.Pointer->time = -builder.Delay;

            tween._data.Pointer->isPaused = false;
            tween._data.Pointer->isPlaying = false;
            tween._data.Pointer->updateIndex = -1;

            return tween;
        }

        public static Tween<TContext, TValue> Create(delegate*<TValue, TValue, float, Ease, TValue> updater)
        {
            var tween = new Tween<TContext, TValue>();
            tween._data = Allocator.Malloc();
            tween._data.Pointer->time = 0;
            tween._data.Pointer->isPaused = false;
            tween._data.Pointer->isPlaying = false;
            tween._data.Pointer->updateIndex = -1;
            tween._updater = updater;
            return tween;
        }

        public Tween<TContext, TValue> Play()
        {
            if (_data.IsNull()) return this;
            if (IsPaused) _data.Pointer->isPaused = false;
            if (IsPlaying) return this;
            _data.Pointer->isPlaying = true;
            ListenUpdate();
            if (!_cancellationState.IsNull()) _cancellationState.Pointer->count++;
            return this;
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
        public void Complete()
        {
            if (!IsPlaying) return;
            Cancel();
            _onComplete?.Invoke(_context);
        }

        private void ListenUpdate()
        {
            if (_data.Pointer->updateIndex == -1)
            {
                if (_updateType == UpdateType.Update || _updateType == UpdateType.UnscaledUpdate)
                {
                    _data.Pointer->updateIndex = Tweener.SubscribeUpdate(Update);
                }
                else
                {
                    _data.Pointer->updateIndex = Tweener.SubscribeFixedUpdate(Update);
                }
            }
        }

        private void UnlistenUpdate()
        {
            if (_data.Pointer->updateIndex == -1) return;
            if (_updateType == UpdateType.Update || _updateType == UpdateType.UnscaledUpdate)
            {
                Tweener.UnsubscribeUpdate(_data.Pointer->updateIndex);
            }
            else
            {
                Tweener.UnsubscribeFixedUpdate(_data.Pointer->updateIndex);
            }
            _data.Pointer->updateIndex = -1;
        }

        private unsafe void Update()
        {
            if (!_obj)
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

            if (_curve != null) value = _curve.Evaluate(value);

            if (_updater != null)
            {
                _currentValue = _updater(_startValue, _endValue, value, _ease);
            }

            _onValueChange?.Invoke(_context, _currentValue);

            if (data->time >= data->duration) Complete();
        }
    }
}
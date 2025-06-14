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

    public struct CancellationToken : IDisposable
    {
        private static Allocator<State> Allocator;

        internal struct State
        {
            public bool isCancelled;
            public int count;
        }

        private bool _isCreated;
        private Ptr<State> _state;

        internal unsafe Ptr<State> Create()
        {
            if (_isCreated) return _state;
            _isCreated = true;
            _state = Allocator.Malloc();
            _state.Pointer->isCancelled = false;
            return _state;
        }
        public unsafe void Cancel()
        {
            if (!_isCreated) return;
            _isCreated = false;
            _state.Pointer->isCancelled = true;
            Allocator.Free(_state);
        }

        public void Dispose()
        {
            Cancel();
        }
    }

    public unsafe struct Tween<TContext, TValue>
    {
        internal struct SharedData
        {
            public float time;
            public bool isPlaying;
            public bool isPaused;
            public float duration;
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

        public static Tween<TContext, TValue> Create(delegate*<TValue, TValue, float, Ease, TValue> updater)
        {
            var tween = new Tween<TContext, TValue>();
            tween._data = Allocator.Malloc();
            tween._data.Ref.time = 0;
            tween._data.Ref.isPaused = false;
            tween._data.Ref.isPlaying = false;
            tween._updater = updater;
            return tween;
        }

        public Tween<TContext, TValue> SetStartValue(TValue startValue) { _startValue =  startValue; return this; }
        public Tween<TContext, TValue> SetEndValue(TValue endValue) { _endValue =  endValue; return this; }
        public Tween<TContext, TValue> SetDuration(float duration) { _data.Ref.duration = duration; return this; }
        public Tween<TContext, TValue> SetUpdater(delegate*<TValue, TValue, float, Ease, TValue> updater) { _updater = updater; return this; }
        public Tween<TContext, TValue> SetContext(TContext context) { _context = context; return this; }
        public Tween<TContext, TValue> SetOnValueChange(Action<TContext, TValue> onValueChange) { _onValueChange = onValueChange; return this; }
        public Tween<TContext, TValue> SetOnComplete(Action<TContext> onComplete) { _onComplete = onComplete; return this; }
        public Tween<TContext, TValue> SetEase(Ease ease) { _ease = ease; return this; }
        public Tween<TContext, TValue> SetCurve(AnimationCurve curve) { _curve = curve; return this; }
        public Tween<TContext, TValue> SetUpdateType(UpdateType updateType) { _updateType = updateType; return this; }
        public Tween<TContext, TValue> SetLink(UnityEngine.Object obj) { _obj = obj; return this; }
        public Tween<TContext, TValue> SetDelay(float delay) { _data.Ref.time = -delay; return this; }
        public Tween<TContext, TValue> SetCancellationToken(ref CancellationToken token) { _cts = new Ptr<CancellationToken>(ref token); _cancellationState = token.Create(); return this; }

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
            if (!Tweener.UpdateActions.TryGetValue(_data, out Action update)) Tweener.UpdateActions[_data] = update = Update;
            if (_updateType == UpdateType.Update || _updateType == UpdateType.UnscaledUpdate)
            {
                Tweener.Update += update;
            }
            else
            {
                Tweener.FixedUpdate += update;
            }
        }

        private void UnlistenUpdate()
        {
            if (!Tweener.UpdateActions.TryGetValue(_data, out Action update)) return;
            if (_updateType == UpdateType.Update || _updateType == UpdateType.UnscaledUpdate)
            {
                Tweener.Update -= update;
            }
            else
            {
                Tweener.FixedUpdate -= update;
            }
            Tweener.UpdateActions.Remove(_data);
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
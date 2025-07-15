using Moths.Tweens.Memory;
using System;
using UnityEngine;

namespace Moths.Tweens
{
    public unsafe struct TweenBuilder<TContext, TValue> where TValue : unmanaged
    {
        private struct Data
        {
            internal TContext Context;
            internal TValue StartValue;
            internal TValue EndValue;
            internal float Duration;
            internal float Delay;
            internal delegate*<TValue, TValue, float, Ease, TValue> Updater;
            internal Ptr<CancellationToken> Cts;
            internal Ease Ease;
            internal AnimationCurve Curve;
            internal UpdateType UpdateType;
            internal object Obj;
            internal Action<TContext, TValue> OnValueChange;
            internal Action<TContext> OnComplete;
        }

        private static Data _data;

        internal TContext Context => _data.Context;
        internal TValue StartValue => _data.StartValue;
        internal TValue EndValue => _data.EndValue;
        internal float Duration => _data.Duration;
        internal float Delay => _data.Delay;
        internal delegate*<TValue, TValue, float, Ease, TValue> Updater => _data.Updater;
        internal Ptr<CancellationToken> Cts => _data.Cts;
        internal Ease Ease => _data.Ease;
        internal AnimationCurve Curve => _data.Curve;
        internal UpdateType UpdateType => _data.UpdateType;
        internal object Obj => _data.Obj;
        internal Action<TContext, TValue> OnValueChange => _data.OnValueChange;
        internal Action<TContext> OnComplete => _data.OnComplete;

        public TweenBuilder<TContext, TValue> SetStartValue(TValue startValue)
        {
            _data.StartValue = startValue;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetEndValue(TValue endValue)
        {
            _data.EndValue = endValue;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetDuration(float duration)
        {
            _data.Duration = duration;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetUpdater(delegate*<TValue, TValue, float, Ease, TValue> updater)
        {
            _data.Updater = updater;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetContext(TContext context)
        {
            _data.Context = context;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetOnValueChange(Action<TContext, TValue> onValueChange)
        {
            _data.OnValueChange = onValueChange;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetOnComplete(Action<TContext> onComplete)
        {
            _data.OnComplete = onComplete;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetEase(Ease ease)
        {
            _data.Ease = ease;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetCurve(AnimationCurve curve)
        {
            _data.Curve = curve;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetUpdateType(UpdateType updateType)
        {
            _data.UpdateType = updateType;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetLink(object obj)
        {
            _data.Obj = obj;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetDelay(float delay)
        {
            _data.Delay = delay;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetCancellationToken(ref CancellationToken token)
        {
            _data.Cts = new Ptr<CancellationToken>(ref token);
            return this;
        }

        public Tween<TContext, TValue> Build()
        {
            var tween = Tween<TContext, TValue>.Create(this);
            return tween;
        }

        public Tween<TContext, TValue> Play()
        {
            var tween = Tween<TContext, TValue>.Create(this);
            tween.Play();
            return tween;
        }
    }
}
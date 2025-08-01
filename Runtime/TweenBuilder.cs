using Moths.Tweens.Memory;
using System;
using UnityEngine;

namespace Moths.Tweens
{
    public unsafe struct TweenBuilder<TContext, TValue>
        where TValue : unmanaged
        where TContext : class
    {
        internal object Context;
        internal TValue StartValue;
        internal TValue EndValue;
        internal float Duration;
        internal float Delay;
        internal delegate*<TValue, TValue, float, Ease, TValue> Updater;
        internal Ptr<CancellationToken> Cts;
        internal Ease Ease;
        internal AnimationCurve Curve;
        internal UpdateType UpdateType;
        internal object Link;
        internal Action<TContext, TValue> OnValueChange;
        internal Action<TContext> OnComplete;

        public TweenBuilder<TContext, TValue> SetStartValue(TValue startValue)
        {
            StartValue = startValue;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetEndValue(TValue endValue)
        {
            EndValue = endValue;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetUpdater(delegate*<TValue, TValue, float, Ease, TValue> updater)
        {
            Updater = updater;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetContext(TContext context)
        {
            Context = context;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetOnValueChange(Action<TContext, TValue> onValueChange)
        {
            OnValueChange = onValueChange;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetOnComplete(Action<TContext> onComplete)
        {
            OnComplete = (Action<object>)onComplete;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetEase(Ease ease)
        {
            Ease = ease;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetCurve(AnimationCurve curve)
        {
            Curve = curve;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetUpdateType(UpdateType updateType)
        {
            UpdateType = updateType;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetLink(object link)
        {
            Link = link;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetDelay(float delay)
        {
            Delay = delay;
            return this;
        }

        public TweenBuilder<TContext, TValue> SetCancellationToken(ref CancellationToken token)
        {
            Cts = new Ptr<CancellationToken>(ref token);
            return this;
        }

        public Tween<TContext, TValue> Build()
        {
            Tween<TContext, TValue>.Create(ref this, out var tween);
            return tween;
        }

        public Tween<TContext, TValue> Play()
        {
            Tween<TContext, TValue>.Create(ref this, out var tween);
            tween.Play();
            return tween;
        }
    }
}

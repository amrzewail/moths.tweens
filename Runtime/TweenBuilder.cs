using Moths.Tweens.Memory;
using System;
using UnityEngine;

namespace Moths.Tweens
{
    public unsafe struct TweenBuilder<TContext, TValue>
    {
        internal TContext Context { get; private set; }
        internal TValue StartValue { get; private set; }
        internal TValue EndValue { get; private set; }
        internal float Duration { get; private set; }
        internal float Delay { get; private set; }
        internal delegate*<TValue, TValue, float, Ease, TValue> Updater { get; private set; }
        internal Ptr<CancellationToken> Cts { get; private set; }
        internal Ease Ease { get; private set; }
        internal AnimationCurve Curve { get; private set; }
        internal UpdateType UpdateType { get; private set; }
        internal UnityEngine.Object Obj { get; private set; }
        internal Action<TContext, TValue> OnValueChange { get; private set; }
        internal Action<TContext> OnComplete { get; private set; }

        public TweenBuilder<TContext, TValue> SetStartValue(TValue startValue) { StartValue = startValue; return this; }
        public TweenBuilder<TContext, TValue> SetEndValue(TValue endValue) { EndValue = endValue; return this; }
        public TweenBuilder<TContext, TValue> SetDuration(float duration) { Duration = duration; return this; }
        public TweenBuilder<TContext, TValue> SetUpdater(delegate*<TValue, TValue, float, Ease, TValue> updater) { Updater = updater; return this; }
        public TweenBuilder<TContext, TValue> SetContext(TContext context) { Context = context; return this; }
        public TweenBuilder<TContext, TValue> SetOnValueChange(Action<TContext, TValue> onValueChange) { OnValueChange = onValueChange; return this; }
        public TweenBuilder<TContext, TValue> SetOnComplete(Action<TContext> onComplete) { OnComplete = onComplete; return this; }
        public TweenBuilder<TContext, TValue> SetEase(Ease ease) { Ease = ease; return this; }
        public TweenBuilder<TContext, TValue> SetCurve(AnimationCurve curve) { Curve = curve; return this; }
        public TweenBuilder<TContext, TValue> SetUpdateType(UpdateType updateType) { UpdateType = updateType; return this; }
        public TweenBuilder<TContext, TValue> SetLink(UnityEngine.Object obj) { Obj = obj; return this; }
        public TweenBuilder<TContext, TValue> SetDelay(float delay) { Delay = delay; return this; }
        public TweenBuilder<TContext, TValue> SetCancellationToken(ref CancellationToken token) { Cts = new Ptr<CancellationToken>(ref token); return this; }


        public Tween<TContext, TValue> Play()
        {
            var tween = Tween<TContext, TValue>.Create(this);
            return tween.Play();
        }
    }
}
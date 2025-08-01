using Moths.Tweens.Memory;
using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Moths.Tweens 
{ 
    public unsafe partial struct Tween<TContext, TValue>
    {
        private static bool _wasPlaying = false;

        private static void UpdateTween(int index)
        {
            ref TweenInstance tween = ref Tween.Tweens.GetRef<TweenInstance>(index);

            if (!tween.isAllocated) return;

            if (!tween.shared.isPlaying) return;

            tween.Update(false);
        }

        private static void CancelWithLink(int index, object link, bool complete)
        {
            ref TweenInstance tween = ref Tween.Tweens.GetRef<TweenInstance>(index);

            if (!tween.isAllocated) return;
            if (!tween.shared.hasLink || !tween.shared.link.IsAllocated) return;

            var tweenLink = tween.shared.link.Value;

            if (tweenLink == link && complete)
            {
                CompleteTween(index, true);
            }
            else
            {
                CancelTween(index);
            }
        }

        public static void Create(ref TweenBuilder<TContext, TValue> builder, out Tween<TContext, TValue> tween)
        {
            tween = new Tween<TContext, TValue>();

            SharedData shared = new SharedData();

            shared.startValue = builder.StartValue;
            shared.endValue = builder.EndValue;
            shared.updater = builder.Updater;
            shared.updateType = builder.UpdateType;
            shared.ease = builder.Ease;

            shared.duration = builder.Duration;
            shared.time = -builder.Delay;

            shared.cts = builder.Cts;
            shared.hasCancellation = !shared.cts.IsNull();
            if (shared.hasCancellation)
            {
                shared.cancellationState = new Ptr<CancellationToken.State>(shared.cts.Pointer->Create());
                shared.cancellationState.Pointer->count++;
            }

            shared.context = new(builder.Context);
            shared.curve = new(builder.Curve);
            if (builder.Link != null) shared.link = new(builder.Link);
            shared.hasLink = builder.Link != null;
            shared.onValueChange = new(builder.OnValueChange);
            shared.onComplete = new(builder.OnComplete);

            int index = Tween.Tweens.Allocate();
            shared.tweenIndex = index;
            var instance = new TweenInstance
            {
                shared = shared,
                isAllocated = true
            };

            Tween.Tweens.Set(index, ref instance, 0);
            Tween.RegisterUpdate(index, ((int)shared.updateType & 1) > 0, &UpdateTween, &CancelWithLink);

            tween._tweenIndex = index;
        }

        private static void ResumeTween(int index)
        {
            if (index < 0) return;
            ref var tween = ref Tween.Tweens.GetRef<TweenInstance>(index);
            if (!tween.isAllocated) return;
            tween.shared.isPaused = false;
        }

        private static void RemoveTween(int index)
        {
            ref var tween = ref Tween.Tweens.GetRef<TweenInstance>(index);
            tween.isAllocated = false;
            tween.shared.Dispose();

            Tween.Tweens.Free(index);
            Tween.UnregisterUpdate(index);
        }

        private static void CompleteTween(int index, bool update)
        {
            Tween.Tweens.Get<TweenInstance>(index, out var tween);
            var shared = tween.shared;
            if (update)
            {
                tween.Update(true);
            }
            if (shared.onComplete.IsAllocated && shared.context.IsAllocated)
            {
                var complete = (Action<TContext>)shared.onComplete.Value;
                complete?.Invoke((TContext)shared.context.Value);
            }
            CancelTween(index);
        }

        private static void CancelTween(int index)
        {
            RemoveTween(index);
        }

    }
}
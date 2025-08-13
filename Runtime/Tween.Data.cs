using Moths.Tweens.Memory;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Moths.Tweens
{
    public unsafe partial struct Tween<TContext, TValue>
    {
        internal struct TweenInstance
        {
            public SharedData shared;
            public bool isAllocated;

            public unsafe void Update(bool complete)
            {
                if (shared.hasLink && shared.link.IsAllocated && shared.link == null)
                {
                    CancelTween(shared.tweenIndex);
                    return;
                }

                if (!shared.Update(out var canceled, complete))
                {
                    if (canceled)
                    {
                        CancelTween(shared.tweenIndex);
                    }
                    return;
                }

                float value = shared.value;

                if (shared.curve.IsAllocated)
                {
                    var curve = shared.curve.Value;
                    if (curve != null) value = curve.Evaluate(value);
                }

                shared.Ease(value);

                if (shared.onValueChange.IsAllocated && shared.context.IsAllocated)
                {
                    var valueChange = (Action<TContext, TValue>)shared.onValueChange.Value;
                    valueChange?.Invoke((TContext)shared.context.Value, shared.easedValue);
                }

                if (shared.time >= shared.duration && !complete) CompleteTween(shared.tweenIndex, false);
            }
        }

        internal struct SharedData
        {
            public int tweenIndex;

            public float time;
            public float value;
            public TValue startValue;
            public TValue endValue;
            public TValue easedValue;

            public float duration;
            public Ease ease;

            public delegate*<TValue, TValue, float, Ease, TValue> updater;
            public UpdateType updateType;

            public bool hasLink;
            public ManagedHeap<object> context;
            public ManagedHeap<AnimationCurve> curve;
            public ManagedHeap<object> link;
            public ManagedHeap<object> onValueChange;
            public ManagedHeap<object> onComplete;

            public bool hasCancellation;
            public Ptr<CancellationToken> cts;
            public Ptr<CancellationToken.State> cancellationState;

            public bool awaitingPlay;
            public bool isPlaying;
            public bool isPaused;
            public bool isCanceled;

            public bool Update(out bool canceled, bool complete)
            {
                if (canceled = isCanceled) return false;
                if (canceled = !cancellationState.IsNull() && cancellationState.Pointer->isCancelled) return false;

                if (complete)
                {
                    value = 1;
                    return true;
                }

                if (isPaused) return false;

                float deltaTime = updateType switch
                {
                    UpdateType.Update => Tweener.DeltaTime,
                    UpdateType.FixedUpdate => Tweener.DeltaTime,
                    UpdateType.UnscaledUpdate => Tweener.UnscaledDeltaTime,
                    UpdateType.UnscaledFixedUpdate => Tweener.UnscaledDeltaTime,
                    _ => Tweener.DeltaTime
                };

                time += deltaTime;

                // wait for delay
                if (time < 0) return false;

                value = duration > 0 ? Mathf.Clamp01(time / duration) : 1;

                return true;
            }

            public void Ease(float value)
            {
                easedValue = updater(startValue, endValue, value, ease);
            }

            public void Dispose()
            {
                context.Dispose();
                curve.Dispose();
                link.Dispose();
                onValueChange.Dispose();
                onComplete.Dispose();

                if (!hasCancellation) return;
                if (cancellationState.IsNull()) return;
                cancellationState.Pointer->count--;
                if (cancellationState.Pointer->count == 0)
                {
                    if (!cts.IsNull()) cts.Pointer->Dispose();
                }
            }
        };

    }
}
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
            public bool isAllocated;
            public bool isAwaitingPlay;
            public bool isStarted;
            public ManagedData managed;
            public SharedData shared;

            public unsafe void Update()
            {
                if (managed.hasLink && managed.obj == null)
                {
                    CancelTween(shared.tweenIndex);
                    return;
                }

                if (!shared.Update(out var canceled))
                {
                    if (canceled)
                    {
                        CancelTween(shared.tweenIndex);
                    }
                    return;
                }

                //float value = shared.value;

                //if (managed.curve != null)
                //{
                //    var curve = managed.curve;
                //    if (curve != null ) value = curve.Evaluate(value);
                //}

                managed.onValueChange?.Invoke(managed.context, shared.easedValue);

                if (shared.time >= shared.duration) CompleteTween(shared.tweenIndex);
            }
        }

        internal struct SharedData
        {
            public int tweenIndex;

            public float time;
            public float duration;

            public float value;

            public TValue startValue;
            public TValue endValue;
            public TValue easedValue;
            public delegate*<TValue, TValue, float, Ease, TValue> updater;
            public Ease ease;
            public UpdateType updateType;

            public Ptr<CancellationToken> cts;
            public Ptr<CancellationToken.State> cancellationState;

            public bool awaitingPlay;
            public bool isPlaying;
            public bool isPaused;
            public bool isCanceled;

            public bool Update(out bool canceled)
            {
                if (canceled = isCanceled) return false;
                if (canceled = !cancellationState.IsNull() && cancellationState.Pointer->isCancelled) return false;
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

                easedValue = updater(startValue, endValue, value, ease);

                return true;
            }


            public void Dispose()
            {
                if (!cancellationState.IsNull())
                {
                    cancellationState.Pointer->count--;
                    if (cancellationState.Pointer->count == 0)
                    {
                        if (!cts.IsNull()) cts.Pointer->Dispose();
                    }
                }
            }
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

    }
}
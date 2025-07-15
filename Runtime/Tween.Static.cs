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
        private const int LENGTH = 4096;

        private static Allocator<SharedData> Allocator;

        private static int _allocatedTweensCount = 0;
        private static int _startedTweensCount = 0;
        private static TweenInstance[] _tweens = new TweenInstance[LENGTH];

        private static readonly Action _updateAction = UpdateTweens;
        private static int _updateIndex;
        private static bool _wasPlaying = false;

        private static unsafe void UpdateTweens()
        {
            if (!Application.isPlaying)
            {
                if (_wasPlaying)
                {
                    for (int i = 0; i < _tweens.Length; i++)
                    {
                        if (!_tweens[i].isAllocated) continue;
                        _tweens[i].shared.Dispose();
                    }
                    _tweens = new TweenInstance[LENGTH];
                    _startedTweensCount = 0;
                    Allocator.FreeAll();
                }
                if (_tweens.Length != LENGTH || _wasPlaying)
                {
                    _tweens = new TweenInstance[LENGTH];
                }

                _wasPlaying = false;
                return;
            }

            _wasPlaying = true;
            int length = _tweens.Length;
            for (int i = 0; i < length; i++)
            {
                if (!_tweens[i].isAllocated) continue;
                if (!_tweens[i].isStarted)
                {
                    if (!_tweens[i].isAwaitingPlay) continue;
                    StartTween(i);
                    if (!_tweens[i].shared.cancellationState.IsNull()) _tweens[i].shared.cancellationState.Pointer->count++;
                }

                _tweens[i].Update();
            }
        }

        public static Tween<TContext, TValue> Create(TweenBuilder<TContext, TValue> builder)
        {
            var tween = new Tween<TContext, TValue>();

            SharedData shared = new SharedData();

            shared.startValue = builder.StartValue;
            shared.endValue = builder.EndValue;
            shared.updater = builder.Updater;
            shared.updateType = builder.UpdateType;
            shared.ease = builder.Ease;

            shared.duration = builder.Duration;
            shared.time = -builder.Delay;

            //tween._cts = builder.Cts;
            //if (!tween._cts.IsNull()) tween._cancellationState = tween._cts.Pointer->Create();

            ManagedData managed = new ManagedData();
            managed.context = builder.Context;
            managed.curve = builder.Curve;
            managed.obj = builder.Obj;
            managed.hasLink = managed.obj != null;
            managed.onValueChange = builder.OnValueChange;
            managed.onComplete = builder.OnComplete;

            tween._tweenIndex = AllocateTween(shared, managed);

            return tween;
        }

        private static int AllocateTween(SharedData data, ManagedData managed)
        {
            int length = _tweens.Length;
            for (int i = _allocatedTweensCount; i < _allocatedTweensCount + length; i++)
            {
                int index = i % length;
                if (_tweens[index].isAllocated) continue;
                data.tweenIndex = index;
                _tweens[index] = new TweenInstance
                {
                    shared = data,
                    managed = managed,
                    isStarted = false,
                    isAwaitingPlay = false,
                    isAllocated = true
                };
                _allocatedTweensCount++;

                if (_allocatedTweensCount == 1)
                {
                    _updateIndex = Tweener.SubscribeUpdate(_updateAction);
                }

                return index;
            }

            // No free slot found: grow array
            int oldLength = _tweens.Length;
            int newLength = oldLength * 2;

            var newArray = new TweenInstance[newLength];
            for (int i = 0; i < oldLength; i++)
                newArray[i] = _tweens[i];

            _tweens = newArray;

            // Now retry allocation at the first free slot in extended space
            _tweens[oldLength] = new TweenInstance
            {
                shared = data,
                managed = managed,
                isStarted = false,
                isAwaitingPlay = false,
                isAllocated = true
            };

            _allocatedTweensCount++;

            if (_allocatedTweensCount == 1)
            {
                _updateIndex = Tweener.SubscribeUpdate(_updateAction);
            }

            return oldLength;
        }

        private static void StartTween(int index)
        {
            if (index < 0) return;
            var tween = _tweens[index];
            if (!tween.isAllocated) return;
            tween.isStarted = true;
            _tweens[index] = tween;
            _startedTweensCount++;
        }

        private static void RemoveTween(int index)
        {
            if (_tweens == null) return;
            _tweens[index] = default;
            _startedTweensCount--;
            _allocatedTweensCount--;
            if (_allocatedTweensCount == 0)
            {
                Tweener.UnsubscribeUpdate(_updateIndex);
            }
        }

        private static void CompleteTween(int index)
        {
            if (_tweens == null) return;
            var tween = _tweens[index];
            var managed = tween.managed;
            managed.onComplete?.Invoke(managed.context);
            CancelTween(index);
        }

        private static void CancelTween(int index)
        {
            RemoveTween(index);
        }

    }
}
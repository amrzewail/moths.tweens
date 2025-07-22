using Moths.Tweens.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Moths.Tweens 
{ 
    public unsafe partial struct Tween<TContext, TValue>
    {
        private const int CAPACITY = 1024;

        private static int _allocatedTweensCount = 0;
        private static int _startedTweensCount = 0;

        private static DynamicArray<TweenInstance> _tweens;

        private static readonly Action _updateAction = UpdateTweens;
        private static int _updateIndex;
        private static bool _wasPlaying = false;

        private static unsafe void UpdateTweens()
        {
            if (!Application.isPlaying)
            {
                if (_wasPlaying)
                {
                    if (_tweens.IsInitialized)
                    {
                        for (int i = 0; i < _tweens.Length; i++)
                        {
                            if (!_tweens[i].isAllocated) continue;
                            _tweens[i].shared.Dispose();
                            _tweens[i].managed.Dispose();
                        }
                        _tweens.Dispose();
                        _tweens = default;
                    }

                    _startedTweensCount = 0;
                    _allocatedTweensCount = 0;
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

            //var job = new TweenJob();

            //fixed (DynamicArray<TweenInstance>* ptr = &_tweens)
            //{
            //    job.tweensPtr = ptr;
            //    JobHandle handle = job.Schedule(length, 64);
            //    handle.Complete();
            //}

            //for (int i = 0; i < length; i++)
            //{
            //    if (!_tweens[i].isAllocated) continue;

            //    if (!_tweens[i].isStarted)
            //    {
            //        if (!_tweens[i].isAwaitingPlay) continue;
            //        StartTween(i);
            //        if (!_tweens[i].shared.cancellationState.IsNull()) _tweens[i].shared.cancellationState.Pointer->count++;
            //        continue;
            //    }

            //    if (_tweens[i].isCancelled)
            //    {
            //        CancelTween(i);
            //        continue;
            //    }

            //    if (_tweens[i].shared.time < 0) continue;

            //    //var managed = _tweens[i].managed.Value;
            //    var shared = _tweens[i].shared;

            //    _tweens[i].managed.Value.onValueChange?.Invoke(_tweens[i].managed.Value.context, _tweens[i].shared.easedValue);
            //    if (_tweens[i].shared.time >= _tweens[i].shared.duration) CompleteTween(_tweens[i].shared.tweenIndex);
            //}

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

            shared.cts = builder.Cts;
            shared.hasCancellation = !shared.cts.IsNull();
            if (shared.hasCancellation) shared.cancellationState = new Ptr<CancellationToken.State>(shared.cts.Pointer->Create());

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
            _tweens.Create(CAPACITY);

            int length = _tweens.Length;
            for (int i = _allocatedTweensCount; i < _allocatedTweensCount + length; i++)
            {
                int index = i % length;
                if (_tweens[index].isAllocated) continue;
                data.tweenIndex = index;
                _tweens[index] = new TweenInstance
                {
                    shared = data,
                    managed = new(managed),
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

            _tweens.Resize(_tweens.Length * 2);

            return AllocateTween(data, managed);
        }

        private static void StartTween(int index)
        {
            if (index < 0) return;
            if (!_tweens.IsInitialized) return;
            var tween = _tweens[index];
            if (!tween.isAllocated) return;
            tween.isStarted = true;
            _tweens[index] = tween;
            Interlocked.Increment(ref _startedTweensCount);
        }

        private static void RemoveTween(int index)
        {
            if (!_tweens.IsInitialized) return;
            _tweens[index].shared.Dispose();
            _tweens[index].managed.Dispose();
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
            if (!_tweens.IsInitialized) return;
            var tween = _tweens[index];
            var managed = tween.managed.Value;
            managed.onComplete?.Invoke(managed.context);
            CancelTween(index);
        }

        private static void CancelTween(int index)
        {
            RemoveTween(index);
        }

    }
}
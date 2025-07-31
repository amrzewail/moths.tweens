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
    internal unsafe class Tween
    {
        internal unsafe struct TweenUpdate
        {
            public int tweenIndex;
            public delegate*<int, void> updater;
        }

        const int CAPACITY = 1024 * 10;

        public static GenericArray Tweens;

        private static int _tweenUpdatesCount;
        private static TweenUpdate[] _tweenUpdates;
        private static Stack<int> _freeIndices;

        public static int Count => _tweenUpdatesCount;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Tweens = new GenericArray(CAPACITY, 128);
            _tweenUpdatesCount = 0;
            _tweenUpdates = new TweenUpdate[CAPACITY];
            _freeIndices = new Stack<int>(CAPACITY);
            for (int i = 0; i < CAPACITY; i++) _freeIndices.Push(i);
        }

        public static void Update()
        {
            for (int i = 0; i < _tweenUpdates.Length; i++)
            {
                if (_tweenUpdates[i].updater == null) continue;
                _tweenUpdates[i].updater(_tweenUpdates[i].tweenIndex);
            }
        }

        public static void RegisterUpdate(delegate*<int, void> update, int tweenIndex)
        {
            int index = _freeIndices.Pop();
            _tweenUpdates[index].updater = update;
            _tweenUpdates[index].tweenIndex = tweenIndex;
            _tweenUpdatesCount++;
        }

        public static void UnregisterUpdate(int index)
        {
            _freeIndices.Push(index);
            _tweenUpdates[index] = default;
            _tweenUpdatesCount--;
        }
    }


    public unsafe partial struct Tween<TContext, TValue>
    {
        private static bool _wasPlaying = false;

        //private static unsafe void UpdateTweens()
        //{
        //    if (!Application.isPlaying)
        //    {
        //        if (_wasPlaying)
        //        {
        //            if (_tweens.IsInitialized)
        //            {
        //                for (int i = 0; i < _tweens.Length; i++)
        //                {
        //                    if (!_tweens[i].isAllocated) continue;
        //                    _tweens[i].shared.Dispose();
        //                    _tweens[i].managed.Dispose();
        //                }
        //                _tweens.Dispose();
        //                _tweens = default;
        //            }

        //            _startedTweensCount = 0;
        //            _allocatedTweensCount = 0;
        //        }

        //        _wasPlaying = false;
        //        return;
        //    }

        //    _wasPlaying = true;
        //    int length = _tweens.Length;
        //    for (int i = 0; i < length; i++)
        //    {
        //        if (!_tweens[i].isAllocated) continue;
        //        if (!_tweens[i].isStarted)
        //        {
        //            if (!_tweens[i].isAwaitingPlay) continue;
        //            StartTween(i);
        //            if (!_tweens[i].shared.cancellationState.IsNull()) _tweens[i].shared.cancellationState.Pointer->count++;
        //        }

        //        _tweens[i].Update();
        //    }
        //}

        private static void UpdateTween(int index)
        {
            TweenInstance tween = Tween.Tweens.Get<TweenInstance>(index);

            if (!tween.isAllocated) return;

            if (!tween.isStarted)
            {
                if (!tween.isAwaitingPlay) return;

                tween.isStarted = true;

                if (!tween.shared.cancellationState.IsNull()) tween.shared.cancellationState.Pointer->count++;
            }

            tween.Update();
            Tween.Tweens.Set(index, tween);
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
            int index = Tween.Tweens.Allocate();
            data.tweenIndex = index;
            var instance = new TweenInstance
            {
                shared = data,
                managed = new(managed),
                isStarted = false,
                isAwaitingPlay = false,
                isAllocated = true
            };

            Tween.Tweens.Set(index, instance);
            Tween.RegisterUpdate(&UpdateTween, index);

            return index;
        }

        private static void ResumeTween(int index)
        {
            var tween = Tween.Tweens.Get<TweenInstance>(index);
            if (index < 0) return;
            if (!tween.isAllocated) return;
            if (tween.shared.isPaused) tween.shared.isPaused = false;
            if (tween.shared.isPlaying) return;
            tween.shared.isPlaying = true;
            tween.isStarted = true;
            if (!tween.shared.cancellationState.IsNull()) tween.shared.cancellationState.Pointer->count++;
            Tween.Tweens.Set(index, tween);
        }

        private static void RemoveTween(int index)
        {
            var tween = Tween.Tweens.Get<TweenInstance>(index);
            tween.shared.Dispose();
            tween.managed.Dispose();

            Tween.Tweens.Free(index);
            Tween.UnregisterUpdate(index);
        }

        private static void CompleteTween(int index)
        {
            var tween = Tween.Tweens.Get<TweenInstance>(index);
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
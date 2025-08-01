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
            public bool isFixedUpdate;
            public delegate*<int, void> updater;
            public delegate*<int, object, bool, void> linkCanceller;
        }

        const int CAPACITY = 1024 * 4;

        public static GenericArray Tweens;

        private static int _tweenUpdatesCount;
        private static TweenUpdate[] _tweenUpdates;
        private static Stack<int> _freeIndices;

        public static int Count => _tweenUpdatesCount;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Tweens = new GenericArray(CAPACITY, 256);
            _tweenUpdatesCount = 0;
            _tweenUpdates = new TweenUpdate[CAPACITY];
            _freeIndices = new Stack<int>(CAPACITY);
            for (int i = 0; i < CAPACITY; i++) _freeIndices.Push(i);
        }

        ~Tween()
        {
            Tweens.Dispose();
        }

        public static void Update()
        {
            for (int i = 0; i < _tweenUpdates.Length; i++)
            {
                if (_tweenUpdates[i].isFixedUpdate) continue;
                if (_tweenUpdates[i].updater == null) continue;
                _tweenUpdates[i].updater(_tweenUpdates[i].tweenIndex);
            }
        }

        public static void FixedUpdate()
        {
            for (int i = 0; i < _tweenUpdates.Length; i++)
            {
                if (!_tweenUpdates[i].isFixedUpdate) continue;
                if (_tweenUpdates[i].updater == null) continue;
                _tweenUpdates[i].updater(_tweenUpdates[i].tweenIndex);
            }
        }

        public static void CancelWithLink(object link, bool complete)
        {
            if (link == null) return;

            for (int i = 0; i < _tweenUpdates.Length; i++)
            {
                if (_tweenUpdates[i].linkCanceller == null) continue;
                _tweenUpdates[i].linkCanceller(_tweenUpdates[i].tweenIndex, link, complete);
            }
        }


        public static void RegisterUpdate(int tweenIndex, bool fixedUpdate, delegate*<int, void> update, delegate*<int, object, bool, void> canceller)
        {
            int index = _freeIndices.Pop();
            _tweenUpdates[index].updater = update;
            _tweenUpdates[index].isFixedUpdate = fixedUpdate;
            _tweenUpdates[index].tweenIndex = tweenIndex;
            _tweenUpdates[index].linkCanceller = canceller;
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
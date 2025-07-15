using Moths.Tweens.Memory;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Moths.Tweens 
{ 
    public partial struct Tween<TContext, TValue>
    {
        private struct TweenInstance
        {
            public bool isAllocated;
            public bool isStarted;
            public ManagedData managed;
            public Ptr<SharedData> data;
        }

        private const int LENGTH = 4096;

        private static Allocator<SharedData> Allocator;

        private static int _allocatedTweensCount = 0;
        private static int _startedTweensCount = 0;
        private static TweenInstance[] _tweens = new TweenInstance[LENGTH];

        private static readonly Action _updateAction = UpdateTweens;
        private static int _updateIndex;

        private static unsafe void UpdateTweens()
        {
            if (!Application.isPlaying)
            {
                if (_startedTweensCount > 0)
                {
                    for (int i = 0; i < LENGTH; i++)
                    {
                        if (_tweens[i].isStarted) continue;
                        _tweens[i].data.Pointer->tween.Dispose();
                    }
                    _startedTweensCount = 0;
                    Allocator.FreeAll();
                }
                return;
            }

            for (int i = 0; i < LENGTH; i++)
            {
                if (!_tweens[i].isStarted) continue;
                _tweens[i].data.Pointer->tween.Update();
            }
        }

        private static int AllocateTween(Ptr<SharedData> data, ManagedData managed)
        {
            for (int i = _startedTweensCount; i < _startedTweensCount + LENGTH; i++)
            {
                int index = _startedTweensCount % LENGTH;
                if (_tweens[index].isAllocated) continue;
                _tweens[index] = new TweenInstance
                {
                    data = data,
                    managed = managed,
                    isStarted = false,
                    isAllocated = true
                };
                _allocatedTweensCount++;
                return index;
            }
            return -1;
        }

        private static void StartTween(int index)
        {
            if (index < 0) return;
            var tween = _tweens[index];
            if (!tween.isAllocated) return;
            tween.isStarted = true;
            _tweens[index] = tween;
            _startedTweensCount++;
            if (_startedTweensCount == 1)
            {
                _updateIndex = Tweener.SubscribeUpdate(_updateAction);
            }
        }

        private static void RemoveTween(int index)
        {
            if (_tweens == null) return;
            _tweens[index] = default;
            _startedTweensCount--;
            _allocatedTweensCount--;
            if (_startedTweensCount == 0)
            {
                Tweener.UnsubscribeUpdate(_updateIndex);
            }
        }

    }
}
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

        private const int LENGTH = 128;

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
                        _tweens[i].data.Pointer->tween.Dispose();
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
                if (!_tweens[i].isStarted) continue;
                _tweens[i].data.Pointer->tween.Update();
            }
        }

        private static int AllocateTween(Ptr<SharedData> data, ManagedData managed)
        {
            int length = _tweens.Length;
            for (int i = _allocatedTweensCount; i < _allocatedTweensCount + length; i++)
            {
                int index = _allocatedTweensCount % length;
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

            // No free slot found: grow array
            int oldLength = _tweens.Length;
            int newLength = oldLength * 4;

            var newArray = new TweenInstance[newLength];
            for (int i = 0; i < oldLength; i++)
                newArray[i] = _tweens[i];

            _tweens = newArray;

            // Now retry allocation at the first free slot in extended space
            _tweens[oldLength] = new TweenInstance
            {
                data = data,
                managed = managed,
                isStarted = false,
                isAllocated = true
            };

            _allocatedTweensCount++;
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
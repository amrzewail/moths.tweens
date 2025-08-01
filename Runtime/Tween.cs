using Moths.Tweens.Memory;
using System.Collections.Generic;
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
}
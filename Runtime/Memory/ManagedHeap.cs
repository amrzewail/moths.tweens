using System;
using System.Collections.Generic;

namespace Moths.Tweens.Memory
{
    public struct ManagedHeap<T> where T : class
    {
        private static T[] _pool;
        private static Stack<int> _freeIndices;
        private static int _capacity;

        private readonly int _index;

        public bool IsAllocated { get; private set; }

        static ManagedHeap()
        {
            _capacity = 64;
            _pool = new T[_capacity];
            _freeIndices = new Stack<int>(_capacity);
            for (int i = 0; i < _capacity; i++)
            {
                _freeIndices.Push(i);
            }
        }

        private ManagedHeap(int index)
        {
            IsAllocated = true;
            _index = index;
        }

        public ManagedHeap(T value)
        {
            this = Allocate(value);
        }

        public static ManagedHeap<T> Allocate(T value)
        {
            if (_freeIndices.Count == 0)
            {
                Expand();
            }
            var index = _freeIndices.Pop();
            _pool[index] = value;
            return new ManagedHeap<T>(index);
        }

        public ref T Value => ref _pool[_index];

        public void Dispose()
        {
            if (!IsAllocated) return;

            IsAllocated = false;
            _pool[_index] = null;
            _freeIndices.Push(_index);
        }

        private static void Expand()
        {
            int newCapacity = _capacity * 2;

            Array.Resize(ref _pool, newCapacity);

            for (int i = _capacity; i < newCapacity; i++) _freeIndices.Push(i);

            _capacity = newCapacity;
        }
    }
}
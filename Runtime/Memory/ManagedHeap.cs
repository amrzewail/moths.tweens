using System;

namespace Moths.Tweens.Memory
{
    public struct ManagedHeap<T>
    {
        private static (T obj, bool occupied)[] _pool;
        private static int _capacity;
        private static int _count;

        private readonly int _index;

        static ManagedHeap()
        {
            _capacity = 64;
            _pool = new (T, bool)[_capacity];
        }

        private ManagedHeap(int index)
        {
            _index = index;
        }

        public ManagedHeap(T value)
        {
            this = Allocate(value);
        }

        public static ManagedHeap<T> Allocate(T value)
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (!_pool[i].occupied)
                {
                    _pool[i] = (value, true);
                    _count++;
                    return new ManagedHeap<T>(i);
                }
            }

            Expand();
            return Allocate(value); // Try again after expanding
        }

        public ref T Value => ref _pool[_index].obj;

        public bool IsAlive => _index >= 0 && _index < _capacity && _pool[_index].occupied;

        public void Dispose()
        {
            if (_index < 0 || _index >= _capacity) return;
            if (!_pool[_index].occupied) return;

            _pool[_index] = (default, false);
            _count--;
        }

        private static void Expand()
        {
            int newCapacity = _capacity * 2;

            Array.Resize(ref _pool, newCapacity);

            _capacity = newCapacity;
        }
    }
}
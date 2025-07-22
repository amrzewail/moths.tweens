using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Moths.Tweens.Memory
{
    internal unsafe struct DynamicArray<T> where T : unmanaged
    {
        private T* _ptr;
        private int _length;

        public bool IsInitialized => _ptr != null;

        public int Length => _length;

        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                {
                    throw new System.IndexOutOfRangeException();
                }
                return ref _ptr[index];
            }
        }

        public void Set(int index, T value)
        {
            if (index < 0 || index >= _length)
            {
                throw new System.IndexOutOfRangeException();
            }
            _ptr[index] = value;
        }

        public void Create(int capacity)
        {
            if (IsInitialized) return;
            _ptr = Malloc(_length = capacity);
        }

        public void Resize(int newLength)
        {
            if (newLength < _length) return;

            // No free slot found: grow array
            int oldLength = _length;
            var newArray = Malloc(newLength);

            for (int i = 0; i < oldLength; i++)
                newArray[i] = _ptr[i];

            UnsafeUtility.Free(_ptr, Allocator.Persistent);
            _ptr = newArray;
            _length = newLength;
        }

        public void Dispose()
        {
            if (_ptr == null) return;
            UnsafeUtility.Free(_ptr, Allocator.Persistent);
        }

        private static T* Malloc(int length)
        {
            long totalSize = UnsafeUtility.SizeOf<T>() * (long)length;

            var ptr = (T*)UnsafeUtility.Malloc(
                totalSize,
                UnsafeUtility.AlignOf<T>(),
                Allocator.Persistent);

            UnsafeUtility.MemClear(ptr, totalSize);

            return ptr;
        }
    }
}

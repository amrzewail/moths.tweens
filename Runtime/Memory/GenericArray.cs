using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Moths.Tweens.Memory
{
    public unsafe struct GenericArray : IDisposable
    {
        private NativeArray<byte> _buffer;
        private Stack<int> _freeIndices;
        private int _slotSize;
        private int _count;

        public GenericArray(int capacity, int maxStructSize)
        {
            _slotSize = maxStructSize;
            _count = capacity;
            _buffer = new NativeArray<byte>(_slotSize * _count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            _freeIndices = new Stack<int>(capacity);
            for (int i = 0; i < capacity; i++) _freeIndices.Push(i);
        }

        public void Set<TValue>(int index, TValue value) where TValue : unmanaged
        {
            int offset = index * _slotSize;

            if (UnsafeUtility.SizeOf<TValue>() > _slotSize)
                throw new InvalidOperationException($"Type {typeof(TValue)} is too big for slot size {_slotSize}");

            void* ptr = (byte*)_buffer.GetUnsafePtr() + offset;
            UnsafeUtility.CopyStructureToPtr(ref value, ptr);
        }

        public TValue Get<TValue>(int index) where TValue : unmanaged
        {
            int offset = index * _slotSize;

            if (UnsafeUtility.SizeOf<TValue>() > _slotSize)
                throw new InvalidOperationException($"Type {typeof(TValue)} is too big for slot size {_slotSize}");

            void* ptr = (byte*)_buffer.GetUnsafeReadOnlyPtr() + offset;
            return UnsafeUtility.ReadArrayElement<TValue>(ptr, 0);
        }

        public int Allocate()
        {
            if (_freeIndices.Count == 0)
                throw new InvalidOperationException("GenericArray out of space.");

            return _freeIndices.Pop();
        }

        public void Free(int index)
        {
            _freeIndices.Push(index);
        }

        public void Dispose()
        {
            if (_buffer.IsCreated)
                _buffer.Dispose();
        }
    }

}
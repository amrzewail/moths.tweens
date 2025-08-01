using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Moths.Tweens.Memory
{
    public unsafe struct GenericArray : IDisposable
    {
        private byte* _buffer;
        private Stack<int> _freeIndices;
        private int _slotSize;
        private int _count;
        private Allocator _allocator;

        public GenericArray(int capacity, int maxStructSize, Allocator allocator = Allocator.Persistent)
        {
            _slotSize = maxStructSize;
            _count = capacity;
            _allocator = allocator;

            long totalSize = (long)_slotSize * _count;
            _buffer = (byte*)UnsafeUtility.Malloc(totalSize, 16, _allocator);
            if (_buffer == null)
                throw new InvalidOperationException("Failed to allocate memory for GenericArray.");

            _freeIndices = new Stack<int>(capacity);
            for (int i = 0; i < capacity; i++) _freeIndices.Push(i);
        }

        public void Set<TValue>(int index, ref TValue value, int offset = 0) where TValue : unmanaged
        {
            offset = offset + index * _slotSize;

            if (UnsafeUtility.SizeOf<TValue>() > _slotSize)
                throw new InvalidOperationException($"Type {typeof(TValue)} is too big for slot size {_slotSize}");

            void* ptr = _buffer + offset;
            UnsafeUtility.CopyStructureToPtr(ref value, ptr);
        }

        public void Get<TValue>(int index, out TValue value) where TValue : unmanaged
        {
            GetPtr<TValue>(index, out var ptr);
            value = UnsafeUtility.ReadArrayElement<TValue>(ptr, 0);
        }

        public ref TValue GetRef<TValue>(int index) where TValue : unmanaged
        {
            GetPtr<TValue>(index, out var ptr);
            return ref UnsafeUtility.AsRef<TValue>(ptr);
        }

        private unsafe void GetPtr<TValue>(int index, out void* ptr) where TValue : unmanaged
        {
            int offset = index * _slotSize;

            if (UnsafeUtility.SizeOf<TValue>() > _slotSize)
                throw new InvalidOperationException($"Type {typeof(TValue)} is too big for slot size {_slotSize}");

            ptr = _buffer + offset;
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
            if (_buffer != null)
            {
                UnsafeUtility.Free(_buffer, _allocator);
                _buffer = null;
            }
        }
    }
}

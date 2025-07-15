//#define ENABLE_LOGS

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Moths.Tweens.Memory
{
    /// <summary>
    /// Memory allocator that keeps track of allocated pointers to prevent memory leaks<br></br>
    /// It has methods resembling C 's malloc and free<br></br>
    /// Call Dispose to free all allocated memory and there is no need for this allocator anymore
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal unsafe struct Allocator<T> : IDisposable where T : unmanaged
    {
        private bool _isInitialized;
        private NativeParallelHashMap<int, Ptr<T>> _allocations;

        public void Initialize(int capacity = 128)
        {
            _allocations = new NativeParallelHashMap<int, Ptr<T>>(capacity, Allocator.Persistent);
            _isInitialized = true;
        }

        /// <summary>
        /// Free all blocks of memory allocated by this Allocator
        /// </summary>
        public void FreeAll()
        {
            if (_isInitialized)
            {
                foreach (var ptr in _allocations)
                {
                    UnsafeUtility.FreeTracked(ptr.Value, Allocator.Persistent);
                    Debug.Log($"[Allocator<{typeof(T).Name}>] Free allocated");
                }
                _allocations.Clear();
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Frees the block of memory allocated by its pointer
        /// </summary>
        /// <param name="ptr"></param>
        public void Free(Ptr<T> ptr)
        {
#if ENABLE_LOGS
            Debug.Log($"[Allocator<{typeof(T).Name}>] Free allocated");
#endif
            UnsafeUtility.FreeTracked(ptr, Allocator.Persistent);
            if (_isInitialized) _allocations.Remove(ptr);
        }


        /// <summary>
        /// Allocates a new block of memory and returns its pointer
        /// </summary>
        /// <returns></returns>
        public Ptr<T> Malloc()
        {
            T* m = (T*)UnsafeUtility.MallocTracked(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), Allocator.Persistent, 0);
            Ptr<T> ptr = new Ptr<T>(m);
            if (_isInitialized) _allocations.AsParallelWriter().TryAdd(ptr, ptr);
#if ENABLE_LOGS
            Debug.Log($"[Allocator<{typeof(T).Name}>] Allocated new");
#endif
            return ptr;
        }

        /// <summary>
        /// Returns true if this pointer was allocated with this Allocator and is not freed yet
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public bool IsAllocated(Ptr<T> ptr)
        {
            if (_isInitialized)
            {
                if (!_allocations.IsCreated) return false;
                return _allocations.ContainsKey(ptr);
            }
            return false;
        }


        /// <summary>
        /// Free all pointers and completely dispose this Allocator. It will be unusable after that.
        /// </summary>
        public void Dispose()
        {
            FreeAll();
            if (_isInitialized)
            {
                _allocations.Dispose();
            }
        }
    }
}
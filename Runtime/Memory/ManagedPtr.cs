using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Moths.Tweens.Memory
{
    public struct ManagedPtr<T> : IDisposable
    {
        private GCHandle _handle;

        public T Value => IsAllocated ? (T)_handle.Target : default;
        public bool IsAllocated { get; private set; }

        public bool IsAlive
        {
            get
            {
                if (!IsAllocated) return false;
                var target = _handle.Target;
                if (target == null) return false;
                if (target is UnityEngine.Object unityObj && unityObj == null) return false;
                return true;
            }
        }
        public ManagedPtr(T target)
        {
            IsAllocated = true;
            _handle = GCHandle.Alloc(target, GCHandleType.Normal);
        }

        public void Dispose()
        {
            if (!_handle.IsAllocated) return;
            _handle.Free();
        }
    }
}
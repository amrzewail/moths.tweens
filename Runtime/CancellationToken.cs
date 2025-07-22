using Moths.Tweens.Memory;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Moths.Tweens
{
    public unsafe struct CancellationToken : IDisposable
    {
        internal struct State
        {
            public bool isCancelled;
            public int count;
        }

        private bool _isCreated;
        private State* _state;

        internal unsafe State* Create()
        {
            if (_isCreated) return _state;
            _isCreated = true;
            _state = (State*)UnsafeUtility.MallocTracked(UnsafeUtility.SizeOf<State>(), UnsafeUtility.AlignOf<State>(), Unity.Collections.Allocator.Persistent, 0);
            _state->isCancelled = false;
            _state->count = 0;
            return _state;
        }
        public unsafe void Cancel()
        {
            if (!_isCreated) return;
            _isCreated = false;
            _state->isCancelled = true;
            UnsafeUtility.FreeTracked(_state, Unity.Collections.Allocator.Persistent);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
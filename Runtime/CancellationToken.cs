using Moths.Tweens.Memory;
using System;

namespace Moths.Tweens
{
    public struct CancellationToken : IDisposable
    {
        private static Allocator<State> Allocator;

        internal struct State
        {
            public bool isCancelled;
            public int count;
        }

        private bool _isCreated;
        private Ptr<State> _state;

        internal unsafe Ptr<State> Create()
        {
            if (_isCreated) return _state;
            _isCreated = true;
            _state = Allocator.Malloc();
            _state.Pointer->isCancelled = false;
            return _state;
        }
        public unsafe void Cancel()
        {
            if (!_isCreated) return;
            _isCreated = false;
            _state.Pointer->isCancelled = true;
            Allocator.Free(_state);
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
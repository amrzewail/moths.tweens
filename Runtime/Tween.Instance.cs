using Moths.Tweens.Memory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Tweens
{
    public enum UpdateType
    {
        Update,
        FixedUpdate,
        UnscaledUpdate,
        UnscaledFixedUpdate,
    };

    public unsafe partial struct Tween<TContext, TValue> 
        where TValue : unmanaged
        where TContext : class
    {
        private int _tweenIndex;
        private ref SharedData Shared
        {
            get
            {
                return ref Tween.Tweens.GetRef<TweenInstance>(_tweenIndex).shared;
            }
        }

        public bool IsPlaying => Shared.isPlaying;
        public bool IsPaused => IsPlaying ? Shared.isPaused : false;
        public float Time => Mathf.Max(IsPlaying ? Shared.time : 0, 0);
        public float Duration => Shared.duration;
        public float Value => Duration > 0 ? Mathf.Clamp01(Time / Duration) : 1;
        public TValue EasedValue => Shared.updater(Shared.startValue, Shared.endValue, Value, Shared.ease);

        public void Play()
        {
            if (_tweenIndex < 0) return;

            ref var tween = ref Tween.Tweens.GetRef<TweenInstance>(_tweenIndex);

            if (!IsPlaying)
            {
                Shared.isPlaying = true;
                return;
            }

            ResumeTween(_tweenIndex);
        }

        public void Pause()
        {
            if (IsPaused) return;
            Shared.isPaused = true;
        }

        public void Cancel()
        {
            if (!IsPlaying) return;
            CancelTween(_tweenIndex);
        }

        public void Complete()
        {
            if (!IsPlaying) return;
            CompleteTween(_tweenIndex, true);
        }
    }
}
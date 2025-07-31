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

    public unsafe partial struct Tween<TContext, TValue> where TValue : unmanaged
    {
        private int _tweenIndex;
        private SharedData Shared
        {
            get => Tween.Tweens.Get<TweenInstance>(_tweenIndex).shared;
            set
            {
                var tween = Tween.Tweens.Get<TweenInstance>(_tweenIndex);
                tween.shared = value;
                Tween.Tweens.Set(_tweenIndex, tween);
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
            if (_tweenIndex == -1) return;

            var tween = Tween.Tweens.Get<TweenInstance>(_tweenIndex);
            if (!tween.isAwaitingPlay)
            {
                tween.isAwaitingPlay = true;
                Tween.Tweens.Set(_tweenIndex, tween);
                return;
            }

            ResumeTween(_tweenIndex);
        }

        public void Pause()
        {
            if (IsPaused) return;
            var shared = Shared;
            shared.isPaused = true;
            Shared = shared;
        }

        public void Cancel()
        {
            if (!IsPlaying) return;
            CancelTween(_tweenIndex);
        }

        public void Complete()
        {
            if (!IsPlaying) return;
            CompleteTween(_tweenIndex);
        }
    }
}
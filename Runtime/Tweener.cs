using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Moths.Tweens
{
    public static class Tweener
    {
        private struct TweenPlayerLoop { }

        internal static float DeltaTime;
        internal static float UnscaledDeltaTime;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, Update.ScriptRunBehaviourUpdate>(TweenUpdate);
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, FixedUpdate>(TweenFixedUpdate);
        }

        private static void TweenUpdate()
        {
            if (!Application.isPlaying) return;
            
            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            if (Tween.Count > 0)
            {
                Tween.Update();
            }
        }

        private static void TweenFixedUpdate()
        {
            if (!Application.isPlaying) return;

            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            if (Tween.Count > 0)
            {
                Tween.FixedUpdate();
            }
        }

        public static void CancelAll(object link)
        {
            Tween.CancelWithLink(link, false);
        }

        public static void CompleteAll(object link)
        {
            Tween.CancelWithLink(link, true);
        }

        public static unsafe TweenBuilder<TContext, TValue> Value<TContext, TValue>(TContext context, TValue startValue, TValue endValue, delegate*<TValue, TValue, float, Ease, TValue> updater) 
        where TValue : unmanaged
        where TContext : class
        {
            return new TweenBuilder<TContext, TValue>()
                .SetContext(context)
                .SetUpdater(updater)
                .SetStartValue(startValue)
                .SetEndValue(endValue)
                .SetDuration(1)
                .SetLink(context);
        }

        public static unsafe TweenBuilder<TContext, float> Value<TContext>(TContext context, float startValue, float endValue)
        where TContext : class
        {
            return Value(context, startValue, endValue, &Updaters.FloatUpdater.Update);
        }

        public static unsafe TweenBuilder<TContext, Vector3> Value<TContext>(TContext context, Vector3 startValue, Vector3 endValue)
        where TContext : class
        {
            return Value(context, startValue, endValue, &Updaters.Vector3Updater.Update);
        }

        public static unsafe TweenBuilder<TContext, Vector2> Value<TContext>(TContext context, Vector2 startValue, Vector2 endValue)
        where TContext : class
        {
            return Value(context, startValue, endValue, &Updaters.Vector2Updater.Update);
        }

        public static unsafe TweenBuilder<TContext, Quaternion> Value<TContext>(TContext context, Quaternion startValue, Quaternion endValue)
        where TContext : class
        {
            return Value(context, startValue, endValue, &Updaters.QuaternionUpdater.Update);
        }

        public static unsafe TweenBuilder<TContext, Color> Value<TContext>(TContext context, Color startValue, Color endValue)
        where TContext : class
        {
            return Value(context, startValue, endValue, &Updaters.ColorUpdater.Update);
        }
    }
}
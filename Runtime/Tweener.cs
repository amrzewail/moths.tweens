using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Moths.Tweens
{
    public static class Tweener
    {
        public struct TweenPlayerLoop { }

        internal static float DeltaTime;
        internal static float UnscaledDeltaTime;

        internal static Dictionary<int, Action> UpdateActions;

        public static event Action Update;
        public static event Action FixedUpdate;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, Update.ScriptRunBehaviourUpdate>(TweenUpdate);
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, FixedUpdate>(TweenFixedUpdate);

            UpdateActions = new Dictionary<int, Action>(256);

            Update = null;
            FixedUpdate = null;
        }

        private static void TweenUpdate()
        {
            if (!Application.isPlaying) return;

            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            Update?.Invoke();
        }

        private static void TweenFixedUpdate()
        {
            if (!Application.isPlaying) return;

            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            FixedUpdate?.Invoke();
        }

        public static unsafe Tween<TContext, TValue> Value<TContext, TValue>(TContext context, TValue startValue, TValue endValue, delegate*<TValue, TValue, float, Ease, TValue> updater)
        {
            return Tween<TContext, TValue>.Create(updater)
                .SetContext(context)
                .SetStartValue(startValue)
                .SetEndValue(endValue)
                .SetDuration(1);
        }

        public static unsafe Tween<TContext, float> Value<TContext>(TContext context, float startValue, float endValue)
        {
            return Value(context, startValue, endValue, &Updaters.FloatUpdater.Update);
        }

        public static unsafe Tween<TContext, Vector3> Value<TContext>(TContext context, Vector3 startValue, Vector3 endValue)
        {
            return Value(context, startValue, endValue, &Updaters.Vector3Updater.Update);
        }

        public static unsafe Tween<TContext, Vector2> Value<TContext>(TContext context, Vector2 startValue, Vector2 endValue)
        {
            return Value(context, startValue, endValue, &Updaters.Vector2Updater.Update);
        }

        public static unsafe Tween<TContext, Quaternion> Value<TContext>(TContext context, Quaternion startValue, Quaternion endValue)
        {
            return Value(context, startValue, endValue, &Updaters.QuaternionUpdater.Update);
        }

        public static unsafe Tween<TContext, Color> Value<TContext>(TContext context, Color startValue, Color endValue)
        {
            return Value(context, startValue, endValue, &Updaters.ColorUpdater.Update);
        }
    }
}
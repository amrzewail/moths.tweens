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

        //internal static Dictionary<int, Action> UpdateActions;

        internal static Action[] UpdatePool;
        internal static Action[] FixedUpdatePool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, Update.ScriptRunBehaviourUpdate>(TweenUpdate);
            PlayerLoopUtility.AddSystem<TweenPlayerLoop, FixedUpdate>(TweenFixedUpdate);

            //UpdateActions = new Dictionary<int, Action>(256);

            UpdatePool = new Action[32];
            FixedUpdatePool = new Action[32];
        }

        private static void TweenUpdate()
        {
            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < UpdatePool.Length; i++)
            {
                if (UpdatePool[i] == null) continue; 
                UpdatePool[i].Invoke();
            }
        }

        private static void TweenFixedUpdate()
        {
            DeltaTime = Time.deltaTime;
            UnscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < FixedUpdatePool.Length; i++)
            {
                if (FixedUpdatePool[i] == null) continue;
                FixedUpdatePool[i].Invoke();
            }
        }

        internal static int SubscribeUpdate(Action update)
        {
            for (int i = 0; i < UpdatePool.Length; i++)
            {
                if (UpdatePool[i] == null)
                {
                    UpdatePool[i] = update;
                    return i;
                }
            }
            return -1;
        }

        internal static int SubscribeFixedUpdate(Action fixedUpdate)
        {
            for (int i = 0; i < FixedUpdatePool.Length; i++)
            {
                if (FixedUpdatePool[i] == null)
                {
                    FixedUpdatePool[i] = fixedUpdate;
                    return i;
                }
            }
            return -1;
        }

        internal static void UnsubscribeUpdate(int index)
        {
            UpdatePool[index] = null;
        }

        internal static void UnsubscribeFixedUpdate(int index)
        {
            FixedUpdatePool[index] = null;
        }

        internal static void UnsubscribeFixedUpdate(Action update)
        {
        }

        public static unsafe TweenBuilder<TContext, TValue> Value<TContext, TValue>(TContext context, TValue startValue, TValue endValue, delegate*<TValue, TValue, float, Ease, TValue> updater) where TValue : unmanaged
        {
            return new TweenBuilder<TContext, TValue>()
                .SetUpdater(updater)
                .SetContext(context)
                .SetStartValue(startValue)
                .SetEndValue(endValue)
                .SetDuration(1);
        }

        public static unsafe TweenBuilder<TContext, float> Value<TContext>(TContext context, float startValue, float endValue)
        {
            return Value(context, startValue, endValue, &Updaters.FloatUpdater.Update);
        }

        public static unsafe TweenBuilder<TContext, Vector3> Value<TContext>(TContext context, Vector3 startValue, Vector3 endValue)
        {
            return Value(context, startValue, endValue, &Updaters.Vector3Updater.Update);
        }

        public static unsafe TweenBuilder<TContext, Vector2> Value<TContext>(TContext context, Vector2 startValue, Vector2 endValue)
        {
            return Value(context, startValue, endValue, &Updaters.Vector2Updater.Update);
        }

        public static unsafe TweenBuilder<TContext, Quaternion> Value<TContext>(TContext context, Quaternion startValue, Quaternion endValue)
        {
            return Value(context, startValue, endValue, &Updaters.QuaternionUpdater.Update);
        }

        public static unsafe TweenBuilder<TContext, Color> Value<TContext>(TContext context, Color startValue, Color endValue)
        {
            return Value(context, startValue, endValue, &Updaters.ColorUpdater.Update);
        }
    }
}
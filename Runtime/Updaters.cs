using UnityEngine;

namespace Moths.Tweens.Updaters
{
    internal static class FloatUpdater
    {
        public static float Update(float start, float end, float t, Ease ease)
        {
            return Mathf.LerpUnclamped(start, end, ease.Evaluate(t));
        }
    }

    internal static class Vector3Updater
    {
        public static Vector3 Update(Vector3 start, Vector3 end, float t, Ease ease)
        {
            t = ease.Evaluate(t);
            return Vector3.LerpUnclamped(start, end, t);
        }
    }

    internal static class Vector2Updater
    {
        public static Vector2 Update(Vector2 start, Vector2 end, float t, Ease ease)
        {
            return Vector2.LerpUnclamped(start, end, ease.Evaluate(t));
        }
    }

    internal static class QuaternionUpdater
    {
        public static Quaternion Update(Quaternion start, Quaternion end, float t, Ease ease)
        {
            return Quaternion.SlerpUnclamped(start, end, ease.Evaluate(t));
        }
    }

    internal static class ColorUpdater
    {
        public static Color Update(Color start, Color end, float t, Ease ease)
        {
            return Color.LerpUnclamped(start, end, ease.Evaluate(t));
        }
    }
}
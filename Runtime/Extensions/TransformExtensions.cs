using UnityEngine;

namespace Moths.Tweens.Extensions
{
    public static class TransformExtensions
    {
        public static Tween<Transform, Vector3> TweenPosition(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.position, to)
                .SetOnValueChange((t, v) => t.position = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Quaternion> TweenRotation(this Transform transform, Quaternion to)
        {
            return Tweener.Value(transform, transform.rotation, to)
                .SetOnValueChange((t, v) => t.rotation = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Vector3> TweenEulerAngles(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.eulerAngles, to)
                .SetOnValueChange((t, v) => t.eulerAngles = v)
                .SetLink(transform);
        }
        public static Tween<Transform, Vector3> TweenLocalPosition(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localPosition, to)
                .SetOnValueChange((t, v) => t.localPosition = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Quaternion> TweenLocalRotation(this Transform transform, Quaternion to)
        {
            return Tweener.Value(transform, transform.localRotation, to)
                .SetOnValueChange((t, v) => t.localRotation = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Vector3> TweenLocalScale(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localScale, to)
                .SetOnValueChange((t, v) => t.localScale = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Vector3> TweenLocalEulerAngles(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localEulerAngles, to)
                .SetOnValueChange((t, v) => t.localEulerAngles = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Vector3> TweenDirection(this Transform transform, Vector3 direction)
        {
            return Tweener.Value(transform, transform.position, transform.position + direction)
                .SetOnValueChange((t, v) => t.position = v)
                .SetLink(transform);
        }

        public static Tween<Transform, Vector3> TweenLocalDirection(this Transform transform, Vector3 localDirection)
        {
            return Tweener.Value(transform, transform.localPosition, transform.localPosition + transform.InverseTransformDirection(localDirection))
                .SetOnValueChange((t, v) => t.localPosition = v)
                .SetLink(transform);
        }
    }
}
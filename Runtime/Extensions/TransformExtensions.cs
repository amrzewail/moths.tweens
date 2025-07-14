using System;
using UnityEngine;

namespace Moths.Tweens.Extensions
{
    public static class TransformExtensions
    {
        // ==== Cached Callbacks ====

        private static readonly Action<Transform, Vector3> _setPosition = (t, v) => t.position = v;
        private static readonly Action<Transform, Quaternion> _setRotation = (t, v) => t.rotation = v;
        private static readonly Action<Transform, Vector3> _setEulerAngles = (t, v) => t.eulerAngles = v;
        private static readonly Action<Transform, Vector3> _setLocalPosition = (t, v) => t.localPosition = v;
        private static readonly Action<Transform, Quaternion> _setLocalRotation = (t, v) => t.localRotation = v;
        private static readonly Action<Transform, Vector3> _setLocalScale = (t, v) => t.localScale = v;
        private static readonly Action<Transform, Vector3> _setLocalEulerAngles = (t, v) => t.localEulerAngles = v;

        // ==== Tween Extension Methods ====

        public static TweenBuilder<Transform, Vector3> TweenPosition(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.position, to)
                .SetOnValueChange(_setPosition)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Quaternion> TweenRotation(this Transform transform, Quaternion to)
        {
            return Tweener.Value(transform, transform.rotation, to)
                .SetOnValueChange(_setRotation)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenEulerAngles(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.eulerAngles, to)
                .SetOnValueChange(_setEulerAngles)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenLocalPosition(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localPosition, to)
                .SetOnValueChange(_setLocalPosition)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Quaternion> TweenLocalRotation(this Transform transform, Quaternion to)
        {
            return Tweener.Value(transform, transform.localRotation, to)
                .SetOnValueChange(_setLocalRotation)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenLocalScale(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localScale, to)
                .SetOnValueChange(_setLocalScale)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenLocalEulerAngles(this Transform transform, Vector3 to)
        {
            return Tweener.Value(transform, transform.localEulerAngles, to)
                .SetOnValueChange(_setLocalEulerAngles)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenDirection(this Transform transform, Vector3 direction)
        {
            return Tweener.Value(transform, transform.position, transform.position + direction)
                .SetOnValueChange(_setPosition)
                .SetLink(transform);
        }

        public static TweenBuilder<Transform, Vector3> TweenLocalDirection(this Transform transform, Vector3 localDirection)
        {
            var target = transform.localPosition + transform.InverseTransformDirection(localDirection);
            return Tweener.Value(transform, transform.localPosition, target)
                .SetOnValueChange(_setLocalPosition)
                .SetLink(transform);
        }
    }
}
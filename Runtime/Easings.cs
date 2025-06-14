using UnityEngine;

namespace Moths.Tweens
{
    public enum Ease
    {
        Linear,

        InSine,
        OutSine,
        InOutSine,

        InQuad,
        OutQuad,
        InOutQuad,

        InCubic,
        OutCubic,
        InOutCubic,

        InQuart,
        OutQuart,
        InOutQuart,

        InQuint,
        OutQuint,
        InOutQuint,

        InExpo,
        OutExpo,
        InOutExpo,

        InCirc,
        OutCirc,
        InOutCirc,

        InBack,
        OutBack,
        InOutBack,

        InElastic,
        OutElastic,
        InOutElastic,

        InBounce,
        OutBounce,
        InOutBounce,

        Flash,
        InFlash,
        OutFlash,
        InOutFlash,
    }

    public static class Easings
    {
        public static float Evaluate(this Ease ease, float t)
        {
            return ease switch
            {
                Ease.Linear => Linear(t),

                Ease.InSine => InSine(t),
                Ease.OutSine => OutSine(t),
                Ease.InOutSine => InOutSine(t),

                Ease.InQuad => InQuad(t),
                Ease.OutQuad => OutQuad(t),
                Ease.InOutQuad => InOutQuad(t),

                Ease.InCubic => InCubic(t),
                Ease.OutCubic => OutCubic(t),
                Ease.InOutCubic => InOutCubic(t),

                Ease.InQuart => InQuart(t),
                Ease.OutQuart => OutQuart(t),
                Ease.InOutQuart => InOutQuart(t),

                Ease.InQuint => InQuint(t),
                Ease.OutQuint => OutQuint(t),
                Ease.InOutQuint => InOutQuint(t),

                Ease.InExpo => InExpo(t),
                Ease.OutExpo => OutExpo(t),
                Ease.InOutExpo => InOutExpo(t),

                Ease.InCirc => InCirc(t),
                Ease.OutCirc => OutCirc(t),
                Ease.InOutCirc => InOutCirc(t),

                Ease.InBack => InBack(t),
                Ease.OutBack => OutBack(t),
                Ease.InOutBack => InOutBack(t),

                Ease.InElastic => InElastic(t),
                Ease.OutElastic => OutElastic(t),
                Ease.InOutElastic => InOutElastic(t),

                Ease.InBounce => InBounce(t),
                Ease.OutBounce => OutBounce(t),
                Ease.InOutBounce => InOutBounce(t),

                Ease.Flash => Flash(t),
                Ease.InFlash => InFlash(t),
                Ease.OutFlash => OutFlash(t),
                Ease.InOutFlash => InOutFlash(t),

                _ => t
            };
        }

        internal static float Linear(float t) => t;

        internal static float InSine(float t) => 1f - Mathf.Cos((t * Mathf.PI) / 2f);
        internal static float OutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2f);
        internal static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

        internal static float InQuad(float t) => t * t;
        internal static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        internal static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

        internal static float InCubic(float t) => t * t * t;
        internal static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        internal static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        internal static float InQuart(float t) => t * t * t * t;
        internal static float OutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
        internal static float InOutQuart(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;

        internal static float InQuint(float t) => t * t * t * t * t;
        internal static float OutQuint(float t) => 1f - Mathf.Pow(1f - t, 5f);
        internal static float InOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;

        internal static float InExpo(float t) => t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
        internal static float OutExpo(float t) => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        internal static float InOutExpo(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        internal static float InCirc(float t) => 1f - Mathf.Sqrt(1f - t * t);
        internal static float OutCirc(float t) => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f));
        internal static float InOutCirc(float t) => t < 0.5f
            ? (1f - Mathf.Sqrt(1f - 4f * t * t)) / 2f
            : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;

        internal static float InBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return c3 * t * t * t - c1 * t * t;
        }
        internal static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
        internal static float InOutBack(float t)
        {
            const float c2 = 1.70158f * 1.525f;
            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        internal static float InElastic(float t)
        {
            if (t == 0f || t == 1f) return t;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * ((2f * Mathf.PI) / 3f));
        }
        internal static float OutElastic(float t)
        {
            if (t == 0f || t == 1f) return t;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ((2f * Mathf.PI) / 3f)) + 1f;
        }
        internal static float InOutElastic(float t)
        {
            if (t == 0f || t == 1f) return t;
            const float c = (2f * Mathf.PI) / 4.5f;
            return t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c)) / 2f + 1f;
        }

        internal static float InBounce(float t) => 1f - OutBounce(1f - t);
        internal static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
        internal static float InOutBounce(float t) => t < 0.5f
            ? (1f - OutBounce(1f - 2f * t)) / 2f
            : (1f + OutBounce(2f * t - 1f)) / 2f;

        internal static float Flash(float t) => Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
        internal static float InFlash(float t) => Mathf.Sin(t * Mathf.PI * 2f) * t;
        internal static float OutFlash(float t) => Mathf.Sin(t * Mathf.PI * 2f) * (1f - t);
        internal static float InOutFlash(float t) => t < 0.5f
            ? Mathf.Sin(t * Mathf.PI * 4f) * t * 2f
            : Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * 2f;
    }

}
using UnityEngine;

namespace Moths.Tweens.Extensions
{
    public static class CanvasGroupExtensions
    {
        public static Tween<CanvasGroup, float> TweenAlpha(this CanvasGroup group, float to)
        {
            return Tweener.Value(group, group.alpha, to)
                .SetOnValueChange((group, alpha) => group.alpha = alpha)
                .SetLink(group);
        }
    }
}
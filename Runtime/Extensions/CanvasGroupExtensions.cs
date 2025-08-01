using System;
using UnityEngine;

namespace Moths.Tweens.Extensions
{
    public static class CanvasGroupExtensions
    {
        private static readonly Action<CanvasGroup, float> _setAlpha = (c, v) => c.alpha = v;

        public static TweenBuilder<CanvasGroup, float> TweenAlpha(this CanvasGroup group, float to)
        {
            return Tweener.Value(group, group.alpha, to)
                .SetOnValueChange(_setAlpha)
                .SetLink(group);
        }
    }
}
using Moths.Tweens.Memory;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Moths.Tweens 
{
    public unsafe partial struct Tween<TContext, TValue>
    {
        private unsafe struct TweenJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            public DynamicArray<TweenInstance>* tweensPtr;

            public void Execute(int index)
            {
                var tweens = *tweensPtr;

                if (!tweens[index].isAllocated) return;
                if (!tweens[index].isStarted) return;

                var managed = tweens[index].managed.Value;

                if (managed.hasLink && managed.obj == null)
                {
                    tweens[index].isCancelled = true;
                    return;
                }

                if (!tweens[index].shared.Update(out var canceled))
                {
                    if (canceled)
                    {
                        tweens[index].isCancelled = true;
                    }
                    return;
                }

                float value = tweens[index].shared.value;

                if (managed.curve != null)
                {
                    var curve = managed.curve;
                    if (curve != null) value = curve.Evaluate(value);
                }

                tweens[index].shared.Ease(value);
            }
        }
    }
}
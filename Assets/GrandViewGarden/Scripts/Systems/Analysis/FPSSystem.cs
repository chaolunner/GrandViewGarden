using System.Collections.Generic;
using UnityEngine.UI;
using UniEasy.ECS;
using UnityEngine;
using UniRx;

public class FPSSystem : SystemBehaviour
{
    private IGroup FPSComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        FPSComponents = this.Create(typeof(FPSComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        FPSComponents.OnAdd().Subscribe(entity =>
        {
            var fps = entity.GetComponent<FPSComponent>();
            fps.FrameRange = Mathf.Clamp(fps.FrameRange, 1, int.MaxValue);
            var fpsBuffer = new int[fps.FrameRange];
            var fpsBufferIndex = 0;

            Observable.EveryUpdate().Subscribe(_ =>
            {
                fpsBuffer[fpsBufferIndex++] = Mathf.FloorToInt(1f / Time.unscaledDeltaTime);
                if (fpsBufferIndex >= fps.FrameRange)
                {
                    fpsBufferIndex = 0;
                }

                var sum = 0;
                var highest = 0;
                var lowest = int.MaxValue;
                for (int i = 0; i < fps.FrameRange; i++)
                {
                    var value = fpsBuffer[i];
                    sum += value;
                    if (value > highest)
                    {
                        highest = value;
                    }
                    if (value < lowest)
                    {
                        lowest = value;
                    }
                }
                Draw(fps.AverageFPSLabel, fps.ColorRamp, Mathf.FloorToInt((float)sum / fps.FrameRange));
                Draw(fps.HighestFPSLabel, fps.ColorRamp, highest);
                Draw(fps.LowestFPSLabel, fps.ColorRamp, lowest);
            }).AddTo(Disposer).AddTo(fps.Disposer);
        }).AddTo(Disposer);
    }

    private void Draw(Text label, List<FPSColorRange> ramp, int fps)
    {
        if (label != null)
        {
            label.text = fps.ToString();
            for (int i = 0; i < ramp.Count; i++)
            {
                if (fps >= ramp[i].Range.Min && fps < ramp[i].Range.Max)
                {
                    label.color = ramp[i].Color;
                    break;
                }
            }
        }
    }
}

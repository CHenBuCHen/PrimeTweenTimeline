#if PRIME_TWEEN && ODIN_INSPECTOR
using PrimeTween;
using UnityEngine;

namespace PTT
{
    public interface ITimelineTrack
    {
        bool IsActive { get; set; }
        float Delay { get; set; }
        float Duration { get; }
        int Cycles { get; }
        string Label { get; }
        bool CallbackView => false;
        Texture2D CustomIcon => null;

        Sequence Create(ref Sequence sequence, GameObject self);

        ITimelineTrack Clone();
    }
}
#endif
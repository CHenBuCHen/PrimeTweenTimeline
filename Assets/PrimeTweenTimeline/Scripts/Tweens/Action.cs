#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace PTT.Tweens
{
    [Serializable]
    public class Action : ITimelineTrack
    {
        bool ITimelineTrack.IsActive
        {
            get => active;
            set => active = value;
        }

        float ITimelineTrack.Delay
        {
            get => delay;
            set => delay = value;
        }

        float ITimelineTrack.Duration => 0;

        int ITimelineTrack.Cycles => 0;

        string ITimelineTrack.Label => label;

        bool ITimelineTrack.CallbackView => true;

        [HideInInspector]
        public bool active = true;

        public string label = "Action";

        [OnValueChanged(nameof(OnDelayChanged))]
        public float delay;

        public bool invokeInEditorMode;

        [SerializeField]
        public UnityEvent callback;

        private void Invoke()
        {
            callback?.Invoke();
        }

        public Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (Application.isPlaying || invokeInEditorMode) sequence.InsertCallback(delay, Invoke);
            return sequence;
        }

        public ITimelineTrack Clone()
        {
            return MemberwiseClone() as ITimelineTrack;
        }

        private void OnDelayChanged()
        {
            if (delay < 0) delay = 0;
        }
    }
}
#endif
#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using System.Collections;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PTT.Tweens
{
    [Serializable]
    public class PunchScale : ITimelineTrack
    {
        bool ITimelineTrack.IsActive
        {
            get => active;
            set => active = value;
        }

        float ITimelineTrack.Delay
        {
            get => settings.startDelay;
            set => settings.startDelay = value;
        }

        float ITimelineTrack.Duration => settings.duration;

        int ITimelineTrack.Cycles => settings.cycles;

        string ITimelineTrack.Label
        {
            get
            {
                const string name = "Punch Scale";
                if (targetIsSelf) return name;
                return target ? $"{name}: {target.name}" : $"{name}: NULL";
            }
        }

        [HideInInspector]
        public bool active = true;

        [ValueDropdown(nameof(targetModeEnum))]
        [EnumToggleButtons] [LabelText("Target")] [HorizontalGroup(Width = 120)]
        [OnValueChanged(nameof(OnTargetModeChanged))]
        public bool targetIsSelf = true;

        [HideIf(nameof(targetIsSelf))]
        [HideLabel] [HorizontalGroup]
        [SceneObjectsOnly]
        public Transform target;

        public ShakeSettings settings = new()
        {
            duration = 1,
            cycles = 1
        };

        private IEnumerable targetModeEnum = new ValueDropdownList<bool>
        {
            { "Self", true },
            { "Target", false }
        };

        private void OnTargetModeChanged()
        {
            if (targetIsSelf) target = null;
        }

        Sequence ITimelineTrack.Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (targetIsSelf) target = self.transform;

            if (!target) return sequence;

            sequence.Group(Tween.PunchScale(target, settings));

            return sequence;
        }

        ITimelineTrack ITimelineTrack.Clone()
        {
            return MemberwiseClone() as ITimelineTrack;
        }
    }
}
#endif
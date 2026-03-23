#if PRIME_TWEEN && ODIN_INSPECTOR
using System.Collections;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PTT.Tweens
{
    public abstract class TweenTrack<T> : ITimelineTrack
    {
        bool ITimelineTrack.IsActive
        {
            get => active;
            set => active = value;
        }

        float ITimelineTrack.Delay
        {
            get => tweenSettings.startDelay;
            set => tweenSettings.startDelay = value;
        }

        float ITimelineTrack.Duration => tweenSettings.duration;

        int ITimelineTrack.Cycles => tweenSettings.cycles;

        string ITimelineTrack.Label
        {
            get
            {
                string niceName = GetType().GetNiceName().Replace("Tween", "");
                if (targetIsSelf) return niceName;
                return target ? $"{niceName}: {target.name}" : $"{niceName}: NULL";
            }
        }

        [HideInInspector]
        public bool active = true;

        [ValueDropdown(nameof(targetModeEnum))]
        [EnumToggleButtons] [LabelText("Target")] [HorizontalGroup(Width = 120)]
        [OnValueChanged(nameof(OnTargetModeChanged))]
        public bool targetIsSelf = true;

        [HideIf(nameof(targetIsSelf))]
        [HideLabel] [HorizontalGroup] [OnValueChanged(nameof(OnTargetChanged))]
        [SceneObjectsOnly]
        public Object target;

        [HorizontalGroup("Start", 18)]
        [ToggleLeft] [HideLabel]
        public bool hasStartValue;

        [HorizontalGroup("Start")] [EnableIf(nameof(hasStartValue))]
        [InlineButton(nameof(GoToStart), "GoTo")]
        [InlineButton(nameof(PickStart), "Pick")]
        public T start;

        [InlineButton(nameof(GoToEnd), "GoTo")]
        [InlineButton(nameof(PickEnd), "Pick")]
        public T end;

        public TweenSettings tweenSettings = new()
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

        protected virtual void OnTargetChanged()
        {
        }

        protected virtual void PickStart()
        {
        }

        protected virtual void PickEnd()
        {
        }

        protected virtual void GoToStart()
        {
        }

        protected virtual void GoToEnd()
        {
        }

        protected void RegisterUndo(string undoName)
        {
#if UNITY_EDITOR
            var scriptedAnimation = Selection.activeGameObject.GetComponent<Timeline>();
            if (scriptedAnimation) Undo.RegisterCompleteObjectUndo(scriptedAnimation, undoName);
#endif
        }

        public abstract Sequence Create(ref Sequence sequence, GameObject self);

        ITimelineTrack ITimelineTrack.Clone()
        {
            return MemberwiseClone() as ITimelineTrack;
        }
    }
}
#endif
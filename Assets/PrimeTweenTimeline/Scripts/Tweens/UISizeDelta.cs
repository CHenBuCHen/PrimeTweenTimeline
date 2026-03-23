#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using PrimeTween;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PTT.Tweens
{
    [Serializable]
    public class UISizeDelta : TweenTrack<Vector2>
    {
        private RectTransform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget)
            {
                cachedTarget = targetIsSelf
                    ? self.transform as RectTransform
                    : target as RectTransform;
            }

            if (!cachedTarget) return sequence;

            if (!hasStartValue && cachedTarget.sizeDelta == end) return sequence;

            TweenSettings<Vector2> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.sizeDelta = start;

            sequence.Group(Tween.UISizeDelta(cachedTarget, settings));

            return sequence;
        }

        protected override void OnTargetChanged()
        {
            if (target is RectTransform) return;

            if (target is GameObject go)
            {
                target = go.transform as RectTransform;
                return;
            }

            if (target is Transform t)
            {
                target = t as RectTransform;
                return;
            }

            target = null;
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject.transform is RectTransform rectTransform)
            {
                RegisterUndo("Pick start");
                start = rectTransform.sizeDelta;
            }
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject.transform is RectTransform rectTransform)
            {
                Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
                rectTransform.sizeDelta = start;
            }
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject.transform is RectTransform rectTransform)
            {
                RegisterUndo("Pick end");
                end = rectTransform.sizeDelta;
            }
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject.transform is RectTransform rectTransform)
            {
                Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
                rectTransform.sizeDelta = end;
            }
#endif
        }
    }
}
#endif
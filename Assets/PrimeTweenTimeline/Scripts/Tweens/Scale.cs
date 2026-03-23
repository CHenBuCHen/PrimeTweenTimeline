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
    public class Scale : TweenTrack<Vector3>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget)
            {
                if (targetIsSelf)
                {
                    cachedTarget = self.transform;
                }
                else
                {
                    cachedTarget = target switch
                    {
                        GameObject go => go.transform,
                        Transform t => t,
                        _ => cachedTarget
                    };
                }
            }

            if (!cachedTarget) return sequence;

            if (!hasStartValue && cachedTarget.localScale == end) return sequence;

            TweenSettings<Vector3> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.localScale = start;

            sequence.Group(Tween.Scale(cachedTarget, settings));

            return sequence;
        }

        protected override void OnTargetChanged()
        {
            if (target is not GameObject && target is not Transform) target = null;
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick start");
            start = Selection.activeGameObject.transform.localScale;
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
            Selection.activeGameObject.transform.localScale = start;
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick end");
            end = Selection.activeGameObject.transform.localScale;
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
            Selection.activeGameObject.transform.localScale = end;
#endif
        }
    }
}
#endif
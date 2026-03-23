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
    public class EulerAngle : TweenTrack<Vector3>
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

            if (!hasStartValue && cachedTarget.eulerAngles == end) return sequence;

            TweenSettings<Vector3> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.eulerAngles = start;

            sequence.Group(Tween.EulerAngles(cachedTarget, settings));

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
            start = Selection.activeGameObject.transform.eulerAngles;
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
            Selection.activeGameObject.transform.eulerAngles = start;
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick end");
            end = Selection.activeGameObject.transform.eulerAngles;
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
            Selection.activeGameObject.transform.eulerAngles = end;
#endif
        }
    }

    [Serializable]
    public class LocalEulerAngle : TweenTrack<Vector3>
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

            if (!hasStartValue && cachedTarget.localEulerAngles == end) return sequence;

            TweenSettings<Vector3> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.localEulerAngles = start;

            sequence.Group(Tween.LocalEulerAngles(cachedTarget, settings));

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
            start = Selection.activeGameObject.transform.localEulerAngles;
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
            Selection.activeGameObject.transform.localEulerAngles = start;
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick end");
            end = Selection.activeGameObject.transform.localEulerAngles;
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
            Selection.activeGameObject.transform.localEulerAngles = end;
#endif
        }
    }
}
#endif
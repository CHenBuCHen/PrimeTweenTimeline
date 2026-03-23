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
    public class Move : TweenTrack<Vector3>
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

            if (!hasStartValue && cachedTarget.position == end) return sequence;

            TweenSettings<Vector3> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.position = start;

            sequence.Group(Tween.Position(cachedTarget, settings));

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
            start = Selection.activeGameObject.transform.position;
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
            Selection.activeGameObject.transform.position = start;
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick end");
            end = Selection.activeGameObject.transform.position;
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
            Selection.activeGameObject.transform.position = end;
#endif
        }
    }

    [Serializable]
    public class LocalMove : TweenTrack<Vector3>
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

            if (!hasStartValue && cachedTarget.localPosition == end) return sequence;

            TweenSettings<Vector3> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.localPosition = start;

            sequence.Group(Tween.LocalPosition(cachedTarget, settings));

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
            start = Selection.activeGameObject.transform.localPosition;
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto start");
            Selection.activeGameObject.transform.localPosition = start;
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            RegisterUndo("Pick end");
            end = Selection.activeGameObject.transform.localPosition;
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "Goto end");
            Selection.activeGameObject.transform.localPosition = end;
#endif
        }
    }
}
#endif
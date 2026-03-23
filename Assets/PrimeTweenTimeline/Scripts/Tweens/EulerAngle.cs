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

            if (!cachedTarget) GetCache(self);

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
            if (target is GameObject go) target = cachedTarget = go.transform;

            if (target is not Transform) target = null;
        }

        private void GetCache(GameObject self)
        {
            cachedTarget = targetIsSelf ? self.transform : target as Transform;
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                RegisterUndo("Pick start");
                start = cachedTarget.eulerAngles;
            }
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto start");
                cachedTarget.eulerAngles = start;
            }
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                RegisterUndo("Pick end");
                end = cachedTarget.eulerAngles;
            }
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto end");
                cachedTarget.eulerAngles = end;
            }
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

            if (!cachedTarget) GetCache(self);

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
            if (target is GameObject go) target = cachedTarget = go.transform;

            if (target is not Transform) target = null;
        }

        private void GetCache(GameObject self)
        {
            cachedTarget = targetIsSelf ? self.transform : target as Transform;
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                RegisterUndo("Pick start");
                start = cachedTarget.localEulerAngles;
            }
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto start");
                cachedTarget.localEulerAngles = start;
            }
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                RegisterUndo("Pick end");
                end = cachedTarget.localEulerAngles;
            }
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto end");
                cachedTarget.localEulerAngles = end;
            }
#endif
        }
    }
}
#endif
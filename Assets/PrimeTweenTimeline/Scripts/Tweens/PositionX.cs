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
    public class PositionX : TweenTrack<float>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.position.x, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue)
            {
                cachedTarget.position =
                    new Vector3(start, cachedTarget.position.y, cachedTarget.position.z);
            }

            sequence.Group(Tween.PositionX(cachedTarget, settings));

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
                start = cachedTarget.position.x;
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
                cachedTarget.position = new Vector3(start, cachedTarget.position.y, cachedTarget.position.z);
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
                end = cachedTarget.position.x;
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
                cachedTarget.position = new Vector3(end, cachedTarget.position.y, cachedTarget.position.z);
            }
#endif
        }
    }

    [Serializable]
    public class LocalPositionX : TweenTrack<float>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.localPosition.x, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue)
            {
                cachedTarget.localPosition =
                    new Vector3(start, cachedTarget.localPosition.y, cachedTarget.localPosition.z);
            }

            sequence.Group(Tween.LocalPositionX(cachedTarget, settings));

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
                start = cachedTarget.localPosition.x;
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
                cachedTarget.localPosition =
                    new Vector3(start, cachedTarget.localPosition.y, cachedTarget.localPosition.z);
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
                end = cachedTarget.localPosition.x;
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
                cachedTarget.localPosition =
                    new Vector3(end, cachedTarget.localPosition.y, cachedTarget.localPosition.z);
            }
#endif
        }
    }
}
#endif
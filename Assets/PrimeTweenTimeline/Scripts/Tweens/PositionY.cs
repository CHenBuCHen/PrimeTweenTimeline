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
    public class PositionY : TweenTrack<float>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.position.y, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue)
            {
                cachedTarget.position =
                    new Vector3(cachedTarget.position.x, start, cachedTarget.position.z);
            }

            sequence.Group(Tween.PositionY(cachedTarget, settings));

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
                start = cachedTarget.position.y;
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
                cachedTarget.position = new Vector3(cachedTarget.position.x, start, cachedTarget.position.z);
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
                end = cachedTarget.position.y;
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
                cachedTarget.position = new Vector3(cachedTarget.position.x, end, cachedTarget.position.z);
            }
#endif
        }
    }

    [Serializable]
    public class LocalPositionY : TweenTrack<float>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.localPosition.y, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue)
            {
                cachedTarget.localPosition =
                    new Vector3(cachedTarget.localPosition.x, start, cachedTarget.localPosition.z);
            }

            sequence.Group(Tween.LocalPositionY(cachedTarget, settings));

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
                start = cachedTarget.localPosition.y;
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
                    new Vector3(cachedTarget.localPosition.x, start, cachedTarget.localPosition.z);
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
                end = cachedTarget.localPosition.y;
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
                    new Vector3(cachedTarget.localPosition.x, end, cachedTarget.localPosition.z);
            }
#endif
        }
    }
}
#endif
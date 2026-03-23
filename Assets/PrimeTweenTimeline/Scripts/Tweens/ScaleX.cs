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
    public class ScaleX : TweenTrack<float>
    {
        private Transform cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (!cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.localScale.x, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue)
            {
                cachedTarget.localScale =
                    new Vector3(start, cachedTarget.localScale.y, cachedTarget.localScale.z);
            }

            sequence.Group(Tween.ScaleX(cachedTarget, settings));

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
                start = cachedTarget.localScale.x;
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
                cachedTarget.localScale = new Vector3(start, cachedTarget.localScale.y, cachedTarget.localScale.z);
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
                end = cachedTarget.localScale.x;
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
                cachedTarget.localScale = new Vector3(end, cachedTarget.localScale.y, cachedTarget.localScale.z);
            }
#endif
        }
    }
}
#endif
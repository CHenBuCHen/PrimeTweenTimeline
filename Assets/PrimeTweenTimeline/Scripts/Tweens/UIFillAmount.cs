using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if PRIME_TWEEN && ODIN_INSPECTOR
namespace PTT.Tweens
{
    [Serializable]
    public class UIFillAmount : TweenTrack<float>
    {
        private Image cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            if (cachedTarget) GetCache(self);

            if (!cachedTarget) return sequence;

            if (!hasStartValue && Mathf.Approximately(cachedTarget.fillAmount, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) cachedTarget.fillAmount = start;

            sequence.Group(Tween.UIFillAmount(cachedTarget, settings));

            return sequence;
        }

        protected override void OnTargetChanged()
        {
            if (target is GameObject go) target = go.GetComponent<Image>();

            if (target is Transform transform) target = transform.GetComponent<Image>();

            if (target is not Image) target = null;
        }

        private void GetCache(GameObject self)
        {
            cachedTarget = targetIsSelf ? self.transform.GetComponent<Image>() : target as Image;
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            GetCache(Selection.activeGameObject);
            if (cachedTarget)
            {
                RegisterUndo("Pick start");
                start = cachedTarget.fillAmount;
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
                cachedTarget.fillAmount = start;
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
                end = cachedTarget.fillAmount;
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
                cachedTarget.fillAmount = end;
            }
#endif
        }
    }
}
#endif
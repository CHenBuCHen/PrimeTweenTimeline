#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PTT.Tweens
{
    [Serializable]
    public class Alpha : TweenTrack<float>
    {
        private Object cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            GetCachedTarget(self);

            if (!cachedTarget) return sequence;

            float currentAlpha = GetCurrentAlpha();

            if (!hasStartValue && Mathf.Approximately(currentAlpha, end)) return sequence;

            TweenSettings<float> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) SetAlpha(start);

            switch (cachedTarget)
            {
                case Graphic targetGraphic:
                    sequence.Group(Tween.Alpha(targetGraphic, settings));
                    break;
                case Shadow shadow:
                    sequence.Group(Tween.Alpha(shadow, settings));
                    break;
                case SpriteRenderer targetSprite:
                    sequence.Group(Tween.Alpha(targetSprite, settings));
                    break;
                
                case CanvasGroup targetCanvasGroup:
                    sequence.Group(Tween.Alpha(targetCanvasGroup, settings));
                    break;
            }

            return sequence;
        }

        private float GetCurrentAlpha()
        {
            return cachedTarget switch
            {
                Graphic graphic => graphic.color.a,
                Shadow shadow => shadow.effectColor.a,
                SpriteRenderer spriteRenderer => spriteRenderer.color.a,
                CanvasGroup canvasGroup => canvasGroup.alpha,
                _ => 1
            };
        }

        private void SetAlpha(float alpha)
        {
            UnityEngine.Color color;
            switch (cachedTarget)
            {
                case Graphic graphic:
                    color = graphic.color;
                    color.a = alpha;
                    graphic.color = color;
                    break;

                case Shadow shadow:
                    color = shadow.effectColor;
                    color.a = alpha;
                    shadow.effectColor = color;
                    break;

                case SpriteRenderer spriteRenderer:
                    color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                    break;
                
                case CanvasGroup canvasGroup:
                    canvasGroup.alpha = alpha;
                    break;
            }
        }

        protected override void OnTargetChanged()
        {
            if (target is Transform t) target = t.gameObject;

            if (target is GameObject go)
            {
                target = go.GetComponent<Graphic>();

                if (!target) target = go.GetComponent<Shadow>();

                if (!target) target = go.GetComponent<CanvasGroup>();

                if (!target) target = go.GetComponent<SpriteRenderer>();

                return;
            }

            if (target is not Graphic && target is not Shadow && target is not SpriteRenderer &&  target is not CanvasGroup)
            {
                target = null;
            }
        }

        private void GetCachedTarget(GameObject self)
        {
            if (cachedTarget) return;

            if (targetIsSelf)
            {
                cachedTarget = self.GetComponent<Graphic>();

                if (!cachedTarget) cachedTarget = self.GetComponent<Shadow>();
                
                if (!cachedTarget) cachedTarget = self.GetComponent<CanvasGroup>();

                if (!cachedTarget) cachedTarget = self.GetComponent<SpriteRenderer>();
            }
            else
            {
                cachedTarget = target;
            }
        }

        protected override void PickStart()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            RegisterUndo("Pick start");
            start = GetCurrentAlpha();
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto start");
            SetAlpha(start);
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            RegisterUndo("Pick end");
            end = GetCurrentAlpha();
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto end");
            SetAlpha(end);
#endif
        }
    }
}
#endif
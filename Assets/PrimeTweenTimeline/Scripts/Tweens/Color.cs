#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UColor = UnityEngine.Color;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PTT.Tweens
{
    [Serializable]
    public class Color : TweenTrack<UColor>
    {
        private Object cachedTarget;

        public override Sequence Create(ref Sequence sequence, GameObject self)
        {
            if (!active) return sequence;

            GetCachedTarget(self);

            if (!cachedTarget) return sequence;

            UColor currentColor = GetCurrentColor();

            if (!hasStartValue && currentColor == end) return sequence;

            TweenSettings<UColor> settings = new(start, end, tweenSettings)
            {
                startFromCurrent = !hasStartValue
            };

            if (hasStartValue) SetColor(start);

            if (cachedTarget is Graphic targetGraphic)
            {
                sequence.Group(Tween.Color(targetGraphic, settings));
            }
            else if (cachedTarget is Shadow shadow)
            {
                sequence.Group(Tween.Color(shadow, settings));
            }
            else if (cachedTarget is SpriteRenderer targetSprite)
            {
                sequence.Group(Tween.Color(targetSprite, settings));
            }

            return sequence;
        }

        private UColor GetCurrentColor()
        {
            return cachedTarget switch
            {
                Graphic graphic => graphic.color,
                Shadow shadow => shadow.effectColor,
                SpriteRenderer spriteRenderer => spriteRenderer.color,
                _ => UColor.white
            };
        }

        private void SetColor(UColor color)
        {
            switch (cachedTarget)
            {
                case Graphic graphic:
                    graphic.color = color;
                    break;
                case Shadow shadow:
                    shadow.effectColor = color;
                    break;
                case SpriteRenderer spriteRenderer:
                    spriteRenderer.color = color;
                    break;
            }
        }

        protected override void OnTargetChanged()
        {
            if (target is Transform t)
            {
                target = t.gameObject;
            }

            if (target is GameObject go)
            {
                target = go.GetComponent<Graphic>();

                if (!target) target = go.AddComponent<Shadow>();

                if (!target) target = go.AddComponent<SpriteRenderer>();

                return;
            }

            if (target is not Graphic && target is not Shadow && target is not SpriteRenderer)
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

                if (!cachedTarget) cachedTarget = self.AddComponent<Shadow>();

                if (!cachedTarget) cachedTarget = self.AddComponent<SpriteRenderer>();
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
            start = GetCurrentColor();
#endif
        }

        protected override void GoToStart()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto start");
            SetColor(start);
#endif
        }

        protected override void PickEnd()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            RegisterUndo("Pick end");
            end = GetCurrentColor();
#endif
        }

        protected override void GoToEnd()
        {
#if UNITY_EDITOR
            GetCachedTarget(Selection.activeGameObject);
            Undo.RegisterCompleteObjectUndo(cachedTarget, "Goto end");
            SetColor(end);
#endif
        }
    }
}

#endif
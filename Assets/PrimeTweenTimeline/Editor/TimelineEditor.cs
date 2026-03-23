#if PRIME_TWEEN && ODIN_INSPECTOR
using PrimeTween;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PTT.Editor
{
    [CustomEditor(typeof(Timeline))]
    public class TimelineEditor : OdinEditor
    {
        private Timeline timeline;

        private TimelineView view;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (targets == null || targets.Length == 0) return;
            timeline = (Timeline)target;
            view = new TimelineView(timeline);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            view?.Dispose();
            timeline.Stop();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Sequence controller = timeline.Controller;

            view.Draw(controller);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (controller.isAlive || view.IsTimeDragging || view.IsTweenDragging)
            {
                Repaint();
            }
        }
    }
}
#endif
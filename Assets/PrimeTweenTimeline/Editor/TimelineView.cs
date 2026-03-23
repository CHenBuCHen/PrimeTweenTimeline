#if PRIME_TWEEN
using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace PTT.Editor
{
    public class TimelineView : IDisposable
    {
        private readonly Timeline timeline;
        private readonly AddMoreItem[] addMoreItems;

        private bool isTimeDragging;
        private bool isTweenDragging;
        private float? dragTweenTimeShift;

        public bool IsTimeDragging => isTimeDragging;
        public bool IsTweenDragging => isTweenDragging;
        public float TimeScale { get; private set; }
        public static bool IsSnapping { get; set; }

        private PropertyTree selectedDrawer;

        private bool IsTweenSelected
        {
            get
            {
                int index = timeline.selectedIndex;
                bool selected = index >= 0 && index < timeline.sequence.Count;
                if (!selected && index != -1) timeline.selectedIndex = -1;
                return selected;
            }
        }

        private ITimelineTrack SelectedTrack => timeline.sequence[timeline.selectedIndex];

        static TimelineView()
        {
            IsSnapping = true;
        }

        public TimelineView(Timeline timeline, AddMoreItem[] addMoreItems = null)
        {
            this.timeline = timeline;
            this.addMoreItems = addMoreItems;

            if (this.addMoreItems == null)
            {
                this.addMoreItems = CreateAddMoreItems();
            }

            this.timeline.sequence.RemoveAll(item => item == null);

            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        }

        ~TimelineView()
        {
            Dispose();
        }

        public void Dispose()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;

            if (selectedDrawer == null) return;
            selectedDrawer.Dispose();
            selectedDrawer = null;
        }

        public void Draw(Sequence controler)
        {
            Rect rect = TimelineGUI.GetTimelineControlRect(timeline.sequence.Count);

            TimelineGUI.Background(rect);

            Rect headerRect = TimelineGUI.Header(rect, timeline);

            TimeScale = CalculateTimeScale();
            bool timeDragStarted = false;
            Rect timeRect = TimelineGUI.Time(
                rect, TimeScale, ref isTimeDragging,
                () => timeDragStarted = true, OnTimeDragEnd
            );

            Rect tweensRect = TimelineGUI.Tweens(rect, timeline, TimeScale, ref isTweenDragging, TweenSelected);

            TimelineGUI.AddButton(rect, addMoreItems, item => AddMore(item.Type));

            if (IsTweenSelected && TimelineGUI.RemoveButton(rect))
            {
                RemoveSelected();
            }

            if (IsTweenSelected && TimelineGUI.DuplicateButton(rect))
            {
                DuplicateSelected();
            }

            if (controler.isAlive)
            {
                float scaledTime = controler.elapsedTime * TimeScale;
                Rect verticalRect = AddRect(timeRect, tweensRect);
                TimelineGUI.TimeVerticalLine(verticalRect, scaledTime, controler.isPaused);

                if (controler.isPaused)
                {
                    TimelineGUI.PlayheadLabel(timeRect, scaledTime, controler.elapsedTime);
                }
            }

            if (isTimeDragging)
            {
                float scaledTime = TimelineGUI.GetScaledTimeUnderMouse(timeRect);
                float rawTime = scaledTime / TimeScale;
                TimelineGUI.TimeVerticalLine(AddRect(timeRect, tweensRect), scaledTime, true);
                TimelineGUI.PlayheadLabel(timeRect, scaledTime, rawTime);

                if (Event.current.type is EventType.MouseDrag || timeDragStarted)
                {
                    OnTimeDrag(controler, rawTime);
                }
            }

            if (isTweenDragging && IsTweenSelected)
            {
                float time = TimelineGUI.GetScaledTimeUnderMouse(timeRect);

                if (Event.current.type == EventType.MouseDrag)
                {
                    float rawTime = time / TimeScale;
                    DragSelectedAnimation(rawTime);
                }
            }

            if (controler.isAlive)
            {
                if (controler.isPaused)
                {
                    if (TimelineGUI.PlayButton(rect)) timeline.Play();
                }
                else
                {
                    if (TimelineGUI.PauseButton(rect)) controler.isPaused = true;
                }
            }
            else
            {
                if (TimelineGUI.PlayButton(rect)) timeline.Play();
            }

            if (TimelineGUI.StopButton(rect, controler)) timeline.Stop();

            bool snapToggle = TimelineGUI.SnapToggle(rect, IsSnapping);
            if (snapToggle != IsSnapping)
            {
                IsSnapping = snapToggle;
            }

            TimelineGUI.PreviewEye(headerRect, timeline, isTimeDragging);

            if (Event.current.type == EventType.MouseDown)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                if (IsTweenSelected && rect.Contains(mousePosition))
                {
                    TweenSelected(-1, true);
                }
            }

            if (IsTweenSelected)
            {
                selectedDrawer ??= PropertyTree.Create(SelectedTrack);

                TimelineGUI.DrawSelected(
                    selectedDrawer, SelectedTrack,
                    OnSelectedPropertyChanged,
                    OnSelectUpButtonClicked,
                    OnSelectDownButtonClicked
                );
            }
        }

        private void DragSelectedAnimation(float time)
        {
            // Sometimes (e.g., for Frame) undo is not recorded when dragging, so we force it
            Undo.RecordObject(timeline, $"Drag {SelectedTrack.Label}");

            dragTweenTimeShift ??= time - SelectedTrack.Delay;

            float delay = time - dragTweenTimeShift.Value;
            delay = Mathf.Max(0, delay);
            delay = TrySnapTime(SelectedTrack, delay, TimeScale);
            delay = (float)Math.Round(delay, 2);
            SelectedTrack.Delay = delay;

            // Complete undo record
            Undo.FlushUndoRecordObjects();
        }

        private float TrySnapTime(ITimelineTrack target, float newDelay, float timeScale)
        {
            if (!IsSnapActive() || timeline.sequence.Count < 2)
            {
                return newDelay;
            }

            float snapThreshold = 1f / 40f / timeScale;
            float[] snapPoints = timeline.sequence
                .Where(animation => animation != target)
                .SelectMany(animation => Enumerable.Empty<float>().Append(animation.Delay)
                    .Append(animation.Delay + animation.Duration * Mathf.Max(1, animation.Cycles)))
                .Distinct().ToArray();

            float snapTime = snapPoints.OrderBy(snapPoint => Mathf.Abs(snapPoint - newDelay)).First();
            if (Math.Abs(snapTime - newDelay) < snapThreshold)
            {
                return snapTime;
            }

            if (target.Cycles == -1)
            {
                return newDelay;
            }

            float targetFullDuration = target.Duration * Mathf.Max(1, target.Cycles);
            float newEndTime = newDelay + targetFullDuration;
            float snapEndTime = snapPoints.OrderBy(snapPoint => Mathf.Abs(snapPoint - newEndTime)).First();
            if (Math.Abs(snapEndTime - newEndTime) < snapThreshold)
            {
                return snapEndTime - targetFullDuration;
            }

            return newDelay;
        }

        private bool IsSnapActive()
        {
            bool reverseSnap = Event.current.control;
            bool snapEnabled = IsSnapping;
            return reverseSnap ? !snapEnabled : snapEnabled;
        }

        private void RemoveSelected()
        {
            Undo.RegisterCompleteObjectUndo(timeline, "Remove selected tween");
            timeline.sequence.RemoveAt(timeline.selectedIndex);
            TweenSelected(-1);
            EditorUtility.SetDirty(timeline);
        }

        private void DuplicateSelected()
        {
            Undo.RegisterCompleteObjectUndo(timeline, "Duplicate selected tween");

            ITimelineTrack clone = SelectedTrack.Clone();
            timeline.sequence.Add(clone);
            TweenSelected(timeline.sequence.Count - 1);
            EditorUtility.SetDirty(timeline);
        }

        private float CalculateTimeScale()
        {
            float maxTime = timeline.sequence.Count > 0
                ? timeline.sequence.Max(animation =>
                    animation.Delay + animation.Duration * Mathf.Max(1, animation.Cycles))
                : 1f;
            return 1f / maxTime;
        }

        private void AddMore(Type type)
        {
            var tweenData = Activator.CreateInstance(type) as ITimelineTrack;
            if (tweenData == null) return;

            Undo.RegisterCompleteObjectUndo(timeline, $"Add animation {tweenData.Label}");
            timeline.sequence.Add(tweenData);
            EditorUtility.SetDirty(timeline);
        }

        private void OnTimeDrag(Sequence sequence, float rawTime)
        {
            if (!sequence.isAlive) timeline.Play();

            sequence = timeline.Controller;
            sequence.elapsedTime = rawTime;
            sequence.isPaused = true;
        }

        private void OnTimeDragEnd(Event mouseEvent)
        {
            const int mouseButtonMiddle = 2;
            if (IsRightMouseButton(mouseEvent) || mouseEvent.button == mouseButtonMiddle)
            {
                timeline.Stop();
            }
        }

        public static bool IsRightMouseButton(Event @event)
        {
            const int mouseButtonLeft = 0;
            if (Application.platform == RuntimePlatform.OSXEditor && @event.control && @event.button == mouseButtonLeft)
            {
                return true;
            }

            const int mouseButtonRight = 1;
            return @event.button == mouseButtonRight;
        }

        public void TweenSelected(int selectedIndex, bool undo = false)
        {
            if (selectedIndex < -1 || selectedIndex > timeline.sequence.Count - 1)
            {
                selectedIndex = -1;
            }

            if (timeline.selectedIndex == selectedIndex) return;

            if (undo)
            {
                string msg = selectedIndex == -1 ? "Unselect tween" : "Selected tween";
                Undo.RegisterCompleteObjectUndo(timeline, msg);
            }

            timeline.selectedIndex = selectedIndex;
            if (undo) EditorUtility.SetDirty(timeline);

            GUIUtility.keyboardControl = 0;
            dragTweenTimeShift = null;

            if (selectedDrawer != null)
            {
                selectedDrawer.Dispose();
                selectedDrawer = null;
            }
        }

        private void OnSelectedPropertyChanged(Action action)
        {
            Undo.RegisterCompleteObjectUndo(timeline, "Change selected property changed");
            action?.Invoke();
            EditorUtility.SetDirty(timeline);
        }

        private void OnSelectUpButtonClicked()
        {
            int selectedIndex = timeline.selectedIndex;
            if (selectedIndex is -1 or 0) return;

            Undo.RegisterCompleteObjectUndo(timeline, $"Move up {SelectedTrack.Label}");
            
            List<ITimelineTrack> sequence = timeline.sequence;
            (sequence[selectedIndex], sequence[selectedIndex - 1]) = (sequence[selectedIndex - 1], sequence[selectedIndex]);

            TweenSelected(selectedIndex - 1);

            EditorUtility.SetDirty(timeline);
        }

        private void OnSelectDownButtonClicked()
        {
            int selectedIndex = timeline.selectedIndex;
            if (selectedIndex == -1 || selectedIndex == timeline.sequence.Count - 1) return;

            Undo.RegisterCompleteObjectUndo(timeline, $"Move down {SelectedTrack.Label}");

            List<ITimelineTrack> sequence = timeline.sequence;
            (sequence[selectedIndex], sequence[selectedIndex + 1]) = (sequence[selectedIndex + 1], sequence[selectedIndex]);
            
            TweenSelected(selectedIndex + 1);

            EditorUtility.SetDirty(timeline);
        }

        public static Rect AddRect(Rect a, Rect b)
        {
            if (b.xMin < (double)a.xMin) a.xMin = b.xMin;
            if (b.xMax > (double)a.xMax) a.xMax = b.xMax;
            if (b.yMin < (double)a.yMin) a.yMin = b.yMin;
            if (b.yMax > (double)a.yMax) a.yMax = b.yMax;

            return a;
        }

        private static AddMoreItem[] CreateAddMoreItems()
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type =>
                    type.IsClass && !type.IsAbstract && typeof(ITimelineTrack).IsAssignableFrom(type))
                .ToArray();

            return types
                .Select((type, _) =>
                    new AddMoreItem(new GUIContent($"{type.GetNiceName().Replace("Tween", "")}"), type))
                .ToArray();
        }
    }
}
#endif
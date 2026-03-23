#if PRIME_TWEEN
using System;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Action = System.Action;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace PTT.Editor
{
    public class TimelineGUI
    {
        private const int TIMELINE_HEADER_HEIGHT = 28;
        private const float ROW_HEIGHT = 20;
        private const int BOTTOM_HEIGHT = 30;
        private const int TIME_HEIGHT = 20;
        private const float MIN_TWEEN_RECT_WIDTH = 16f;

        private const string ICON_CALLBACK =
            "iVBORw0KGgoAAAANSUhEUgAAABQAAAAoCAYAAAD+MdrbAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAD4SURBVHgB7ZWxCoJQFIaPkkuOQVtDLQ02tPQGDrn6CvU+tbeH4NTmCzgLRWM0NGhBuCiBBXZO3eISFXppMLgf/ChX7ufPWQ6ApHIo3HsX08ZoUI4zZodZ84c9x3GmWZYleUmSJNnTXVboiUUfckGoCDqGJFKZsKbrehME0TRNBzYqFX6MFEqhFErhnwsvcRxvQZA0TQ/4OPFnHdu2RyJrAIts8O4YXnYK0cKYvu/Pi4hoj3ieN8M7Fty35VvqmD61jaJo+UkWhuGKtRpAwbV7a+u67oQfA9fKxDSgJPRng9oGQbCgsFYGfGmlFBDTGB4zijBHkEgqzhX38zVoGGkfagAAAABJRU5ErkJggg==";

        private static Texture2D IconCallback => ImageFromString(ICON_CALLBACK);

        private static readonly Vector2 PlayButtonSize = new(34, 24);
        private static readonly Vector2 LoopToggleSize = new(24, 24);
        private static readonly Color ToggleFadeColor = new(1f, 1f, 1f, 0.7f);
        private static readonly Color PlayheadColor = new(0.19f, 0.44f, 0.89f);
        private static readonly Vector2 InspectorButtonSize = new(24f, 20f);


        private static readonly GUIStyle TimelineHeaderStyle =
            new(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };

        private static readonly GUIStyle AddTweenButtonStyle =
            new(EditorStyles.miniButtonLeft) { fixedHeight = 0 };

        private static readonly GUIStyle InspectorHeaderStyle =
            new(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };

        private static readonly GUIStyle InspectorButtonStyle = new(EditorStyles.iconButton)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
            fixedWidth = 0, fixedHeight = 0
        };

        private static readonly GUIContent InspectorUpButton = EditorGUIUtility.TrTextContent("↑", "Move Up");

        private static readonly GUIContent InspectorDownButton = EditorGUIUtility.TrTextContent("↓", "Move Down");


        private static readonly Dictionary<string, Texture2D> IconCache = new();

        private static readonly Color[] Colors =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        public static Rect GetTimelineControlRect(int tweenCount)
        {
            return EditorGUILayout.GetControlRect(false,
                TIMELINE_HEADER_HEIGHT + TIME_HEIGHT + tweenCount * ROW_HEIGHT + BOTTOM_HEIGHT);
        }

        public static void Background(Rect rect)
        {
            RoundRect(rect, new Color(0, 0, 0, 0.3f), 4);
            RoundRect(rect, Color.black, 4, 1);
        }

        public static Rect Header(Rect rect, Timeline scriptedAnimation)
        {
            rect = rect.SetHeight(TIMELINE_HEADER_HEIGHT);
            Rect textRect = rect.AlignCenter(rect.width - PlayButtonSize.x * 6);

            EditorGUI.BeginChangeCheck();
            string txt = EditorGUI.TextField(textRect, new GUIContent(), scriptedAnimation.label, TimelineHeaderStyle);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(scriptedAnimation, "Change label");
                scriptedAnimation.label = txt;
                EditorUtility.SetDirty(scriptedAnimation);
            }

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);

            return rect;
        }

        public static Rect Time(Rect rect, float timeScale, ref bool isDragging, Action start, Action<Event> end)
        {
            rect = new Rect(rect.x, rect.y + TIMELINE_HEADER_HEIGHT, rect.width, rect.height);
            rect = rect.SetHeight(TIME_HEIGHT);

            GUIStyle style = new(GUI.skin.label)
            {
                fontSize = 9, normal = { textColor = new Color(1, 1, 1, 0.5f) }
            };

            const int count = 10;
            const float step = 1f / count;
            for (int i = 0; i < count; i++)
            {
                float time = i * step;
                var position = new Rect(rect.x + i * step * rect.width, rect.y, step * rect.width, rect.height);
                time /= timeScale;
                GUI.Label(position, time.ToString("0.00"), style);
            }

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            ProcessDragEvents(rect, ref isDragging, start, end);

            return rect;
        }

        private static void ProcessDragEvents(Rect rect, ref bool isDragging, Action start, Action<Event> end)
        {
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown when !isDragging && rect.Contains(current.mousePosition):
                    isDragging = true;
                    start?.Invoke();
                    current.Use();
                    break;

                case EventType.MouseUp when isDragging:
                    isDragging = false;
                    end?.Invoke(current);
                    current.Use();
                    break;
            }
        }

        public static Rect Tweens(Rect rect, Timeline timeline, float timeScale,
            ref bool isTweenDragging, Action<int, bool> tweenSelected)
        {
            rect = new Rect(rect.x, rect.y + TIMELINE_HEADER_HEIGHT + TIME_HEIGHT, rect.width, rect.height);
            rect = rect.SetHeight(timeline.sequence.Count * ROW_HEIGHT);

            for (int i = 0; i < timeline.sequence.Count; i++)
            {
                ITimelineTrack track = timeline.sequence[i];
                var rowRect = new Rect(rect.x, rect.y + i * ROW_HEIGHT, rect.width, ROW_HEIGHT);

                bool isSelected = timeline.selectedIndex == i;
                Rect tweenRect = Element(track, rowRect, isSelected, timeScale);

                ProcessDragEvents(tweenRect, ref isTweenDragging, Start, null);

                var bottomLine = new Rect(rowRect.x, rowRect.y + rowRect.height, rowRect.width, 1);
                EditorGUI.DrawRect(bottomLine, Color.black);
                continue;

                void Start()
                {
                    tweenSelected?.Invoke(i, true);
                }
            }

            return rect;
        }

        private static Rect Element(ITimelineTrack track, Rect rowRect, bool isSelected, float timeScale)
        {
            if (track.CallbackView)
            {
                return Callback(track, rowRect, isSelected, timeScale);
            }

            return Tween(track, rowRect, isSelected, timeScale);
        }

        private static Rect Callback(ITimelineTrack track, Rect rowRect, bool isSelected, float timeScale)
        {
            float alphaMultiplier = track.IsActive ? 1f : 0.4f;

            void Label(Rect rect, GUIContent content, GUIStyle style)
            {
                GUIHelper.PushColor(new Color(1, 1, 1, track.IsActive ? 1 : 0.4f));
                GUI.Label(rect, content, style);
                GUIHelper.PopColor();
            }

            void Icon(bool isHovered, Rect iconRect)
            {
                var iconColor = new Color(1, 1, 1, 0.6f * alphaMultiplier);
                if (isSelected)
                {
                    iconColor = new Color(0.2f, 0.6f, 1f, alphaMultiplier);
                }
                else if (isHovered)
                {
                    iconColor = new Color(1, 1, 1, 0.5f * alphaMultiplier);
                }

                Texture2D icon = track.CustomIcon ?? IconCallback;
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true, 0, iconColor, 0, 0);
            }

            void Underline(bool isHovered, Rect textRect)
            {
                if (!isSelected && !isHovered) return;

                var underlineRect = new Rect(textRect.x, textRect.yMax - 4, textRect.width, 1);
                Color color = isHovered
                    ? new Color(1, 1, 1, 0.7f * alphaMultiplier)
                    : new Color(1, 1, 1, alphaMultiplier);
                EditorGUI.DrawRect(underlineRect, color);
            }

            var textStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                richText = true
            };

            float iconX = CalculateX(rowRect, track.Delay, timeScale);
            Rect iconRect = new(iconX, rowRect.y, 10, 20);

            var labelContent = new GUIContent(track.Label);

            textStyle.padding = new RectOffset((int)iconRect.width + 4, 0, 0, 1);
            float textWidth = textStyle.CalcSize(labelContent).x;
            var rect = new Rect(iconRect.x, rowRect.y, textWidth, rowRect.height);

            bool onRightSide = rect.x > rowRect.x + rowRect.width * 0.5f;
            bool outOfBounds = rect.xMax > rowRect.xMax;
            if (onRightSide && outOfBounds)
            {
                (textStyle.padding.right, textStyle.padding.left) = (textStyle.padding.left, textStyle.padding.right);
                rect.x = iconRect.xMax - textWidth;
            }

            var textOnlyRect = new Rect(
                rect.x + textStyle.padding.left,
                rect.y,
                rect.width + -textStyle.padding.horizontal,
                rect.height
            );
            bool isHovered = rect.Contains(Event.current.mousePosition);

            Icon(isHovered, iconRect);
            Underline(isHovered, textOnlyRect);
            Label(rect, labelContent, textStyle);

            return rect;
        }

        private static Rect Tween(ITimelineTrack track, Rect rowRect, bool isSelected, float timeScale)
        {
            bool isInfinite = track.Cycles == -1;
            int loops = Mathf.Max(1, track.Cycles);
            float start = CalculateX(rowRect, track.Delay, timeScale);
            float width = isInfinite
                ? rowRect.width - start + rowRect.x
                : track.Duration * loops * timeScale * rowRect.width;
            width = Mathf.Max(width, MIN_TWEEN_RECT_WIDTH);

            Rect tweenRect = new Rect(start, rowRect.y, width, rowRect.height).Expand(-1);
            float alphaMultiplier = track.IsActive ? 1f : 0.4f;

            RoundRect(tweenRect, new Color(0.5f, 0.5f, 0.5f, 0.3f * alphaMultiplier), 4);

            bool mouseHover = tweenRect.Contains(Event.current.mousePosition);
            if (isSelected)
            {
                RoundRect(tweenRect, new Color(1, 1, 1, 0.9f * alphaMultiplier), 4, 2);
            }
            else
            {
                if (mouseHover)
                {
                    RoundRect(tweenRect, new Color(1, 1, 1, 0.9f * alphaMultiplier), 4, 1);
                }
            }

            var colorLine = new Rect(tweenRect.x + 1, tweenRect.y + tweenRect.height - 3, tweenRect.width - 2, 2);
            Random.InitState(track.GetHashCode());
            Color color = Colors[Random.Range(0, Colors.Length)];
            color.a = 0.6f * alphaMultiplier;
            EditorGUI.DrawRect(colorLine, color);

            var label = new GUIContent(track.Label);
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1, 1, 1, alphaMultiplier) }
            };
            float labelWidth = style.CalcSize(label).x;
            Rect labelRect = tweenRect;
            if (labelWidth > labelRect.width)
            {
                label.tooltip = track.Label;
                style.alignment = mouseHover ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

                // just to make it look nice
                labelRect.xMin += 4f;
            }

            GUI.Label(labelRect, label, style);

            return tweenRect;
        }

        public static void AddButton(Rect timelineRect, AddMoreItem[] items, Action<AddMoreItem> clicked)
        {
            Rect buttonRect = CalculateAddButtonRect(timelineRect);
            var image = Resources.Load<Texture>("ptt.timeline.add.tween");
            var content = new GUIContent(image) { tooltip = "Add tween" };
            if (GUI.Button(buttonRect, content, AddTweenButtonStyle))
            {
                var menu = new GenericMenu();
                foreach (AddMoreItem item in items)
                {
                    menu.AddItem(item.Content, false, userData => clicked?.Invoke((AddMoreItem)userData),
                        item);
                }

                menu.DropDown(new Rect(buttonRect.x - 4, buttonRect.y, buttonRect.width, buttonRect.height));
            }
        }

        public static bool RemoveButton(Rect timelineRect)
        {
            Vector2 buttonSize = new(50, 24);
            Vector2 position = new(
                timelineRect.x + timelineRect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2,
                timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2
            );
            var buttonRect = new Rect(position, buttonSize);

            return GUI.Button(buttonRect, "Delete");
        }

        public static bool DuplicateButton(Rect rect)
        {
            Vector2 buttonSize = new(66, 24);
            Vector2 position = new(
                rect.x + rect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2 - 50 - 2,
                rect.y + rect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2
            );
            Rect buttonRect = new(position, buttonSize);

            return GUI.Button(buttonRect, "Duplicate");
        }

        public static void TimeVerticalLine(Rect rect, float scaledTime, bool underLabel)
        {
            // some extra shift to nice look on borders
            int shift = underLabel ? 10 : 1;
            var verticalLine = new Rect(rect.x + scaledTime * rect.width, rect.y + shift, 1, rect.height - shift);
            EditorGUI.DrawRect(verticalLine, PlayheadColor);
        }

        public static void PlayheadLabel(Rect timeRect, float scaledTime, float rawTime)
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            var position = new Vector2(timeRect.x + scaledTime * timeRect.width, timeRect.y);
            var labelContent = new GUIContent(rawTime.ToString("0.00"));

            const int yShift = 1;
            var labelRect = new Rect(position.x, position.y + yShift, 32, timeRect.height - yShift * 2);
            labelRect.x -= labelRect.width * 0.5f;
            const int maxXShift = 4;
            labelRect.x = Mathf.Clamp(labelRect.x, timeRect.x - maxXShift, timeRect.xMax - labelRect.width + maxXShift);

            var labelBackground = new Rect(labelRect.x, labelRect.y, labelRect.width, labelRect.height);
            RoundRect(labelBackground, PlayheadColor, 8);

            GUI.Label(labelRect, labelContent, labelStyle);
        }

        public static bool PlayButton(Rect rect)
        {
            GUIContent content = EditorGUIUtility.IconContent("d_PlayButton");
            Vector2 position = rect.position + new Vector2(2, (TIMELINE_HEADER_HEIGHT - PlayButtonSize.y) / 2);
            Rect buttonRect = new(position, PlayButtonSize);
            Color contentColor = GUI.contentColor;
            GUI.contentColor = Color.cyan;
            bool result = GUI.Button(buttonRect, content);
            GUI.contentColor = contentColor;
            return result;
        }

        public static bool PauseButton(Rect rect)
        {
            GUIContent content = EditorGUIUtility.IconContent("d_PauseButton");
            Vector2 position = rect.position + new Vector2(2, (TIMELINE_HEADER_HEIGHT - PlayButtonSize.y) / 2);
            Rect buttonRect = new(position, PlayButtonSize);

            bool result = GUI.Button(buttonRect, content);
            return result;
        }

        public static bool StopButton(Rect rect, Sequence controller)
        {
            Vector2 position = rect.position +
                               new Vector2(2 + PlayButtonSize.x, (TIMELINE_HEADER_HEIGHT - PlayButtonSize.y) / 2);
            Rect buttonRect = new(position, PlayButtonSize);

            if (!controller.isAlive) GUIHelper.PushGUIEnabled(false);
            bool result = GUI.Button(buttonRect, "■");
            if (!controller.isAlive) GUIHelper.PopGUIEnabled();

            return result;
        }

        public static bool SnapToggle(Rect rect, bool value)
        {
            Vector2 position = rect.position + new Vector2(
                rect.width - (LoopToggleSize.x + 1) * 2 - 2,
                (TIMELINE_HEADER_HEIGHT - LoopToggleSize.y) / 2
            );
            var toggleRect = new Rect(position, LoopToggleSize);
            GUIContent iconContent = EditorGUIUtility.TrIconContent("SceneViewSnap",
                $"Toggle snapping while dragging tweens\n\nHold <b>Ctrl</b> to temporarily {(value ? "disable" : "enable")} snapping.");
            var style = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            using ColorScope colorScope = new(ToggleFadeColor, ToggleFadeColor);
            return GUI.Toggle(toggleRect, value, iconContent, style);
        }

        public static void PreviewEye(Rect headerRect, Timeline timeline, bool isTimeDragging)
        {
            if (!timeline.Controller.isAlive && !isTimeDragging) return;

            float width = TimelineHeaderStyle.CalcWidth(timeline.label) / 2;

            Vector2 iconSize = Vector2.one * 16f;
            var eyeShift = new Vector2(width + 2, -0f);
            var iconRect = new Rect(
                headerRect.x + headerRect.width * 0.5f + eyeShift.x,
                headerRect.y + (headerRect.height - iconSize.y) / 2 + eyeShift.y,
                iconSize.x, iconSize.y
            );

            GUIContent eyeIcon = EditorGUIUtility.TrIconContent("animationvisibilitytoggleon");

            GUI.Label(iconRect, eyeIcon, EditorStyles.iconButton);
        }

        public static void DrawSelected(PropertyTree selectedTree, ITimelineTrack selected,
            Action<Action> onSelectedPropertyChanged, Action onButtonUp, Action onButtonDown)
        {
            if (selectedTree == null) return;
            EditorGUILayout.Space();

            Splitter(new Color(0.12f, 0.12f, 0.12f, 1.333f));

            Rect backgroundRect = GUILayoutUtility.GetRect(1f, 20f);
            Rect labelRect = backgroundRect;
            backgroundRect = ToFullWidth(backgroundRect);
            EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.2f));

            EditorGUI.BeginChangeCheck();
            bool active = EditorGUI.ToggleLeft(
                labelRect.AlignLeft(labelRect.width - InspectorButtonSize.x * 3),
                $"{selected.Label}",
                selected.IsActive, InspectorHeaderStyle
            );
            if (EditorGUI.EndChangeCheck())
            {
                onSelectedPropertyChanged?.Invoke(() => selected.IsActive = active);
            }

            CreateInspectorButtons(backgroundRect, onButtonUp, onButtonDown);

            Splitter(new Color(0.19f, 0.19f, 0.19f, 1.333f));

            selectedTree.BeginDraw(false);

            EditorGUI.BeginChangeCheck();
            selectedTree.DrawProperties();
            if (EditorGUI.EndChangeCheck())
            {
                onSelectedPropertyChanged?.Invoke(null);
            }

            selectedTree.EndDraw();

            EditorGUILayout.Space();
            Splitter(new Color(0.12f, 0.12f, 0.12f, 1.333f));
            EditorGUILayout.Space();
        }

        private static void CreateInspectorButtons(Rect backgroundRect, Action onButtonUp, Action onButtonDown)
        {
            const int rightMargin = 6;
            Rect downButtonRect = new(
                backgroundRect.xMax - InspectorButtonSize.x - rightMargin, backgroundRect.y,
                InspectorButtonSize.x, InspectorButtonSize.y
            );
            Rect upButtonRect = new(
                downButtonRect.x - InspectorButtonSize.x, downButtonRect.y,
                downButtonRect.width, downButtonRect.height
            );

            if (GUI.Button(upButtonRect, InspectorUpButton, InspectorButtonStyle))
            {
                onButtonUp?.Invoke();
            }

            if (GUI.Button(downButtonRect, InspectorDownButton, InspectorButtonStyle))
            {
                onButtonDown?.Invoke();
            }
        }

        private static void Splitter(Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 1f);
            rect = ToFullWidth(rect);
            EditorGUI.DrawRect(rect, color);
        }

        private static Rect ToFullWidth(Rect rect)
        {
            rect.xMin = 0f;
            rect.width += 4f;
            return rect;
        }

        public static float GetScaledTimeUnderMouse(Rect timeRect)
        {
            float time = (Event.current.mousePosition.x - timeRect.x) / timeRect.width;
            time = Mathf.Clamp01(time);
            return time;
        }

        private static Rect CalculateAddButtonRect(Rect timelineRect)
        {
            var buttonSize = new Vector2(32, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2,
                timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            return new Rect(position, buttonSize);
        }

        private static void RoundRect(Rect rect, Color color, float borderRadius, float borderWidth = 0)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false,
                0, color, borderWidth, borderRadius);
        }

        private static float CalculateX(Rect rowRect, float time, float timeScale)
        {
            return rowRect.x + time * timeScale * rowRect.width;
        }

        // Converts a base64 string to a Texture2D and caches it for future use
        public static Texture2D ImageFromString(string source)
        {
            if (IconCache.TryGetValue(source, out Texture2D cachedTexture) && cachedTexture != null)
            {
                return cachedTexture;
            }

            byte[] bytes = Convert.FromBase64String(source);
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            IconCache[source] = texture;

            return texture;
        }
    }
}
#endif
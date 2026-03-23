#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using UnityEngine;

namespace PTT.Editor
{
    public class ColorScope : IDisposable
    {
        private bool _disposed;

        private readonly Color _prevBackground;
        private readonly Color _prevContent;
        private readonly Color _prevMain;

        public ColorScope(Color? background, Color? content = null, Color? main = null)
        {
            _prevBackground = GUI.backgroundColor;
            _prevContent = GUI.contentColor;
            _prevMain = GUI.color;
            if (background.HasValue)
            {
                GUI.backgroundColor = background.Value;
            }

            if (content.HasValue)
            {
                GUI.contentColor = content.Value;
            }

            if (!main.HasValue)
            {
                return;
            }

            GUI.color = main.Value;
        }

        ~ColorScope()
        {
            if (_disposed)
            {
                return;
            }

            Debug.LogError("Scope was not disposed! You should use the 'using' keyword or manually call Dispose.");
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CloseScope();
        }

        private void CloseScope()
        {
            GUI.backgroundColor = _prevBackground;
            GUI.contentColor = _prevContent;
            GUI.color = _prevMain;
        }
    }
}
#endif
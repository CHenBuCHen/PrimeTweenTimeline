#if PRIME_TWEEN && ODIN_INSPECTOR
using System;
using UnityEngine;

namespace PTT.Editor
{
    public struct AddMoreItem
    {
        public readonly GUIContent Content;
        public readonly Type Type;

        public AddMoreItem(GUIContent content, Type type)
        {
            Content = content;
            Type = type;
        }
    }
}
#endif
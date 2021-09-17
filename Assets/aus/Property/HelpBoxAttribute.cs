// https://bitbucket.org/alkee/aus

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Property
{

    public class HelpBoxAttribute : PropertyAttribute
    {
        public enum IconType { NONE = 0, INFORMATION = 1, WARNING = 2, ERROR = 3 }
        public string Message;
        public IconType Icon;
        public HelpBoxAttribute(string message, IconType icon = IconType.INFORMATION)
        {
            Message = message;
            Icon = icon;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var attr = attribute as HelpBoxAttribute;
            if (attr == null) return 0;
            return EditorGUIUtility.singleLineHeight * 2.0f;
        }

        public override void OnGUI(Rect position)
        {
            var attr = attribute as HelpBoxAttribute;
            if (attr == null) return;

            EditorGUI.HelpBox(position, attr.Message, (MessageType)attr.Icon);
        }
    }
#endif

}
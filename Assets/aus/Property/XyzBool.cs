// https://bitbucket.org/alkee/aus

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Property
{
    [System.Serializable]
    public class XyzBool
    {
        public bool X;
        public bool Y;
        public bool Z;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(XyzBool))]
    public class XyzBoolDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty x = prop.FindPropertyRelative("X");
            SerializedProperty y = prop.FindPropertyRelative("Y");
            SerializedProperty z = prop.FindPropertyRelative("Z");

            EditorGUI.BeginProperty(pos, GUIContent.none, prop);

            pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect[] xyz = new Rect[3];
            pos.width /= 3.0f;
            xyz[0] = xyz[1] = xyz[2] = pos;
            xyz[1].x += pos.width;
            xyz[2].x = xyz[1].x + pos.width;

            // EditorGUI.PropertyField uses PrefixLabel internally. it requires minimal width of the control for label
            // to avoid large label area, draw custom label and its control without label(GUIContent.none)
            EditorGUI.LabelField(xyz[0], "X"); xyz[0].x += 16; EditorGUI.PropertyField(xyz[0], x, GUIContent.none);
            EditorGUI.LabelField(xyz[1], "Y"); xyz[1].x += 16; EditorGUI.PropertyField(xyz[1], y, GUIContent.none);
            EditorGUI.LabelField(xyz[2], "Z"); xyz[2].x += 16; EditorGUI.PropertyField(xyz[2], z, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
#endif

}

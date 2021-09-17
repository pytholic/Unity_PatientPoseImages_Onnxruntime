// https://bitbucket.org/alkee/aus

using System.Reflection;
using UnityEngine.Events;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Property
{
    public class InspectorInvokableAttribute : PropertyAttribute
    {
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(InspectorInvokableAttribute))]
    public class TInspectorInvokableDrawer : PropertyDrawer
    {
        private UnityEditorInternal.UnityEventDrawer drawer = new UnityEditorInternal.UnityEventDrawer();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsUnityEvent(property) == false)
            {
                position.height -= EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, property, label);
                position.y = position.y + position.height;
                EditorGUI.HelpBox(position, "Wrong target of TestableUnityEventAttribute", MessageType.Error);
            }
            else
            {
                // UnityEvent 와 같이 별도의 Cusom drawer 가 존재하는 경우 EditorGUI.PropertyField 로
                // property 를 그릴 수 없다.
                drawer.OnGUI(position, property, label);

                position.x += 1;
                position.y = position.y + position.height - EditorGUIUtility.singleLineHeight;
                position.height = EditorGUIUtility.singleLineHeight;
                position.width = EditorGUIUtility.labelWidth;

                GUI.enabled = Application.isPlaying;
                var btnText = GUI.enabled ? "Invoke to test" : "NOT playing";
                if (GUI.Button(position, new GUIContent(btnText)))
                {
                    // UnityEvent 는 Unity Object 가 아닌 일반 class 라서 property.objectReferenceValue 로
                    // 얻어올 수 없다.
                    var t = property.serializedObject.targetObject.GetType();
                    var f = t.GetField(property.name);
                    Debug.Assert(f != null);

                    var e = f.GetValue(property.serializedObject.targetObject) as UnityEvent;
                    Debug.Assert(e != null);

                    Debug.LogFormat("Test invoking {0} event in {1}", property.name, property.serializedObject.targetObject.name);
                    e.Invoke();
                }
                GUI.enabled = true;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return drawer.GetPropertyHeight(property, label)
                + (IsUnityEvent(property) ? 0 : EditorGUIUtility.singleLineHeight);
        }

        private bool IsUnityEvent(SerializedProperty property)
        {
            return property.type == "UnityEvent";
        }

    }

#endif

}

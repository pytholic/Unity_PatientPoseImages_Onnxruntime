using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif


namespace aus.Property
{
    public abstract class ReorderableList<T>
    {
        public List<T> members = new List<T>();
    }

    // CustomPropertyDrawer 가 generic 을 지원하지 않기 때문에 지원하는 type 들을 개별적으로
    // 만들어줘야 한다. 같은 방식으로 사용자 정의 ReorderableList 를 만들 수 있다.

    // [Serializable] public class BLABLA : ReorderableList<T> {}
    // #if UNITY_EDITOR [CustomPropertyDrawer(typeof(BLABLA))] public class BLABLA_Drawer : ReorderableListDrawer #end if

    // CustomPropertyDrawer 와 손쉬운 확인을 위해 알파벳 순 정렬된 순서로 선언
    [Serializable] public class Animators : ReorderableList<Animator> { }
    [Serializable] public class Colors : ReorderableList<Color> { }
    [Serializable] public class Floats : ReorderableList<float> { }
    [Serializable] public class GameObjects : ReorderableList<GameObject> { }
    [Serializable] public class Int32s : ReorderableList<Int32> { }
    [Serializable] public class MeshRenderers : ReorderableList<MeshRenderer> { }
    [Serializable] public class SkinnedMeshRenderers : ReorderableList<SkinnedMeshRenderer> { }
    [Serializable] public class Strings : ReorderableList<string> { }
    [Serializable] public class Transforms : ReorderableList<Transform> { }
    [Serializable] public class Vector3s : ReorderableList<Vector3> { }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(Animators))]
    [CustomPropertyDrawer(typeof(Colors))]
    [CustomPropertyDrawer(typeof(Floats))]
    [CustomPropertyDrawer(typeof(GameObjects))]
    [CustomPropertyDrawer(typeof(Int32s))]
    [CustomPropertyDrawer(typeof(MeshRenderers))]
    [CustomPropertyDrawer(typeof(SkinnedMeshRenderers))]
    [CustomPropertyDrawer(typeof(Strings))]
    [CustomPropertyDrawer(typeof(Transforms))]
    [CustomPropertyDrawer(typeof(Vector3s))]
    public class AusReorderableListDrawer : ReorderableListDrawer
    {
    }

    // 필요하다면 AusReorderableListDrawer 와 같이 상속해서 새로운 type 을 외부에서
    // 추가할 수 있을 것.
    public class ReorderableListDrawer : PropertyDrawer
    {
        private const string LIST_MEMBERS_NAME = "members"; // List<T>

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var members = property.FindPropertyRelative(LIST_MEMBERS_NAME);
            EditorGUI.BeginProperty(position, label, property);

            Debug.Assert(members != null, "list member should have [Serializable] attribute");

            if (list == null) list = BuildList(property.name, members);
            list.DoList(position);
            members.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (list == null)
            {
                var members = property.FindPropertyRelative(LIST_MEMBERS_NAME);
                Debug.Assert(members != null, "list member should have [Serializable] attribute");
                list = BuildList(property.name, members);
            }
            return list.GetHeight();
        }

        private static ReorderableList BuildList(string title, SerializedProperty members)
        {
            var list = new ReorderableList(members.serializedObject, members, true, true, true, true);
            list.drawHeaderCallback = (r) =>
           {
               EditorGUI.LabelField(r, title);
           };
            list.drawElementCallback = (r, index, isActive, isFocused) =>
            {
                EditorGUI.PropertyField(r, members.GetArrayElementAtIndex(index), GUIContent.none, true);
            };
            list.elementHeightCallback = (i) =>
            {
                return EditorGUI.GetPropertyHeight(members.GetArrayElementAtIndex(i));
            };

            // 이유는 모르겠으나... Unity5.6 에서 members.serializedObject.Update 가 
            //    OnGUI 에 있으면 다른 sibling member 들의 값 변경을 방해한다.
            members.serializedObject.Update();
            return list;
        }

        private ReorderableList list;
    }


#endif

}
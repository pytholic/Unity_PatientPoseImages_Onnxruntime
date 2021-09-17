// https://bitbucket.org/alkee/aus
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Debugging
{
    // GameObject 에 다는 주석. 중요한 내용이라면 가장 위에 배치하면 좋다.
    public class Comment : MonoBehaviour
    {
        [TextArea]
        public string _;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Comment))]
    public class CommentPropertyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = target as Comment;
            t._ = EditorGUILayout.TextArea(t._, GUILayout.MinHeight(32));
        }
    }
#endif

}

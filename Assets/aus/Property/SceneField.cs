// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace aus.Property
{
    // ref: https://answers.unity.com/questions/242794/inspector-field-for-scene-asset.html
    [System.Serializable]
    public class SceneField
    {
        [SerializeField]
        private Object m_SceneAsset;
        [SerializeField]
        private string m_SceneName = "";

        /// <summary>
        /// implicit operator string 이 있어 직접 이 멤버에 접근할 필요는 없을 것
        /// </summary>
        public string SceneName
        {
            get
            {
                return m_SceneName;
            }
        }

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect.height = base.GetPropertyHeight(property, label);
            EditorGUI.BeginProperty(rect, GUIContent.none, property);
            SerializedProperty sceneAsset = property.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneName = property.FindPropertyRelative("m_SceneName");
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null)
            {
                sceneAsset.objectReferenceValue = EditorGUI.ObjectField(rect, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
                if (sceneAsset.objectReferenceValue != null)
                {
                    sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
                    if (IsInvalidScene(property)==false)
                    {
                        rect.y += base.GetPropertyHeight(property, label);
                        rect.xMax -= 18; // select icon 영역
                        rect.height *= ERROR_BOX_HEIGHT_RATIO;
                        EditorGUI.HelpBox(rect, "this scene is not in the build-list", MessageType.Error);
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var defaultHeight = base.GetPropertyHeight(property, label);

            if (IsInvalidScene(property)) return defaultHeight;

            return defaultHeight * (1 + ERROR_BOX_HEIGHT_RATIO);
        }

        private bool IsInvalidScene(SerializedProperty property)
        {
            SerializedProperty sceneAsset = property.FindPropertyRelative("m_SceneAsset");
            if (sceneAsset == null || sceneAsset.objectReferenceValue == null) return false;

            var path = AssetDatabase.GetAssetPath(sceneAsset.objectReferenceValue);
            return (EditorBuildSettings.scenes.Any(x => x.path == path) == false);
        }

        private const float ERROR_BOX_HEIGHT_RATIO = 2.0f; // 좁은 inspector 영역에서도 두줄로 메시지를 잘 보이게 하기위해

    }

#endif
}

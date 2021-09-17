using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR // Editor 폴더에 포함되어있지 않기 때문에 ios 빌드시에 UnityEditor namepsace 를 찾지 못한다.
using UnityEditor;
#endif

namespace aus.Event
{
    // scene 을 편집하다보면 GameObject 들을 active(enable, visible)/inactive(disable, invisible) 시켜가면서
    // 작업하게 되는데, 편집상태와 상관없이 실제 플레이 시에 설정되어야 하는 값을 스크립트로 셋팅하기 위함.
    // 특정상태(에디터 편집)에서 시작하고 싶다면 이 컴포넌트만 disable 하면 될 것.
    public class EnableDisableInit : MonoBehaviour
    {
        public List<GameObject> EnableTargets = new List<GameObject>();
        public List<GameObject> DisableTargets = new List<GameObject>();

        void Awake()
        {
            SetActive(EnableTargets, true);
            SetActive(DisableTargets, false);
        }

        private void SetActive(List<GameObject> targets, bool value)
        {
            foreach (var t in targets)
            {
                if (t == null) continue;
                Debug.Log("[EnableDisableInit] " + (value ? "Enabling " : "Disabling ") + t.name);
                var isChild // 자손 전부를 포함하지 않으므로 transform.IsChildOf 를 사용할 수 없다.
                    = transform.Cast<Transform>().Any((x) => x == t.transform);
                if (isChild == false)
                {
                    Debug.LogWarning("[EnableDisableInit] " + t.name + " is not a child of " + name);
                    continue;
                }

                t.gameObject.SetActive(value);
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(EnableDisableInit))]
    public class EnableDisableInitInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = target as EnableDisableInit;

            EditorGUILayout.HelpBox("지정된 모든 GameObject 들은 바로 하위(child)의 GameObject 이어야 한다. 그외 자손(grandchild 등)의 경우 해당 부모에서 EnableDisableInit 을 사용해야한다", MessageType.None, true);
            EditorGUILayout.Space();

            showEnabledGameObject = EditorGUILayout.Foldout(showEnabledGameObject
                , new GUIContent("Enable on start(play)", "시작할 때 강제로 Active 시킬 항목"), true);
            if (showEnabledGameObject) ListGameObjects(t.gameObject, t.EnableTargets);
            EditorGUILayout.Space();

            showDisabledGameObject = EditorGUILayout.Foldout(showDisabledGameObject
                , new GUIContent("Disable on start(play)", "시작할 때 강제로 Inactive 시킬 항목"), true);
            if (showDisabledGameObject) ListGameObjects(t.gameObject, t.DisableTargets);
            EditorGUILayout.Space();

            if (t.EnableTargets.Count + t.DisableTargets.Count > 0
                && (t.EnableTargets.Count == 0 || t.DisableTargets.Count == 0))
            {
                EditorGUILayout.HelpBox("모든 자식이 같은 disable 또는 enable 상태로 시작해야 한다면 EnableDisableAllInit 을 사용하는 것으로 고려해 볼 것.", MessageType.Info, true);
                EditorGUILayout.Space();
            }
        }

        private bool showEnabledGameObject = true;
        private bool showDisabledGameObject = true;

        private void ListGameObjects(GameObject parent, List<GameObject> children)
        {
            for (var i = 0; i < children.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                var r = GUILayout.Button(new GUIContent("-", "Remove this"), GUILayout.ExpandWidth(false));
                var child = children[i];
                var isChild = parent.transform.Cast<Transform>().Any((t) => t == child.transform); // prev info
                var label = isChild ? "child GameObject" : "NOT CHILD";

                var prev = GUI.color;
                if (isChild == false) GUI.color = Color.red;
                child = EditorGUILayout.ObjectField(label, child, typeof(GameObject), true) as GameObject;
                GUI.color = prev;

                if (r || child == null)
                {
                    children[i] = null;
                }
                else
                {
                    children[i] = child;
                }
                EditorGUILayout.EndHorizontal();
            }
            children.RemoveAll((g) => g == null);
            var created = EditorGUILayout.ObjectField("Insert new CHILD", null, typeof(GameObject), true) as GameObject;
            if (created != null) children.Add(created);
        }
    }
#endif
}

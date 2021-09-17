// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.SceneManagement;

namespace aus.InterScene
{
    public class Dependency : MonoBehaviour
    {
        [Tooltip("loads additive scene automatically")]
        public Property.SceneField DependentScene;
        public bool SetAsActiveScene;

        void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            if (enabled == false) return;
            Debug.Assert(DependentScene != null && string.IsNullOrEmpty(DependentScene.SceneName) == false
                , "Dependent scene is not set");

            var thisScene = gameObject.scene;
            var count = SceneManager.sceneCount;
            // TODO: duplicated scene name ; AssetDatabase is only available in EDITOR
            var dependent = SceneManager.GetSceneByName(DependentScene.SceneName);

            // if the dependent is already loading somewhere else,
            //    isLoaded could be false BUT dependent.IsValid() is TRUE.
            if (dependent.IsValid() || dependent.isLoaded) return; // prevent duplication

            // TODO: asyncronous load option
            SceneManager.LoadScene(DependentScene.SceneName, LoadSceneMode.Additive);
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SetAsActiveScene && scene.name == DependentScene.SceneName)
            {
                SceneManager.SetActiveScene(scene);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void OnValidate()
        {
            if (SetAsActiveScene)
            {
                // Active scene 은 하나밖에 있을 수 없으므로 모든 Dependency 를 확인해 수정
                var deps = FindObjectsOfType<Dependency>();
                foreach (var d in deps)
                {
                    if (d != this && d.SetAsActiveScene)
                    {
                        Debug.LogWarning(d.DependentScene.SceneName + " has marked out from SetAsActiveScene");
                        d.SetAsActiveScene = false;
                    }
                }
            }
        }

        // just to use 'enabled' in inspector
        void Start()
        {
            Debug.Assert(DependentScene != null);
        }
    }
}

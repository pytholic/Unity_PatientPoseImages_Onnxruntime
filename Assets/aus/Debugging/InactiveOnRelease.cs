// https://bitbucket.org/alkee/aus
using UnityEngine;

namespace aus.Debugging
{
    public class InactiveOnRelease : MonoBehaviour
    {
        public bool NoInactiveOnDevelopmentBuild = true;

        void Awake()
        {
            if (Application.isEditor == false && Debug.isDebugBuild == false)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

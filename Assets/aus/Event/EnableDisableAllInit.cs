using UnityEngine;

namespace aus.Event
{
    public class EnableDisableAllInit : MonoBehaviour
    {
        [Tooltip("true 이면 모든 직접적인 자식(자손 제외)만 강제 disable. false 이면 enable")]
        public bool DisableAllChildren;

        private void Awake()
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(!DisableAllChildren);
            }
        }
    }
}
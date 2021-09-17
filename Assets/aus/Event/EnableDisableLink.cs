using UnityEngine;

// Enable/Disable 시에 함께 Enable/Disable 이 변할 개체 연결
namespace aus.Event
{
    public class EnableDisableLink : MonoBehaviour
    {
        [Tooltip("Enable/Disable 변할 때 함께 Enable/Disable 변할 개체")]
        public GameObject LinkTarget;

        [Tooltip("True 이면 Enable 될때 함께 Target 을 Enable 시키고 그렇지 않으면 반대")]
        public bool Reversed = false;

        void OnEnable()
        {
            if (LinkTarget != null) LinkTarget.SetActive(!Reversed);
        }

        void OnDestroy()
        {
            LinkTarget = null; // object 가 destory 될때에도 OnDisable 이 불린다.
        }

        void OnDisable()
        {
            if (LinkTarget != null) LinkTarget.SetActive(Reversed);
        }
    }
}

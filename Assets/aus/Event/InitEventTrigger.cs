// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.Events;

namespace aus.Event
{
    public class InitEventTrigger : MonoBehaviour
    {
        public UnityEvent OnAwake;
        public UnityEvent OnStart;

        void Awake()
        {
            if (OnAwake != null) OnAwake.Invoke();
        }

        // Use this for initialization
        void Start()
        {
            if (OnStart != null) OnStart.Invoke();
        }
    }
}

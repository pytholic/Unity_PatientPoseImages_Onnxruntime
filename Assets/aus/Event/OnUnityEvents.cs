// https://bitbucket.org/alkee/aus

using aus.Property;
using UnityEngine;
using UnityEngine.Events;


namespace aus.Event
{
    public abstract class OnUnityEvent : MonoBehaviour
    {
        [InspectorInvokable]
        public UnityEvent Jobs;
    }

    public class OnEnableEvent : OnUnityEvent
    {
        private void OnEnable()
        {
            Jobs.Invoke();
        }
    }

    public class OnDisableEvent : OnUnityEvent
    {
        private void OnDisable()
        {
            Jobs.Invoke();
        }
    }

    public class AwakeEvent : OnUnityEvent
    {
        private void Awake()
        {
            Jobs.Invoke();
        }
    }

    public class StartEvent : OnUnityEvent
    {
        private void Start()
        {
            Jobs.Invoke();
        }
    }

    public class OnDestroyEvent : OnUnityEvent
    {
        private void OnDestroy()
        {
            Jobs.Invoke();
        }
    }
}

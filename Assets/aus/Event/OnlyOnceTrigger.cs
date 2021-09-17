// https://bitbucket.org/alkee/aus

using UnityEngine;
using UnityEngine.Events;

namespace aus.Event
{
    // *CAUSION* this may not work if the target UnityEvent is changed somewhere else
    public class OnlyOnceTrigger : MonoBehaviour
    {
        [Property.PropertyOrField(typeof(UnityEvent))]
        public Property.ComponentMemberField TargetField;
        public UnityEvent OnFirstInvoke;

        private UnityEvent target;

        private void Start()
        {
            Debug.Assert(TargetField != null);
        }

        private void OnEnable()
        {
            if (TargetField != null)
            {
                target = TargetField.GetValue() as UnityEvent;
                if (target != null)
                {
                    target.AddListener(OnInvoke);
                }
            }
        }

        private void OnInvoke()
        {
            target.RemoveListener(OnInvoke);
            TargetField = null; // no more use
            OnFirstInvoke.Invoke();
        }
    }

}

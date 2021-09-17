// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.Events;

namespace aus.Event
{
    public class PhysicsTrigger : MonoBehaviour
    {
        public Collider Collider;
        [Tooltip("None means any")]
        public Rigidbody Target;

        // OnTriggerEnter, ... already taken by unity event
        [Header("Events")]
        public UnityEvent OnEnterTrigger;
        public UnityEvent OnExitTrigger;
        public UnityEvent OnEnterCollision;
        public UnityEvent OnExitCollision;

        private void Start()
        {
            if (Collider == null)
            {
                enabled = false;
                Debug.LogWarning("PhysicsTrigger disabled by null Collider in " + name);
                return;
            }
        }

        private void Reset()
        {
            if (Collider == null) Collider = GetComponent<Collider>();
        }

        //private void OnValidate()
        //{
        //    // collider/target validation (trigger or collision)
        //}

        private void OnTriggerEnter(Collider other)
        {
            if (Target == null || other.gameObject == Target.gameObject)
            {
                if (OnEnterTrigger != null) OnEnterTrigger.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (Target == null || other.gameObject == Target.gameObject)
            {
                if (OnExitTrigger != null) OnExitTrigger.Invoke();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Target == null || collision.rigidbody == Target)
            {
                if (OnEnterCollision != null) OnEnterCollision.Invoke();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (Target == null || collision.rigidbody == Target)
            {
                if (OnExitCollision != null) OnExitCollision.Invoke();
            }
        }
    }

}

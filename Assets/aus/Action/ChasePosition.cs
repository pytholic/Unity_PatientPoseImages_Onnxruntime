// https://bitbucket.org/alkee/aus

using UnityEngine;
using UnityEngine.Events;

namespace aus.Action
{
    public class ChasePosition : MonoBehaviour
    {
        [SerializeField]
        private Transform chasingTarget;
        public Transform ChasingTarget { get { return chasingTarget; } set { chasingTarget = value; } }
        [SerializeField]
        private float chasingSpeed = 1;
        public float ChasingSpeed { get { return chasingSpeed; } set { chasingSpeed = value; } }

        [Tooltip("이 거리보다 먼 경우에만 chasing")]
        [SerializeField]
        private float stopDistance = 0.5f; // TODO: editor 에서 stop distance gizmo 표시
        public float StopDistance { get { return stopDistance; } set { stopDistance = value; } }

        // XyzBool type 은 property 를 제공하더라도 UnityEvent inspector 에서 보여지지 않으므로 단순 public 변수로
        public Property.XyzBool FreezeAxis = new Property.XyzBool { Y = true };

        [Header("Events")]
        public UnityEvent OnDeparture;
        public UnityEvent OnArrival;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void OnDisable()
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero; // stop
            }
        }

        void Update()
        { // rigid body 를 이용하지 않는 경우
            if (ChasingTarget == null || (rb != null && rb.isKinematic == false)) return;
            if (IsInStopDistance()) return;

            var direction = GetNormalizedTargetDirection();

            var dt = Time.deltaTime;
            transform.position += direction * dt * ChasingSpeed;
        }

        void FixedUpdate()
        { // rigid body 를 이용하는 경우
            if (ChasingTarget == null || rb == null || rb.isKinematic) return;
            if (IsInStopDistance()) return;

            var direction = GetNormalizedTargetDirection();

            // TODO: 속도가 바로 줄지 않기 때문에 IsInStopDistance 보다 이른 시간에 멈춰야 할텐데..
            // same as rb.velocity = direction * ChasingSpeed;
            // but AddForce makes boundary force on collision
            rb.AddForce(direction * ChasingSpeed - rb.velocity, ForceMode.VelocityChange);
        }

        private Rigidbody rb = null;
        private bool moving = false;

        private bool IsInStopDistance()
        {
            var shoudStop = Vector3.Distance(transform.position, ChasingTarget.position) < StopDistance;
            if (shoudStop && moving)
            {
                moving = false;
                OnArrival.Invoke();
            }
            else if (shoudStop == false && moving == false)
            {
                moving = true;
                OnDeparture.Invoke();
            }
            return shoudStop;
        }

        private Vector3 GetNormalizedTargetDirection()
        {
            if (ChasingTarget == null) return Vector3.zero;
            var direction = ChasingTarget.position - transform.position;
            if (FreezeAxis.X) direction.x = 0.0f;
            if (FreezeAxis.Y) direction.y = 0.0f;
            if (FreezeAxis.Z) direction.z = 0.0f;
            if (direction == Vector3.zero) return Vector3.zero;
            direction.Normalize();
            return direction;
        }
    }
}

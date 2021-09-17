// https://bitbucket.org/alkee/aus

using UnityEngine;

namespace aus.Action
{
    public class ChaseDirection : MonoBehaviour
    {
        public Transform ChasingTarget;
        public float ChasingSpeed = 1;
        public Property.XyzBool FreezeAxis = new Property.XyzBool { X = true, Z = true };

        public void ChangeChasingTarget(Transform target)
        {
            ChasingTarget = target;
        }

        void Update()
        {
            if (ChasingTarget == null) return;

            var direction = ChasingTarget.position - transform.position;
            if (FreezeAxis.X == false) direction.x = 0.0f;
            if (FreezeAxis.Y == false) direction.y = 0.0f;
            if (FreezeAxis.Z == false) direction.z = 0.0f;
            if (direction == Vector3.zero) return;

            direction.Normalize();
            var dt = Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation
                , Quaternion.LookRotation(direction)
                , dt * ChasingSpeed);
        }
    }
}

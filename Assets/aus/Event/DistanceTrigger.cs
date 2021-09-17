// https://bitbucket.org/alkee/aus

using aus.Property;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace aus.Event
{
    /// <summary>
    /// Raising event by object distance
    /// </summary>
    public class DistanceTrigger : MonoBehaviour
    {
        public Transform Source;
        public Transform Target;

        [MinMaxRange(0.0f, 100.0f, true)]
        public MinMaxRange EffectiveDistance;

        [Header("Events")]
        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        public void ChangeTarget(Transform target)
        {
            Target = target;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Source.transform == null) return;

            // variables
            var style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            var h = new Handles();
            var dir = -h.currentCamera.transform.right; // always left from camera
            var start = Source.transform.position + dir * EffectiveDistance.rangeStart;
            var end = Source.transform.position + dir * EffectiveDistance.rangeEnd;

            // represents EffectiveDistance - area
            Gizmos.color = new Color(0, 0, 0, 0.5f);
            Gizmos.DrawSphere(Source.transform.position, EffectiveDistance.rangeStart);
            Handles.color = Color.black;
            Handles.RadiusHandle(Quaternion.identity, Source.transform.position, EffectiveDistance.rangeStart, false);
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawSphere(Source.transform.position, EffectiveDistance.rangeEnd);
            Handles.color = Color.white;
            Handles.RadiusHandle(Quaternion.identity, Source.transform.position, EffectiveDistance.rangeEnd, false);

            // represents EffectiveDistance - length
            Handles.color = Color.green;
            Handles.DrawDottedLine(start, end, 2);
            style.normal.textColor = Color.yellow;
            Handles.Label((start + end) * 0.5f, "\nEffectiveDistance", style);

            // represents Source
            style.normal.textColor = Color.blue;
            Handles.Label(Source.transform.position, "Source", style);

            if (Target.transform == null) return;
            // represents Target
            style.normal.textColor = Color.red;
            Handles.Label(Target.transform.position, "Target", style);
        }
#endif

        private void Reset()
        {
            if (Source == null) Source = transform;
        }

        private bool prevInRange = false;
        void Update()
        {
            if (Target == null || Source == null) return;

            var distance = Vector3.Distance(Target.position, Source.position);
            if (EffectiveDistance.IsIn(distance))
            { // looking
                if (prevInRange == false)
                {
                    prevInRange = true;
                    if (OnEnter != null) OnEnter.Invoke();
                }
            }
            else if (prevInRange == true)
            {
                prevInRange = false;
                if (OnExit != null) OnExit.Invoke();
            }

        }
    }
}

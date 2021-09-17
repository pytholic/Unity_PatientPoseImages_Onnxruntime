// https://bitbucket.org/alkee/aus
using UnityEngine;
using UnityEngine.EventSystems;

namespace aus.Action
{
    // modified by alkee
    // from https://forum.unity3d.com/threads/fly-cam-simple-cam-script.67042/
    public class FpsMotion : MonoBehaviour
    {
        /*
         * Based on Windex's flycam script found here: http://forum.unity3d.com/threads/fly-cam-simple-cam-script.67042/
         * C# conversion created by Ellandar
         * Improved camera made by LookForward
         * Modifications created by Angryboy
         * 1) Have to hold right-click to rotate
         * 2) Made variables public for testing/designer purposes
         * 3) Y-axis now locked (as if space was always being held)
         * 4) Space/Ctrl keys are used to raise/lower the camera(like jump/crouch of FPS games)
         *
         * Another Modification created by micah_3d
         * 1) adding an isColliding bool to allow camera to collide with world objects, terrain etc.
         */

        [Tooltip("use size than position for orthogonal camera")]
        public bool cameraDetection = true;
        public float mouseSensitivity = 2.5f; // Mouse rotation sensitivity.
        public float mainSpeed = 4.0f; // regular speed
        [Range(0.0f, 1.0f)]
        public float preciseRate = 0.2f; // to make precise speed

        [Header("Key bindings")]
        public KeyCode Precise = KeyCode.LeftShift;
        public KeyCode MoveUp = KeyCode.E;
        public KeyCode MoveDown = KeyCode.Q;
        public KeyCode MoveFoward = KeyCode.W;
        public KeyCode MoveBackward = KeyCode.S;
        public KeyCode StepLeft = KeyCode.A;
        public KeyCode StepRight = KeyCode.D;


        private Camera cam;

        // physics and camera scpecific parts are removed
        // because it's more simple and independent.
        // if you want some like physics, you may add other components for it.

        void Start()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            if (EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject(1))
            {
                return; // see http://answers.unity3d.com/questions/784617/how-do-i-block-touch-events-from-propagating-throu.html
            }

            // Angryboy: Hold right-mouse button to rotate
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
            }
            if (Input.GetMouseButtonUp(1))
            {
                isRotating = false;
            }
            if (isRotating)
            {
                if (transform.rotation != lastRot)
                { // something moved camera
                    rotationY = -transform.localEulerAngles.x;
                    if (rotationY < -180) rotationY += 360;
                }

                // Made by LookForward
                // Angryboy: Replaced min/max Y with numbers, not sure why we had variables in the first place
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;
                rotationY = Mathf.Clamp(rotationY, -90, 90);
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, transform.localEulerAngles.z);

                lastRot = transform.rotation;
            }

            // Keyboard commands
            // TODO: find more good way to detect unwanted situation of keyboard command
            if (EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject != null
                && EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
            {
                return;
            }

            var p = GetBaseInput();
            var dp = p * Time.deltaTime * mainSpeed;
            transform.Translate(dp);

            // orthographic camera 를 사용하는 경우, 이동으로는 거리를 맞출 수 없다.
            if (IsOrthogonalCamera)
            {
                if (Input.GetKey(MoveFoward))
                {
                    cam.orthographicSize *= 0.98f;
                }
                else if (Input.GetKey(MoveBackward))
                {
                    cam.orthographicSize *= 1.02f;
                }
            }
        }

        private bool IsOrthogonalCamera
        {
            get
            {
                if (cameraDetection == false) return false;
                return cam != null && cam.orthographic;
            }
        }

        private bool isRotating = false; // Angryboy: Can be called by other things (e.g. UI) to see if camera is rotating
        private float rotationY = 0.0f;
        private Quaternion lastRot;

        private Vector3 GetBaseInput()
        { //returns the basic values, if it's 0 than it's not active.
            Vector3 p_Velocity = new Vector3();
            if (Input.GetKey(MoveFoward))
            {
                p_Velocity += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(MoveBackward))
            {
                p_Velocity += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(StepLeft))
            {
                p_Velocity += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(StepRight))
            {
                p_Velocity += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(MoveUp))
            {
                p_Velocity += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(MoveDown))
            {
                p_Velocity += new Vector3(0, -1, 0);
            }
            if (p_Velocity.sqrMagnitude > 0) p_Velocity.Normalize();
            return p_Velocity * (Input.GetKey(Precise) ? preciseRate : 1.0f);
        }
    }
}


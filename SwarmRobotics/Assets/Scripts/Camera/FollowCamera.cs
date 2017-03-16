using UnityEngine;

namespace Cameras
{
    public class FollowCamera : MonoBehaviour
    {
        public Camera cam;
        private GameObject target;
        private Vector3 targetOffset;

        // Use this for initialization
        void Start()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4;

            targetOffset = new Vector3(25, 20.5f, -25);
        }

        // LateUpdate is called once per frame after all other Update() methods
        void LateUpdate()
        {
            if (target != null)
            {
                gameObject.transform.position = target.transform.position + targetOffset;
            }
        }

        public void setTarget(GameObject target)
        {
            this.target = target;
            gameObject.transform.position = target.transform.position + targetOffset;
        }

        public void setTargetOffset(Vector3 targetOffset)
        {
            this.targetOffset = targetOffset;
        }
    }
}

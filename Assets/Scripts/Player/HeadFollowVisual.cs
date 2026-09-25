using UnityEngine;

namespace JuiceGalaxy
{
    /// <summary>Keeps a purely cosmetic transform under the headset horizontally, at a fixed body height.</summary>
    public class HeadFollowVisual : MonoBehaviour
    {
        public Transform head;
        public Transform bodyRoot;
        public float heightOffset = -0.5f;
        public float followSpeed = 8f;

        void LateUpdate()
        {
            if (head == null || bodyRoot == null) return;
            Vector3 target = new Vector3(head.position.x, bodyRoot.position.y + Mathf.Max(0.3f, head.localPosition.y + heightOffset), head.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSpeed);

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * followSpeed);
        }
    }
}

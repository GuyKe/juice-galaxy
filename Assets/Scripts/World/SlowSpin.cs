using UnityEngine;

namespace JuiceGalaxy
{
    public class SlowSpin : MonoBehaviour
    {
        public float speed = 3f;

        void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
        }
    }
}

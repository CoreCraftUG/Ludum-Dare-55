using UnityEngine;

namespace CoreCraft.LudumDare55
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private void Start()
        {
            transform.LookAt(transform.position + _camera.transform.forward);
        }
    }
}

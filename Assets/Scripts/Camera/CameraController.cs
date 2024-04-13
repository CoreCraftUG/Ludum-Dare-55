using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreCraft.Core
{
    public class CameraController : MonoBehaviour
    {
        private bool _canRotate;
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _movementSpeed;
        private Quaternion _startingPos;
        private float _xMovement;
        private float _zMovement;


        private void Awake()
        {
            _canRotate = false;
            _startingPos.eulerAngles = this.transform.eulerAngles;
            GameInputManager.Instance.OnCanRotateCamera += CanRotateCamera;
            GameInputManager.Instance.OnResetCamera += ResetCamera;
            GameInputManager.Instance.OnMoveCamera += MoveCamera;
            
        }


        private void Update()
        {
            if (_canRotate)
            {
                transform.eulerAngles += _rotationSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
            }

            if(_zMovement != 0 || _xMovement != 0)
            {
                transform.position += _movementSpeed * new Vector3(_xMovement, 0, _zMovement);
            }
        }

        private void CanRotateCamera(object sender, System.EventArgs e)
        {
            _canRotate = !_canRotate;
        }

        private void ResetCamera(object sender, System.EventArgs e)
        {
            this.transform.eulerAngles = _startingPos.eulerAngles;
        }

        private void MoveCamera(object sender, Vector2 e)
        {
            _xMovement = e.x;
            _zMovement = e.y;
        }
    }
}

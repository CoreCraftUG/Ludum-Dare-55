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
        [SerializeField] public float _movementSpeed;
        private Quaternion _startingPos;
        public float _xMovement;
        private float _zMovement;
        private float _zoomValue;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private float _minZoom;
        [SerializeField] private float _maxZoom;
        private float _baseZoom;
        [SerializeField] private float _maxX;
        [SerializeField] private float _minX;
        [SerializeField] private float _maxZ;
        [SerializeField] private float _minZ;
        [SerializeField] private Vector3 _startPos;

        private void Awake()
        {
            _startPos = transform.position;
            _canRotate = false;
            _startingPos.eulerAngles = this.transform.eulerAngles;
            _baseZoom = Camera.main.orthographicSize;
            GameInputManager.Instance.OnCanRotateCamera += CanRotateCamera;
            GameInputManager.Instance.OnResetCamera += ResetCamera;
            GameInputManager.Instance.OnMoveCamera += MoveCamera;
            GameInputManager.Instance.OnZoom += Instance_OnZoom;
        }

        private void Instance_OnZoom(object sender, float e)
        {
            _zoomValue = e/120;
        }

        private void Update()
        {
            //if (_canRotate)
            //{
            //    transform.eulerAngles += _rotationSpeed * new Vector3(0, Input.GetAxis("Mouse X"), 0);
            //}

            if(_zMovement != 0 || _xMovement != 0)
            {
                transform.position = _movementSpeed * new Vector3(Mathf.Clamp(transform.position.x + _xMovement * _movementSpeed, _minX, _maxX), transform.position.y, Mathf.Clamp(transform.position.z + _zMovement * _movementSpeed, _minZ, _maxZ));
            }

            if(_zoomValue != 0)
            {
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + _zoomSpeed * _zoomValue * -1, _minZoom, _maxZoom);
            }
        }

        private void CanRotateCamera(object sender, System.EventArgs e)
        {
            _canRotate = !_canRotate;
        }

        private void ResetCamera(object sender, System.EventArgs e)
        {
            this.transform.position = _startPos;
            Camera.main.orthographicSize = _baseZoom;
        }



        private void MoveCamera(object sender, Vector2 e)
        {
            _xMovement = e.x;
            _zMovement = e.y;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Hardware;

public class DebugCameraMovement : MonoBehaviour {

    [SerializeField]
    private float _speed = 1.0f;
    [SerializeField]
    private Transform _playerTransform; 
    private Transform _cameraTransform;
    private InputManager _inputManager;

    enum CameraDirection {
        None,
        Left, 
        Right
    }

    private void Awake() {
        _inputManager = InputManager.instance;
        _cameraTransform = Camera.main.transform;
    }

    private void OnEnable() {
        if (_inputManager != null) {
            //_inputManager.gameInput.OnPrimaryButtonPress += MoveCameraLeft;
            //_inputManager.gameInput.OnSecondaryButtonPress += MoveCameraRight;
        }
    }

	private void Update() {
        transform.position = _playerTransform.position;
	}

	private void MoveCameraLeft(HardwareInput input) {
        MoveCameraInDirection(CameraDirection.Left);
    }

    private void MoveCameraRight(HardwareInput input) {
        MoveCameraInDirection(CameraDirection.Right);
    }

    private void MoveCameraInDirection(CameraDirection direciton) {
        Vector3 newPosition;
        switch (direciton) {
            case CameraDirection.Left: {
                newPosition = Vector3.Lerp(_cameraTransform.position, new Vector3(-1.0f * _speed, _cameraTransform.transform.position.y, _cameraTransform.position.z), Time.deltaTime);
                break;
            }
            case CameraDirection.Right: {
                newPosition = Vector3.Lerp(_cameraTransform.position, new Vector3( _speed, _cameraTransform.transform.position.y, _cameraTransform.position.z), Time.deltaTime);
                break;
            }
            default: {
                newPosition = _cameraTransform.position;
                break;
            }
        }
        _cameraTransform.position = newPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBackground : MonoBehaviour {

    [SerializeField]
    private Transform[]     _tiles;
    [SerializeField]
    private float           _paralaxSpeed = 1.0f;
    private float           _cameraSize;
    private float           _cameraPreviousXPosition;
    private float           _tileSize;
    private float           _defaultY;
    private float           _defaultZ;
    private Transform       _cameraTransform;
    private float           _viewZone;
    private int             _leftIndex;
    private int             _rightIndex;

    private void Awake() {
        _cameraTransform = Camera.main.transform;

        // Cache the viewport size to determine when to tile left or right
        _cameraSize = Camera.main.ViewportToWorldPoint(Vector2.one).x;

        if (_tiles != null && _tiles.Length != 0) {
            _leftIndex = 0;
            _rightIndex = _tiles.Length - 1;

            if (_tiles[_rightIndex] != null && _tiles[_leftIndex] != null) {
                _tileSize = Mathf.Abs(_tiles[_rightIndex].position.x - _tiles[_leftIndex].position.x) / 2.0f;

                // Initialze these values using left index, it doesnt really matter which one you choose
                _defaultY = _tiles[_leftIndex].position.y;
                _defaultZ = _tiles[_leftIndex].position.z;
            } else {
                Debug.LogError("Arena Background does not have tile transforms assigned");
                gameObject.SetActive(false);
            }

        } else {
            Debug.LogError("Arena background is attempting to initialize a null or empty tile list");
            gameObject.SetActive(false);
        }
    }

	void Update () {

        float cameraCurrentX = _cameraTransform.position.x;
        float deltaX = _cameraPreviousXPosition - cameraCurrentX;
        transform.position += Vector3.right * deltaX * _paralaxSpeed;
        _cameraPreviousXPosition = cameraCurrentX;

        if ((cameraCurrentX + _cameraSize) > _tiles[_rightIndex].position.x) {
            ExtendTileRight();
        }

        if ((cameraCurrentX - _cameraSize) < _tiles[_leftIndex].position.x) {
            ExtendTileLeft();
        }
	}

    private void ExtendTileLeft() {
        _tiles[_rightIndex].position = new Vector3(_tiles[_leftIndex].position.x - _tileSize, _defaultY, _defaultZ);
        _leftIndex = _rightIndex;
        _rightIndex--;

        if (_rightIndex < 0) {
            _rightIndex = _tiles.Length - 1;
        }
    }

    private void ExtendTileRight() {
        _tiles[_leftIndex].position = new Vector3(_tiles[_rightIndex].position.x + _tileSize, _defaultY, _defaultZ);
        _rightIndex = _leftIndex;
        _leftIndex++;

        if (_leftIndex == _tiles.Length) {
            _leftIndex = 0;
        }
    }
}

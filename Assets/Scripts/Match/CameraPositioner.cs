using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace KickDive.Match {
    public class CameraPositioner : MonoBehaviour {

        private Camera _gameCamera;

        [Header("Settings")]
        [SerializeField]
        private float _cameraZoomSpeed = 3.0f;

        [SerializeField]
        private float _cameraMovementSpeed = 3.0f;

        [SerializeField]
        private float _minimumOrthographicCameraSize;

        [SerializeField]
        private float _cameraZoomScale = 0.5f;

        [SerializeField]
        private Transform _playerLeftTransform;

        [SerializeField]
        private Transform _playerRightTransform;

        private Transform _localPlayerTransform;
        private Transform _remotePlayerTransform;

        public bool shouldPosition { get; private set; }

        private void Awake() {
            _gameCamera = Camera.main;
            shouldPosition = false;
            
            // Initialize camera zoom
            _gameCamera.orthographicSize = (Vector2.Distance(_playerLeftTransform.position, _playerRightTransform.position) / 2.0f) *_cameraZoomScale;
        }

        public void InitializePlayers() {
            GameObject playerGameObject = MatchManager.instance.playerGameObject;
            GameObject remotePlayerGameObject = MatchManager.instance.otherPlayerGameObject;

            if ((playerGameObject != null) && (remotePlayerGameObject != null)) {
                _localPlayerTransform = playerGameObject.transform;
                _remotePlayerTransform = remotePlayerGameObject.transform;

                shouldPosition = true;
            } else {
                Debug.LogError("A player transform is missing when attempting to initialize camera positioner");
            }
        }

        public void DisableCameraPositioning() {
            shouldPosition = false;
        }

        private void PositionCameraCenter(Vector3 leftPlayerPosition, Vector3 rightPlayerPosition) {
            // Scales zoom based on distance between players, Super Smash bros style!
            float distanceBetweenPlayers = Vector2.Distance(leftPlayerPosition, rightPlayerPosition);
            
            // Halfway between the two players
            float xAverageBetweenPlayers = Vector2.Lerp(leftPlayerPosition, rightPlayerPosition, 0.5f).x;

            // Divides by two because the camera size is from center of screen to top of screen
            float newCameraSize = Mathf.Lerp(_gameCamera.orthographicSize, (distanceBetweenPlayers / 2.0f) * _cameraZoomScale, _cameraZoomSpeed * Time.deltaTime);

            if (newCameraSize > _minimumOrthographicCameraSize) {
                _gameCamera.orthographicSize = newCameraSize;
            }

            // Center the camera between the two players
            _gameCamera.transform.position = new Vector3(xAverageBetweenPlayers, _gameCamera.transform.position.y, _gameCamera.transform.position.z);
        }

        // Update is called once per frame
        void Update() {
            if (shouldPosition) {
                PositionCameraCenter(_localPlayerTransform.position, _remotePlayerTransform.position);
            }
        }
    }
}

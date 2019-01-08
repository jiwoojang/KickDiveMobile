using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A quick loading circle class using this resource:
// https://www.salusgames.com/2017/01/08/circle-loading-animation-in-unity3d/
// Some modifications were made
namespace KickDive.UI {
    public class LoadingCircle : MonoBehaviour {

        [SerializeField]
        private float           _rotateSpeed = 200f;

        [SerializeField]
        private GameObject      _progressBar;

        private RectTransform   _rectComponent;
        public bool             _isLoading      { get; private set; }

        public void StarLoading() {
            _isLoading = true;
        }

        public void StopLoading() {
            _isLoading = false;
        }

        private void Awake() {
            if (_progressBar != null) {
                _rectComponent = _progressBar.GetComponent<RectTransform>();
            } else {
                Debug.LogError("No progress bar has been selected");
            }
        }

        private void Update() {
            if (_isLoading) {
                _rectComponent.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.UI {
    public class MainMenuUI : MonoBehaviour {

        [SerializeField]
        private GameObject _playButton;

        [SerializeField]
        private LoadingCircle _loadingCircle;

        public void StartGame() {
            // Disable the button
            _playButton.SetActive(false);

            // Turn on the loading circle and start the load
            _loadingCircle.gameObject.SetActive(true);
            _loadingCircle.StarLoading();
        }

        public void ConnectSuccessful() {
            // Turn off the loading wheel
            _loadingCircle.StopLoading();
        }

        public void ConnectFailed() {
            // Reset all buttons and log an error
            _loadingCircle.StopLoading();
            _loadingCircle.gameObject.SetActive(false);

            _playButton.SetActive(true);
        }
    }
}

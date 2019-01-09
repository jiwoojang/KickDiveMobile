using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace KickDive.UI {
    public class MenuUIManager : MonoBehaviour {

        [SerializeField]
        private GameObject _playButton;

        [SerializeField]
        private LoadingCircle _loadingCircle;

        private NetworkManager _networkManagerInstance;

        private void Awake() {
            if (NetworkManager.instance != null) {
                _networkManagerInstance = NetworkManager.instance;
            } else {
                Debug.LogError("Could not find NetworkManager instance");
                return;
            }

            _networkManagerInstance.OnConnectedToPhotonMaster += OnConnectedToMaster;
        }

        public void StartGame() {
            // Disable the button
            _playButton.SetActive(false);

            // Turn on the loading circle and start the load
            _loadingCircle.gameObject.SetActive(true);
            _loadingCircle.StarLoading();

            _networkManagerInstance.ConnectToMaster();

            // Start the timeout coroutine
            StartCoroutine(PhotonMasterConnectionTimer());
        }

        public void OnConnectedToMaster() {
            Debug.Log("MenuUIManager connected to master");

            _loadingCircle.StopLoading();
        }

        IEnumerator PhotonMasterConnectionTimer() {
            // If we cannot connect to the master after 5 seconds, bail
            yield return new WaitForSecondsRealtime(5);

            if (!_networkManagerInstance.isConnectedToMaster) {
                // Reset all buttons and log an error
                _loadingCircle.StopLoading();
                _loadingCircle.gameObject.SetActive(false);

                _playButton.SetActive(true);

                // TODO: Replace this with a player facing error so they know to try again
                Debug.LogError("Could not connect to Photon Master");
            }
        }
    }
}

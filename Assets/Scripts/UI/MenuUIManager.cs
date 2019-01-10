using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace KickDive.UI {
    public class MenuUIManager : MonoBehaviour {

        // References to three main menu screens
        [SerializeField]
        private MainMenuUI _mainMenu;

        [SerializeField]
        private GameObject _joinHostMenu;

        [SerializeField]
        private GameObject _roomNameMenu;

        // Fade to black and back transition
        [SerializeField]
        private TransitionScreenUI _transitionScreen;

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

        public void ConnectToMaster() {
            // Tell photon to connect
            _networkManagerInstance.ConnectToMaster();

            // Start the timeout coroutine
            StartCoroutine(PhotonMasterConnectionTimer());
        }

        public void OnConnectedToMaster() {
            Debug.Log("MenuUIManager connected to master");

            _mainMenu.ConnectSuccessful();

            // Transition to next menu
            _transitionScreen.Transition(TransitionScreenUI.MenuStages.MainMenu, TransitionScreenUI.MenuStages.JoinHost);
        }

        IEnumerator PhotonMasterConnectionTimer() {
            // If we cannot connect to the master after 5 seconds, bail
            yield return new WaitForSecondsRealtime(5);

            if (!_networkManagerInstance.isConnectedToMaster) {
                _mainMenu.ConnectFailed();

                // TODO: Replace this with a player facing error so they know to try again
                Debug.LogError("Could not connect to Photon Master");
            }
        }
    }
}

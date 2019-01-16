using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

namespace KickDive.UI {
    public class MenuUIManager : MonoBehaviour {

        public enum JoinHostState {
            None,
            Join,
            Host
        }

        // References to three main menu screens
        [SerializeField]
        private MainMenuUI _mainMenu;

        [SerializeField]
        private GameObject _joinHostMenu;

        [SerializeField]
        private GameObject _roomNameMenu;

        // Fade to black and back transition
        [SerializeField]
        private TransitionScreenUI  _transitionScreen;

        [SerializeField]
        private TextMeshProUGUI _joinHostButtonText;

        private NetworkManager      _networkManagerInstance;
        private string              _roomName;

        public JoinHostState        joinHostState { get; private set; }

        private void Awake() {
            if (NetworkManager.instance != null) {
                _networkManagerInstance = NetworkManager.instance;
            } else {
                Debug.LogError("Could not find NetworkManager instance");
                return;
            }

            _networkManagerInstance.OnConnectedToPhotonMaster += OnConnectedToMaster;
            _networkManagerInstance.OnConnectedToRoom += OnConnectedToRoom;
        }

        public void ConnectToMaster() {
            if (!_networkManagerInstance.isConnectedToMaster) {
                // Tell photon to connect
                _networkManagerInstance.ConnectToMaster();

                // Start the timeout coroutine
                StartCoroutine(PhotonMasterConnectionTimer());
            } else {
                Debug.Log("Already connected to master");

                _mainMenu.ConnectSuccessful();

                // Transition to next menu
                _transitionScreen.Transition(TransitionScreenUI.MenuStages.MainMenu, TransitionScreenUI.MenuStages.JoinHost);
            }
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

        public void JoinRoom() {
            joinHostState = JoinHostState.Join;
            TransitionToRoomName();
            _joinHostButtonText.SetText("JOIN");
        }

        public void HostRoom() {
            joinHostState = JoinHostState.Host;
            TransitionToRoomName();
            _joinHostButtonText.SetText("HOST");
        }

        public void ConnectToRoom() {
            if (_roomName != null && _roomName != "") {
                _networkManagerInstance.JoinOrCreateRoom(_roomName);
            } else {
                Debug.LogError("Invalid room name was chosen");
            }
        }

        public void LeaveRoom() {
            _networkManagerInstance.LeaveRoom();
            _roomName = null;
        }

        // TODO: Handle the case where the host disconnects but the client remains AFTER client joins the host
        public void OnConnectedToRoom(string roomName) {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1) {
                // Both players are here, lets start the game!
                _networkManagerInstance.RaiseStartGameEvent();
            }
        }

        public void SetRoomName(string roomName) {
            _roomName = roomName;
        }

        public void BackToMainMenu() {
            _transitionScreen.Transition(TransitionScreenUI.MenuStages.JoinHost, TransitionScreenUI.MenuStages.MainMenu);
        }

        public void BackToJoinHost() {
            _transitionScreen.Transition(TransitionScreenUI.MenuStages.RoomName, TransitionScreenUI.MenuStages.JoinHost);
        }

        private void TransitionToRoomName() {
            _transitionScreen.Transition(TransitionScreenUI.MenuStages.JoinHost, TransitionScreenUI.MenuStages.RoomName);
        }

        public void ResetJoinHostData() {
            joinHostState = JoinHostState.None;
            _roomName = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// All things at the game-level related to network live here
// Accessible as a singleton
namespace Photon.Pun {
    public class NetworkManager : MonoBehaviourPunCallbacks {

        public static NetworkManager instance;
        public static PlayerNumber playerNumber = PlayerNumber.None;

        // Public facing events for non-photon views to subscribe to 
        public delegate void ConnectedToPhotonMaster();
        public delegate void ConnectedToRoom(string roomName);

        public event ConnectedToPhotonMaster    OnConnectedToPhotonMaster;
        public event ConnectedToRoom            OnConnectedToRoom;

        // No one should modify these values other than NetworkManager
        public bool isConnectedToMaster     { get; private set; }
        public bool isConnectedToRoom       { get; private set; }

        public string player1RoomPropertyKey;
        public string player2RoomPropertyKey;

        [SerializeField]
        private string  _playerPrefabName;
        private string  _gameVersion = "1.0";

        // The time between sending the RPC to start the timer and when the timer should start on both clients
        // This should be a time in milliseconds
        private int     _roundTimerStartDelay;

        private PhotonView _playerPrefabPhotonView;

        public override void OnEnable() {
            // Register the manager as a callback reciever
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable() {
            // Unregister manager on disable
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Found an existing instance of the NetworkManager, destroying this one");
                DestroyImmediate(this);
            }

            // Set the game version 
            PhotonNetwork.GameVersion = _gameVersion;

            if (!PhotonNetwork.IsConnected) {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.SerializationRate = 20;
                PhotonNetwork.SendRate = 30;
            } else {
                Debug.Log("Network Manager is attempting to initialize while already connected");
            }

            isConnectedToMaster = false;
            isConnectedToRoom = false;
        }

        public override void OnConnectedToMaster() {
            Debug.Log("Connected to Photon master server");
            isConnectedToMaster = true;

            // Set custom some player properties before joining room
            Hashtable playerProperties = new Hashtable();
            playerProperties["Ping"] = PhotonNetwork.GetPing();

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

            // Fire Event 
            if (OnConnectedToPhotonMaster != null)
                OnConnectedToPhotonMaster();
        }

        // Joining a room
        public void JoinOrCreateRoom(string roomName) {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            Debug.LogError("Failed to join room");
            base.OnJoinRoomFailed(returnCode, message);
        }

        public override void OnJoinedRoom() {
            base.OnJoinedRoom();

            isConnectedToRoom = true;

            Debug.Log("Connected to room: " + PhotonNetwork.CurrentRoom.Name);

            // Set player property for player number
            Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;

            // First person to join gets P1
            // TODO: Make this a selectable option?
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1) {
                playerNumber = PlayerNumber.Player2;

                playerProperties[player2RoomPropertyKey] = playerNumber;
            } else {
                playerNumber = PlayerNumber.Player1;

                playerProperties[player1RoomPropertyKey] = playerNumber;
            }

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

            Debug.Log("Joined as Player Number: " + playerNumber);

            MatchManager.instance.SetPlayerSpawn(playerNumber);

            // Fire Event
            if (OnConnectedToRoom != null)
                OnConnectedToRoom(PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer) {
            base.OnPlayerLeftRoom(otherPlayer);

            Debug.Log("Other player has disconnected! Shutting down match");
            
            // Teardown
            MatchManager.instance.EndMatch();
            MatchManager.instance.EndRound();
        }

        public void InstantiatePlayerPrefab() {
            if (PhotonNetwork.CurrentRoom == null) {
                Debug.LogError("Cannot instantiate player prefab, not connected to room. Bailing");
                return;
            }

            _playerPrefabPhotonView = PhotonNetwork.Instantiate(_playerPrefabName, MatchManager.instance.playerSpawn.position, MatchManager.instance.playerSpawn.rotation).GetComponent<PhotonView>();
            _playerPrefabPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            if (_playerPrefabPhotonView == null) {
                Debug.LogError("Player prefab has no photon view! This is an error");
            }
        }
        
        public void DestroyPlayerPrefab() {
            if (PhotonNetwork.CurrentRoom == null) {
                Debug.LogError("Cannot destroy player prefab, not connected to room. Bailing");
                return;
            }

            PhotonNetwork.Destroy(_playerPrefabPhotonView);
        }

        private void Update() {
            // TODO: Profile this in some way and see if its more overhead that just sending an RPC between two clients and checking the RTT
            if (isConnectedToRoom) {
                Hashtable playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;

                // If ping changed, update it
                if (PhotonNetwork.GetPing() != (int)playerCustomProperties["Ping"]) {
                    playerCustomProperties["Ping"] = PhotonNetwork.GetPing();

                    PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
                }
            }
        }
    }
}

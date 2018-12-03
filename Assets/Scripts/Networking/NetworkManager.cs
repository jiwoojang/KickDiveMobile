using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Photon.Pun {
    public class NetworkManager : MonoBehaviourPunCallbacks {

        public static NetworkManager instance;
        public static PlayerNumber playerNumber = PlayerNumber.None;

        // Public facing events for non-photon views to subscribe to 
        public delegate void ConnectedToPhotonMaster();
        public delegate void ConnectedToRoom(string roomName);

        public event ConnectedToPhotonMaster    OnConnectedToPhotonMaster;
        public event ConnectedToRoom            OnConnectedToRoom;

        public bool isConnectedToMaster;
        public bool isConnectedToRoom;

        public string player1RoomPropertyKey;
        public string player2RoomPropertyKey;

        [SerializeField]
        private string _playerPrefabName;
        private string _gameVersion = "1.0";

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
        }

        public override void OnConnectedToMaster() {
            Debug.Log("Connected to Photon master server");
            isConnectedToMaster = true;

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

            // Set custom room properties
            Hashtable setValue = new Hashtable();
            Hashtable expectedValue = new Hashtable();

            // First person to join gets P1
            // TODO: Make this a selectable option?
            if (PhotonNetwork.CurrentRoom.PlayerCount > 1) {
                playerNumber = PlayerNumber.Player2;

                setValue.Add(player2RoomPropertyKey, PhotonNetwork.LocalPlayer.ActorNumber);

                expectedValue.Add(player2RoomPropertyKey, PhotonNetwork.LocalPlayer.ActorNumber);
            } else {
                playerNumber = PlayerNumber.Player1;

                setValue.Add(player1RoomPropertyKey, PhotonNetwork.LocalPlayer.ActorNumber);

                expectedValue.Add(player1RoomPropertyKey, PhotonNetwork.LocalPlayer.ActorNumber);
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(setValue, expectedValue);

            Debug.Log("Joined as Player Number: " + playerNumber);

            MatchManager.instance.SetPlayerSpawn(playerNumber);

            // Fire Event
            if (OnConnectedToRoom != null)
                OnConnectedToRoom(PhotonNetwork.CurrentRoom.Name);
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
    }
}

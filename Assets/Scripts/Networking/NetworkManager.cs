using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Photon.Pun {
    public class NetworkManager : MonoBehaviourPunCallbacks {

        public enum PlayerNumber {
            None,
            // Player 1 starts on the right side of the screen
            Player1, 
            // PLayer 2 starts on the left side of the screen
            Player2,
        }

        public bool isConnectedToMaster;
        public bool isInRoom;

        [SerializeField]
        private string player1RoomPropertyKey, player2RoomPropertyKey;

        public static NetworkManager instance;
        public static PlayerNumber playerNumber = PlayerNumber.None;

        [SerializeField]
        private string _playerPrefabName;
        private string _gameVersion = "1.0";

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
            } else {
                Debug.Log("Network Manager is attempting to initialize while already connected");
            }
        }

        public override void OnConnectedToMaster() {
            Debug.Log("Connected to Photon master server");
            isConnectedToMaster = true;
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

            isInRoom = true;

            Debug.Log("Connected to room: " + PhotonNetwork.CurrentRoom.Name);
            Hashtable setValue = new Hashtable();
            Hashtable expectedValue = new Hashtable();

            // First person to join gets P1
            // TODO: Make this a selectable option?
            if (PhotonNetwork.CurrentRoom.PlayerCount != 0) {
                playerNumber = PlayerNumber.Player2;

                setValue.Add(player2RoomPropertyKey, PhotonNetwork.LocalPlayer.UserId);

                expectedValue.Add(player2RoomPropertyKey, PhotonNetwork.LocalPlayer.UserId);
            } else {
                playerNumber = PlayerNumber.Player1;

                setValue.Add(player1RoomPropertyKey, PhotonNetwork.LocalPlayer.UserId);

                expectedValue.Add(player1RoomPropertyKey, PhotonNetwork.LocalPlayer.UserId);
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(setValue, expectedValue);
            MatchManager matchManager = MatchManager.instance;

            if (matchManager != null) {
                matchManager.SetPlayerSpawn(playerNumber);
            }
        }

        public void InstantiatePlayerPrefab() {
            if (PhotonNetwork.CurrentRoom == null) {
                Debug.LogError("Cannot instantiate player prefab, not connected to room. Bailing");
                return;
            }

            PhotonNetwork.Instantiate(_playerPrefabName, MatchManager.instance.playerSpawn.position, MatchManager.instance.playerSpawn.rotation);
        }
    }
}

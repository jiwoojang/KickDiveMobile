using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KickDive.Match;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// All things at the game-level related to network live here
// Accessible as a singleton
namespace Photon.Pun {
    public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback {

        private enum NetworkManagerState {
            None, 
            InMenu, 
            InGame
        }

        public static NetworkManager instance;
        public static PlayerNumber playerNumber = PlayerNumber.None;

        // Player number 1 is the first person to join the room so they are the host
        public static bool IsHost { get { return playerNumber == PlayerNumber.Player1; } }

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
        private readonly byte startGameEventCode = 0;
        private NetworkManagerState _lifeCycleState;

        private PhotonView      _playerPrefabPhotonView;
        private MatchManager    _matchManagerInstace;

        public override void OnEnable() {
            // Register the manager as a callback reciever
            PhotonNetwork.AddCallbackTarget(this);

            // For intialization when we load the game scene
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDisable() {
            // Unregister manager on disable
            PhotonNetwork.RemoveCallbackTarget(this);

            // Unregister scene initialization
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Awake() {
            // This manager will be needed at both the main menu and inside the game
            DontDestroyOnLoad(gameObject);

            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Found an existing instance of the NetworkManager, destroying this one");
                DestroyImmediate(this);
            }

            _lifeCycleState = NetworkManagerState.InMenu;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            // Only for when we are loading into the game
            if (_lifeCycleState == NetworkManagerState.InGame) {
                InitializeGameNetworkManager();
            }
        }

        private void InitializeGameNetworkManager() {
            if (MatchManager.instance != null) {
                _matchManagerInstace = MatchManager.instance;

                // Do this BEFORE player prefab instantiation
                _matchManagerInstace.SetPlayerSpawn(playerNumber);
            } else {
                Debug.LogError("NetworkManager cannot find MatchManager");
            }

            // Start the player!
            if (isConnectedToMaster && isConnectedToRoom) {
                InstantiatePlayerPrefab();
                _matchManagerInstace.RemoteInitializeRound();
            }
        }

        public void ConnectToMaster() {
            if (!PhotonNetwork.IsConnected) {
                // Set the game version 
                PhotonNetwork.GameVersion = _gameVersion;

                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.SerializationRate = 20;
                PhotonNetwork.SendRate = 30;

                isConnectedToMaster = false;
                isConnectedToRoom = false;
            } else {
                Debug.Log("Network Manager is attempting to initialize while already connected");
            }
        }

        public override void OnConnectedToMaster() {
            // Set custom some player properties before joining room
            Hashtable playerProperties = new Hashtable();
            playerProperties["Ping"] = PhotonNetwork.GetPing();

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

            // Fire Event 
            if (OnConnectedToPhotonMaster != null && !isConnectedToMaster) {
                OnConnectedToPhotonMaster();

                Debug.Log("Connected to Photon master server");
                isConnectedToMaster = true;
            }
        }

        // Joining a room
        public void JoinOrCreateRoom(string roomName) {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
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

            // Fire Event
            if (OnConnectedToRoom != null)
                OnConnectedToRoom(PhotonNetwork.CurrentRoom.Name);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer) {
            base.OnPlayerLeftRoom(otherPlayer);

            Debug.Log("Other player has disconnected! Shutting down match");
            
            // Teardown
            if (_matchManagerInstace != null) {
                _matchManagerInstace.EndMatch();
                _matchManagerInstace.EndRound();
            }
        }

        public void RaiseStartGameEvent() {
            RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(startGameEventCode, null, eventOptions, sendOptions);
        }

        // Event handler for PHOTON events only!
        public void OnEvent(EventData photonEvent) {
            // Load the game scene
            if (photonEvent.Code == 0) {
                Debug.Log("Photon start game event recieved!");
                _lifeCycleState = NetworkManagerState.InGame;
                SceneManager.LoadScene(1, LoadSceneMode.Single);
            }
        }

        public void InstantiatePlayerPrefab() {
            if (PhotonNetwork.CurrentRoom == null) {
                Debug.LogError("Cannot instantiate player prefab, not connected to room. Bailing");
                return;
            }

            if (_playerPrefabPhotonView == null && _matchManagerInstace != null) {
                _playerPrefabPhotonView = PhotonNetwork.Instantiate(_playerPrefabName, _matchManagerInstace.playerSpawn.position, _matchManagerInstace.playerSpawn.rotation).GetComponent<PhotonView>();
                _playerPrefabPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);

                _matchManagerInstace.SetPlayerGameObject(_playerPrefabPhotonView.gameObject);

                if (_playerPrefabPhotonView == null) {
                    Debug.LogError("Player prefab has no photon view! This is an error");
                }
            }
        }
        
        public void DestroyPlayerPrefab() {
            if (PhotonNetwork.CurrentRoom == null) {
                Debug.LogError("Cannot destroy player prefab, not connected to room. Bailing");
                return;
            }
            if (_playerPrefabPhotonView != null) {
                PhotonNetwork.Destroy(_playerPrefabPhotonView);
            }
        }

        private void Update() {
            // TODO: Profile this in some way and see if its more overhead that just sending an RPC between two clients and checking the RTT
            if (isConnectedToRoom) {
                Hashtable playerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;

                // If ping changed, update it
                object otherPlayerPing = playerCustomProperties["Ping"];

                if (otherPlayerPing != null) {
                    if (PhotonNetwork.GetPing() != (int)otherPlayerPing) {
                        playerCustomProperties["Ping"] = PhotonNetwork.GetPing();

                        PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
                    }
                }
            }
        }
    }
}

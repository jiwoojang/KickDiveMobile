using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

namespace Photon.Pun {
    public class NetworkManager : MonoBehaviourPunCallbacks {

        public static NetworkManager instance;

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
        }
    }
}

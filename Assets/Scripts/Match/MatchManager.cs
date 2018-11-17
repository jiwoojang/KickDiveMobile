using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// A class that handles all match specific work
// This may include per-round management
namespace KickDive.Match {
    public class MatchManager : MonoBehaviour {

        public static MatchManager instance;

        // Player 1 spawn
        [SerializeField]
        private Transform _playerRightSpawn;

        // Player 2 spawn
        [SerializeField]
        private Transform _playerLeftSpawn;

        public Transform    playerSpawn;
        public Vector2      DiveDirection;
        public Vector2      KickDirection;

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Found an existing instance of the NetworkManager, destroying this one");
                DestroyImmediate(this);
            }

            if (_playerRightSpawn == null) {
                Debug.LogError("Player 1 spawn has not been set");
            }

            if (_playerLeftSpawn == null) {
                    Debug.LogError("Player 2 spawn has not been set");
            }

            DiveDirection = Vector2.up;
        }

        // For setting the player number, and spawn
        public void SetPlayerSpawn(NetworkManager.PlayerNumber playerNumber) {
            if (playerNumber == NetworkManager.PlayerNumber.None) {
                Debug.LogError("No provided player number, bailing");
                return;
            }

            switch (playerNumber) {
                case NetworkManager.PlayerNumber.Player1: {
                        playerSpawn = _playerRightSpawn;
                        KickDirection = new Vector2(-1.0f, -1.0f);
                        break;
                    }
                case NetworkManager.PlayerNumber.Player2: {
                        playerSpawn = _playerLeftSpawn;
                        KickDirection = new Vector2(1.0f, -1.0f);
                        break;
                    }
            }
        }
    }
}

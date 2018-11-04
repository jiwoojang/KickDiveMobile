using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.Utility {
    public class GameUtility : MonoBehaviour {

        // Player 1 spawn
        [SerializeField]
        private Transform _playerRightSpawn;
        private static Transform playerRightSpawn;

        // Player 2 spawn
        [SerializeField]
        private Transform _playerLeftSpawn;
        private static Transform playerLeftSpawn;

        public static Transform playerSpawn;

        private void Awake() {
            playerRightSpawn.position = _playerRightSpawn.position;
            playerRightSpawn.rotation = _playerRightSpawn.rotation;

            playerLeftSpawn.position = _playerLeftSpawn.position;
            playerLeftSpawn.rotation = _playerLeftSpawn.rotation;
        }

        public static void SetPlayerSpawn(Photon.Pun.NetworkManager.PlayerNumber playerNumber) {
            if (playerNumber == Photon.Pun.NetworkManager.PlayerNumber.None) {
                Debug.LogError("No provided player number, bailing");
                return;
            }

            switch (playerNumber) {
                case Photon.Pun.NetworkManager.PlayerNumber.Player1:
                    playerSpawn = playerRightSpawn;
                    break;
                case Photon.Pun.NetworkManager.PlayerNumber.Player2:
                    playerSpawn = playerLeftSpawn;
                    break;
            }
        }
    }
}

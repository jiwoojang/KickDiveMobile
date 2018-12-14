using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

// A nice class that wraps all the UI related to a single player
// Everything in the top two corners of the screen
namespace KickDive.UI {
    public class PlayerUI : MonoBehaviour {

        private enum PlayerHealthState {
            Alive,
            Dead
        }

        [SerializeField]
        private Slider              _healthbarSlider;

        [SerializeField]
        private float               _healthbarSpeed = 1.0f;

        [SerializeField]
        private TextMeshProUGUI     _playerFighterNameTextMesh;

        [SerializeField]
        private TextMeshProUGUI     _winsTextMesh;

        public string               playerFighterName { get; private set; }
        private PlayerNumber        _playerNumber;
        private PlayerHealthState   _playerHealth = PlayerHealthState.Alive;

        // Sets all important properties for this UI bundle
        public void SetPlayerInfo(string fighterName, PlayerNumber playerNumber) {
            playerFighterName = fighterName;
            _playerNumber = playerNumber;

            if (_playerNumber != PlayerNumber.None) {
                switch (_playerNumber) {
                    // Right Side
                    case PlayerNumber.Player1: {
                            _playerFighterNameTextMesh.alignment = TextAlignmentOptions.MidlineRight;
                            _winsTextMesh.alignment = TextAlignmentOptions.MidlineRight;
                            break;
                        }
                    // Left Side
                    case PlayerNumber.Player2: {
                            _playerFighterNameTextMesh.alignment = TextAlignmentOptions.MidlineLeft;
                            _winsTextMesh.alignment = TextAlignmentOptions.MidlineLeft;
                            break;
                        }
                }
            } else {
                Debug.LogError("Initializing player UI without a player number!");
            }

            _playerFighterNameTextMesh.SetText(fighterName);
        }

        // Always between 1 or 0 health 
        public void DepleteHealth() {
            _playerHealth = PlayerHealthState.Dead;
        }

        public void ResetHealth() {
            _healthbarSlider.value = 1.0f;
            _playerHealth = PlayerHealthState.Alive;
        }

        public void SetPlayerWins(int winNumber) {
            _winsTextMesh.SetText("0" + winNumber + "   Wins");
        }

        void Update() {
            // Smooth out the depleting of the healthbar
            if (_playerHealth == PlayerHealthState.Dead) {
                _healthbarSlider.value = Mathf.MoveTowards(_healthbarSlider.value, _healthbarSlider.minValue, _healthbarSpeed * Time.deltaTime);

                if (_healthbarSlider.value < _healthbarSlider.maxValue * 0.05) {
                    // Reset the player health when the animation is done
                    ResetHealth();
                }
            }
        }
    }
}

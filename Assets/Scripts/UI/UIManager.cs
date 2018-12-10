using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using TMPro;

// Manages logic for in game UI
// Accessible as a singleton
namespace KickDive.UI {
    public class UIManager : MonoBehaviour {

        public static UIManager instance;

        [SerializeField]
        private PlayerUI _player1UI;

        [SerializeField]
        private PlayerUI _player2UI;

        [SerializeField]
        private RoundUI _roundUI;

        // How long the "Round X" text is displayed for in seconds
        [SerializeField]
        private float _roundUIDisplayTime = 2.0f;

        [SerializeField]
        private TextMeshProUGUI _roundTimer;

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Found an existing instance of the UIManager, destroying this one");
                DestroyImmediate(this);
            }

            if (MatchManager.instance != null) {
                MatchManager.instance.OnPlayerWonRound += PlayerWonRound;
            } else {
                Debug.LogError("Could not find MatchManager to get match information for the UI!");
            }
        }

        void Start() {
            // DEBUG VALUES FOR NOW!
            _player1UI.SetPlayerInfo("SubZero", PlayerNumber.Player1);
            _player2UI.SetPlayerInfo("Scorpion", PlayerNumber.Player2);
        }

        public void PlayerWonRound(PlayerNumber playerNumber) {
            switch (playerNumber) {
                case PlayerNumber.Player1: {
                    _player1UI.SetPlayerWins(MatchManager.instance.player1RoundScore);
                    _player2UI.DepleteHealth();
                    break;
                }
                case PlayerNumber.Player2: {
                    _player2UI.SetPlayerWins(MatchManager.instance.player2RoundScore);
                    _player1UI.DepleteHealth();
                    break;
                }
            }
        }

        public void StartRoundUI() {
            StartCoroutine(NewRoundTextCoroutine());
        }

        private IEnumerator NewRoundTextCoroutine() {
            _roundUI.DisplayNewRound();
            yield return new WaitForSeconds(_roundUIDisplayTime);
            _roundUI.StartRoundCounterFade();
        }

        private void Update() {
            _roundTimer.SetText(MatchManager.instance.currentIntegerRoundTimeRemaning.ToString());
        }
    }
}

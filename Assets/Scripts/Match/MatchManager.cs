using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using KickDive.UI;
using KickDive.Hardware;

// A class that handles all match specific work
// This may include per-round management
namespace KickDive.Match {
    public enum PlayerNumber {
        None,
        // Player 1 starts on the right side of the screen
        Player1,
        // Player 2 starts on the left side of the screen
        Player2,
    }

    // Life cycle enum for match and round
    public enum MatchStatus {
        None,
        WaitingToStartMatch,
        WaitingToStartRound,
        RoundInProgress,
        MatchComplete,
    }

    public class MatchManager : MonoBehaviourPun {

        public static MatchManager instance;

        // Events for match and round results
        public delegate void PlayerWonRound(PlayerNumber roundWinningPlayer);
        public delegate void PlayerWonMatch(PlayerNumber matchWinningPlayer);

        public event PlayerWonRound OnPlayerWonRound;
        public event PlayerWonMatch OnPlayerWonMatch;

        // Player 1 spawn
        [SerializeField]
        private Transform _playerRightSpawn;

        // Player 2 spawn
        [SerializeField]
        private Transform _playerLeftSpawn;

        // Length of a round in seconds
        [SerializeField]
        private int roundLength = 30;

        [HideInInspector]
        public Transform    playerSpawn;
        public Vector2      DiveDirection;
        public Vector2      KickDirection;

        // Private setters for these match critical values
        public MatchStatus  matchStatus { get; private set; }

        public int          player1RoundScore { get; private set; }
        public int          player2RoundScore { get; private set; }
        public int          scoreToWin = 3;
        public int          roundNumber { get; private set; }

        // Time left in this round in whole seconds
        // Truncates the floating point remaning time
        public int          currentIntegerRoundTimeRemaning { get { return (int)_currentFloatingPointRoundTimeRemanining; } }

        private float       _roundTimerStartDelay = 0;
        // Time left in this round in floating point precision
        private float       _currentFloatingPointRoundTimeRemanining = 30.0f;

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Found an existing instance of the MatchManager, destroying this one");
                DestroyImmediate(this);
            }

            if (_playerRightSpawn == null) {
                Debug.LogError("Player 1 spawn has not been set");
            }

            if (_playerLeftSpawn == null) {
                    Debug.LogError("Player 2 spawn has not been set");
            }

            DiveDirection = Vector2.up;

            matchStatus = MatchStatus.WaitingToStartMatch;
            InputManager.instance.LockInput();
            roundNumber = 1;
        }

        // For setting the player number, and spawn
        public void SetPlayerSpawn(PlayerNumber playerNumber) {
            if (playerNumber == PlayerNumber.None) {
                Debug.LogError("No provided player number, bailing");
                return;
            }

            switch (playerNumber) {
                case PlayerNumber.Player1: {
                        playerSpawn = _playerRightSpawn;
                        KickDirection = new Vector2(-1.0f, -1.0f);
                        break;
                    }
                case PlayerNumber.Player2: {
                        playerSpawn = _playerLeftSpawn;
                        KickDirection = new Vector2(1.0f, -1.0f);
                        break;
                    }
            }
        }

        // Matches 
        public void EndMatch() {
            matchStatus = MatchStatus.MatchComplete;
        }

        // Rounds
        public void SetPlayerWonRound(PlayerNumber playerNumber) {
            PlayerNumber roundWinningPlayer = PlayerNumber.None;
            switch (playerNumber) {
                case PlayerNumber.Player1: {
                        player1RoundScore++;
                        roundWinningPlayer = PlayerNumber.Player1;
                    break;
                }
                case PlayerNumber.Player2: {
                        player2RoundScore++;
                        roundWinningPlayer = PlayerNumber.Player2;
                    break;
                }
            }

            if (OnPlayerWonRound != null) {
                OnPlayerWonRound(roundWinningPlayer);
            }

            EndRound();
        }

        public void RemoteInitializeRound() {
            if (matchStatus != MatchStatus.MatchComplete) {
                photonView.RPC("InitializeRound", RpcTarget.All);
            }
        }

        [PunRPC]
        public void InitializeRound() {
            matchStatus = MatchStatus.WaitingToStartRound;

            // Player 1 will always request the timer to synchronize
            // TODO: Make this more flexible
            if (NetworkManager.playerNumber == PlayerNumber.Player1) {
                StartRoundTimerSynchronization();
            }

            // Restart player
            NetworkManager.instance.InstantiatePlayerPrefab();
        }

        public void StartRound() {
            // Unlock input
            InputManager.instance.UnlockInput();

            matchStatus = MatchStatus.RoundInProgress;
        }

        public void EndRound() {
            // Lock input 
            InputManager.instance.LockInput();

            // Tear down player
            NetworkManager.instance.DestroyPlayerPrefab();

            // Reset timers
            _currentFloatingPointRoundTimeRemanining = 30.0f;
            matchStatus = MatchStatus.WaitingToStartRound;

            // Check if anyone has won the match 
            if ((player1RoundScore == scoreToWin) || (player2RoundScore == scoreToWin)) {
                EndMatch();

                PlayerNumber matchWinningPlayer = player1RoundScore > player2RoundScore ? PlayerNumber.Player1 : PlayerNumber.Player2;

                Debug.Log(matchWinningPlayer + " won the match!");

                if (OnPlayerWonMatch != null) {
                    OnPlayerWonMatch(matchWinningPlayer);
                }
            } else {
                // Start a new round if no one won the match
                RemoteInitializeRound();
                roundNumber++;
            }
        }

        // Round timers
        public void StartRoundTimerSynchronization() {
            if (matchStatus == MatchStatus.WaitingToStartRound) {
                // There should only be one other player in this list
                // Ping in this situation is RTT
                // Note this value could overflow to negative
                int otherPlayerPing = (int)PhotonNetwork.PlayerListOthers[0].CustomProperties["Ping"];
                int thisPlayerPing = PhotonNetwork.GetPing();

                // Whichever player has the higher ping, use their ping as the delay to start the timer
                int delayPing = (otherPlayerPing > thisPlayerPing) ? otherPlayerPing : thisPlayerPing;

                // Note there may be discrepancy if the ping of the other client changes significantly within the time to recieve this RPC
                // Buffer this by double so that the client with the highest ping will not have to start the timer right away
                photonView.RPC("HandleRoundTimerStartSynchronization", RpcTarget.AllViaServer, PhotonNetwork.ServerTimestamp, 2 * delayPing);
            }
        }

        [PunRPC]
        public void HandleRoundTimerStartSynchronization(int serverTimeStamp, int timerStartDelay) {
            // Find out how long ago this RPC was called from the other client
            // This should be about the same as RTT
            int RPCCallTime = PhotonNetwork.ServerTimestamp - serverTimeStamp;

            SetRoundTimerStartDelay(timerStartDelay - RPCCallTime);
        }

        public void SetRoundTimerStartDelay(int delay) {
            _roundTimerStartDelay = delay;
        }

        private void Update() {
            switch (matchStatus) {
                case MatchStatus.WaitingToStartRound: {
                    // Time sync
                    if (_roundTimerStartDelay != 0) {
                        _roundTimerStartDelay -= (Time.deltaTime * 1000);

                        if (_roundTimerStartDelay < 0) {
                            Debug.Log("Round Timer Synchronized!");
                            _roundTimerStartDelay = 0;
                            UIManager.instance.StartRoundUI();
                        }
                    }
                    break;
                }
                case MatchStatus.RoundInProgress: {
                    _currentFloatingPointRoundTimeRemanining -= Time.deltaTime;

                    // End the round if the time runs out
                    if (_currentFloatingPointRoundTimeRemanining < 0) {
                        EndRound();
                    }
                    break;
                }
            }
        }
    }
}

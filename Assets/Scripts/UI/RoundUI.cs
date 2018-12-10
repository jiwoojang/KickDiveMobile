using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KickDive.Match;

// A class dedicated to the UI at the beginning of rounds
namespace KickDive.UI {
    public class RoundUI : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI     _roundCounterText;
        [SerializeField]
        private Animator            _animator;
        [SerializeField]
        private float               _fadeSpeed = 10.0f;

        private Color               _roundCounterTextColor;
        private bool                _fadeText;
        private int                 _fightPopupStartHash;

        private void Awake() {
            _fightPopupStartHash = Animator.StringToHash("Fight");
            _roundCounterTextColor = _roundCounterText.color;
        }
        
        public void DisplayNewRound() {
            _roundCounterText.color = _roundCounterTextColor;
            _roundCounterText.SetText("Round " + MatchManager.instance.roundNumber);
            _roundCounterText.gameObject.SetActive(true);
        }

        public void StartRoundCounterFade() {
            _fadeText = true;
        }

        private void StopRoundCounterFade() {
            _roundCounterText.gameObject.SetActive(false);
            _fadeText = false;
        }

        private void StartFightPopup() {
            _animator.SetTrigger(_fightPopupStartHash);
        }
        
        // Public so it can be triggered by animation event
        public void EndFightPopup() {
            MatchManager.instance.StartRound();
            _animator.ResetTrigger(_fightPopupStartHash);
        }

        void Update() {
            if (_fadeText) {

                float newAlpha = _roundCounterText.color.a - _fadeSpeed * Time.deltaTime;
                _roundCounterText.color = new Color(_roundCounterTextColor.r, _roundCounterTextColor.g, _roundCounterTextColor.b, newAlpha);

                if (newAlpha < 0) {
                    StopRoundCounterFade();
                    StartFightPopup();
                }
            }
        }
    }
}

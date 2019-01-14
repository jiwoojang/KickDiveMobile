using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KickDive.UI {
    public class TransitionScreenUI : MonoBehaviour {

        public enum MenuStages {
            None,
            MainMenu,
            JoinHost,
            RoomName
        }

        private enum TransitionState {
            Idle,
            FadeOut,
            FadeIn
        }

        [SerializeField]
        private Image _image;

        [SerializeField]
        private float _fadeSpeed;

        [SerializeField]
        private GameObject _mainMenu, _joinHost, _roomName;

        private TransitionState _state;
        private MenuStages      _currentStage, _nextStage;

        private void Awake() {
            if (_mainMenu == null || _joinHost == null || _roomName == null) {
                Debug.LogError("Transition screen is missing a reference to one of the menu screens");
            }

            _nextStage = MenuStages.None;
        }

        public void Transition(MenuStages currentStage, MenuStages nextStage) {
            _currentStage = currentStage;
            _nextStage = nextStage;
            
            if (_state == TransitionState.Idle) {
                // Only allow transitions to start from idle
                _state = TransitionState.FadeIn;
            } else {
                Debug.LogError("Attempting to transition stages while a transition is already happening!");
            }
        }

        private GameObject GetStageGameObject(MenuStages stage) {
            switch (stage) {
                case MenuStages.MainMenu: {
                    return _mainMenu;
                }
                case MenuStages.JoinHost: {
                    return _joinHost;
                }
                case MenuStages.RoomName: {
                    return _roomName;
                }
            }

            // Return some empty value
            Debug.LogError("Could not return a GameObject for provided MenuStage");
            return null;
        }

        // Transition FROM black
        private void FadeOutComplete() {
            _state = TransitionState.Idle;
        }

        // Transition TO Black
        private void FadeInComplete() {
            GetStageGameObject(_currentStage).SetActive(false);
            GetStageGameObject(_nextStage).SetActive(true);

            // Update stage states
            _currentStage = _nextStage;
            _nextStage = MenuStages.None;

            _state = TransitionState.FadeOut;
        }

        void Update() {
            switch(_state) {
                case TransitionState.FadeOut: {
                    _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 0), _fadeSpeed * Time.deltaTime);

                    if (_image.color.a < 0.1f) {
                        // Handler for complete transition
                        FadeOutComplete();
                    }

                    break;
                }
                case TransitionState.FadeIn: {
                    _image.color = Color.Lerp(_image.color, new Color(0, 0, 0, 1), _fadeSpeed * Time.deltaTime);

                    if (_image.color.a > 0.9f) {
                        // Handler for complete transition
                        FadeInComplete();
                    }

                    break;
                }
            }
        }
    }
}

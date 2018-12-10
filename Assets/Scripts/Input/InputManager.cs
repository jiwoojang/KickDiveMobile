using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.Hardware{
    public class InputManager : MonoBehaviour {

        private enum InputLockStatus {
            None, 
            Locked, 
            Unlocked
        }

        enum GameplayInputType {
            PCDebug,
            XboxController
        }

        public static InputManager  instance;
        public HardwareInput        gameInput { get; private set; }

        [SerializeField]
        private GameplayInputType   _inputType;
        private InputLockStatus     _inputLockStatus = InputLockStatus.None;

        private void Awake() {

            if (instance == null) {
                instance = this;
            } else if (instance != null) {
                Debug.Log("Found an existing instance of the InputManager, destroying this one");
                DestroyImmediate(this);
            }

            switch (_inputType) {
                case GameplayInputType.PCDebug: {
                        gameInput = new PCDebugInput();
                        break;
                    }
                case GameplayInputType.XboxController: {
                        gameInput = new XboxControllerInput();
                        break;
                    }
            }
        }

        public void LockInput() {
            _inputLockStatus = InputLockStatus.Locked;
        }

        public void UnlockInput() {
            _inputLockStatus = InputLockStatus.Unlocked;
        }

        private void Update() {
            if (_inputLockStatus == InputLockStatus.Unlocked) {
                gameInput.GetPrimaryButtonStatus();
                gameInput.GetSecondaryButtonStatus();
            }
        }
    }

}
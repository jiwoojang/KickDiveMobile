using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.Hardware{
    public class InputManager : MonoBehaviour {

        enum GameplayInputType {
            PCDebug,
            XboxController
        }

        public static InputManager instance;
        public HardwareInput gameInput { get; private set; }

        [SerializeField]
        private GameplayInputType _inputType;

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

        private void Update() {
            gameInput.GetPrimaryButtonStatus();
            gameInput.GetSecondaryButtonStatus();
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Hardware;
using Photon.Pun;

public class DebugGameManager : MonoBehaviour {

    private InputManager _inputManager;

    private void Awake() {
        if (InputManager.instance != null) {
            _inputManager = InputManager.instance;
        }
    }

    private void OnEnable() {
        if (_inputManager != null) {
            // Subscribe to input events
            _inputManager.gameInput.OnPrimaryButtonStarted += PrimaryButtonStartedHandler;
            _inputManager.gameInput.OnPrimaryButtonEnded += PrimaryButtonEndedHandler;
            _inputManager.gameInput.OnSecondaryButtonStarted += SecondaryButtonStartedHandler;
            _inputManager.gameInput.OnSecondaryButtonEnded += SecondaryButtonEndedHandler;
        } else {
            Debug.LogError("Attempting to subscribe to input manager when it does not exist");
        }
    }

    private void OnDisable() {
        if (_inputManager != null) {
            // Unsubscribe from input events
            _inputManager.gameInput.OnPrimaryButtonStarted -= PrimaryButtonStartedHandler;
            _inputManager.gameInput.OnPrimaryButtonEnded -= PrimaryButtonEndedHandler;
            _inputManager.gameInput.OnSecondaryButtonStarted -= SecondaryButtonStartedHandler;
            _inputManager.gameInput.OnSecondaryButtonEnded -= SecondaryButtonEndedHandler;
        }
    }

    private void PrimaryButtonStartedHandler(HardwareInput sender) {
        //Debug.Log("Primary Button Started");
    }

    private void PrimaryButtonEndedHandler(HardwareInput sender) {
        //Debug.Log("Primary Button Ended");
    }

    private void SecondaryButtonStartedHandler(HardwareInput sender) {
        //Debug.Log("Secondary Button Started");
    }

    private void SecondaryButtonEndedHandler(HardwareInput sender) {
        //Debug.Log("Secondary Button Ended");
    }
}

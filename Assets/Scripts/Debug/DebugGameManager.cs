using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Hardware;
using Photon.Pun;

// A class used for debugging game properties/application level functionality 
// Not meant to be included in the final game
public class DebugGameManager : MonoBehaviour {

    private InputManager    _inputManager;
    private NetworkManager  _networkManager;

    public void Start() {
        if (InputManager.instance != null) {
            _inputManager = InputManager.instance;
        }

        if (NetworkManager.instance != null) {
            _networkManager = NetworkManager.instance;
        }

        if (_inputManager != null) {
            // Subscribe to input events
            _inputManager.gameInput.OnPrimaryButtonStarted += PrimaryButtonStartedHandler;
            _inputManager.gameInput.OnPrimaryButtonEnded += PrimaryButtonEndedHandler;
            _inputManager.gameInput.OnSecondaryButtonStarted += SecondaryButtonStartedHandler;
            _inputManager.gameInput.OnSecondaryButtonEnded += SecondaryButtonEndedHandler;
        } else {
            Debug.LogError("Attempting to subscribe to input manager when it does not exist");
        }

        if (_networkManager != null) {
            _networkManager.OnConnectedToPhotonMaster += OnConnectedToMaster;
            _networkManager.OnConnectedToRoom += OnConnectedToRoom;
        }
    }

    public void OnDisable() {
        if (_inputManager != null) {
            // Unsubscribe from input events
            _inputManager.gameInput.OnPrimaryButtonStarted -= PrimaryButtonStartedHandler;
            _inputManager.gameInput.OnPrimaryButtonEnded -= PrimaryButtonEndedHandler;
            _inputManager.gameInput.OnSecondaryButtonStarted -= SecondaryButtonStartedHandler;
            _inputManager.gameInput.OnSecondaryButtonEnded -= SecondaryButtonEndedHandler;
        }

        if (_networkManager != null) {
            _networkManager.OnConnectedToPhotonMaster -= OnConnectedToMaster;
            _networkManager.OnConnectedToRoom -= OnConnectedToRoom;
        }
    }

    private void OnConnectedToMaster() {
        _networkManager.JoinOrCreateRoom("DebugRoom");
    }

    private void OnConnectedToRoom(string roomName) {
        _networkManager.InstantiatePlayerPrefab();
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

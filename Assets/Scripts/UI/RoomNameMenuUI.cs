using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KickDive.UI {
    public class RoomNameMenuUI : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private LoadingCircle _loadingCircle;

        [SerializeField]
        private Transform _mainContents;

        [SerializeField]
        private Transform _waitingScreen;

        [SerializeField]
        private MenuUIManager _uiManager;

        [SerializeField]
        private TMP_InputField _inputField;

        public bool isWaiting { get; private set; }

        public void LoadWaitingContent() {
            // Turn on the right context
            _inputField.DeactivateInputField();
            _mainContents.gameObject.SetActive(false);
            _waitingScreen.gameObject.SetActive(true);

            _titleText.SetText("WAITING");

            _loadingCircle.StartLoading();

            isWaiting = true;
        }

        public void LoadMainContent() {
            // Turn on the right context
            _inputField.ActivateInputField();
            _mainContents.gameObject.SetActive(true);

            _loadingCircle.StopLoading();
            _waitingScreen.gameObject.SetActive(false);

            _titleText.SetText("ENTER NAME");

            isWaiting = false;
        }
    }
}

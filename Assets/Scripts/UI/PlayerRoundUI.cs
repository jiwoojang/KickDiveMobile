using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Match;
using Photon.Pun;

namespace KickDive.UI {
    public class HealthBar : MonoBehaviour {

        // This sprite is only the green bar
        [SerializeField]
        private SpriteRenderer  _healthbarFullSprite;

        // This sprite also contains the background outline for the healthbar
        [SerializeField]
        private SpriteRenderer  _healthbarEmptySprite;

        private string          _playerFighterName;
        private PlayerNumber    _playerNumber;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}

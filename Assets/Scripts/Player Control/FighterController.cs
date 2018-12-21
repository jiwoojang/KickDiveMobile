using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Hardware;
using KickDive.Match;
using Photon.Pun;

namespace KickDive.Fighter {
    [RequireComponent(typeof(Rigidbody2D))]
    public class FighterController : MonoBehaviour {

        public float    jumpForce;
        public float    kickForce;
        public bool     isGrounded { get; private set; }
        public bool     isKicking { get; private set; }
        public Vector2  kickDirectionVector = new Vector2(1.0f, -1.0f);

        [SerializeField]
        private Animator                    _animator;
        [SerializeField]
        private FighterColliderController   _colliderController;
        [SerializeField]
        private SpriteRenderer              _spriteRenderer;
        [SerializeField]
        private BoxCollider2D               _boxCollider;

        private Rigidbody2D     _rigidbody2D;
        private Vector2         _normalizedDiveDirectionVector;
        private Vector2         _normalizedKickDirectionVector;
        private int             _diveAnimatorHash;
        private int             _kickAnimatorHash;
        private int             _idleAnimatorHash;
        private PhotonView      _photonView;
        private bool            _isPlayerModelFlipped;

        private void Awake() {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            
            // Grab player movement directions based on 
            _normalizedKickDirectionVector = MatchManager.instance.KickDirection.normalized;
            _normalizedDiveDirectionVector = MatchManager.instance.DiveDirection.normalized;

            _photonView = PhotonView.Get(this);

            // This is gross, but custom properties are not ready by the time this check is made
            // TODO: Figure out a better way to get this info on time
            if (_photonView.Owner.ActorNumber == 1) {
                _animator.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                _colliderController.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }

            // Register to the NetworkManager if this is the remote player's controller 
            if (!_photonView.IsMine) {
                MatchManager.instance.SetOtherPlayerGameObject(gameObject);
            }

            // Set up win conditions
            _colliderController.OnPlayerWon += OnPlayerWonRound;

            // Set up animator controls
            _diveAnimatorHash = Animator.StringToHash("Dive");
            _kickAnimatorHash = Animator.StringToHash("Kick");
            _idleAnimatorHash = Animator.StringToHash("Idle");      
        }

        private void OnEnable() {
            if (_photonView != null) {
                if (_photonView.IsMine) {
                    if (InputManager.instance != null) {
                        InputManager.instance.gameInput.OnPrimaryButtonStarted += StartDive;
                        InputManager.instance.gameInput.OnSecondaryButtonStarted += StartKick;
                    }
                }
            } else {
                Debug.LogError("Cannot find photon view to determine input owner of this FighterController");
            }
        }

        private void OnDisable() {
            if (InputManager.instance != null) {
                InputManager.instance.gameInput.OnPrimaryButtonStarted -= StartDive;
                InputManager.instance.gameInput.OnSecondaryButtonStarted -= StartKick;
            }
        }

        [PunRPC]
        private void HandleStartDive() {
            if (Dive()) {
                // Reset the other triggers
                _animator.ResetTrigger(_idleAnimatorHash);
                _animator.ResetTrigger(_kickAnimatorHash);

                _animator.SetTrigger(_diveAnimatorHash);
            }
        }

        [PunRPC]
        private void HandleStartKick() {
            if (Kick()) {
                // Reset the other triggers
                _animator.ResetTrigger(_idleAnimatorHash);
                _animator.ResetTrigger(_diveAnimatorHash);

                _animator.SetTrigger(_kickAnimatorHash);
            }
        }

        private bool Dive() {
            if (isGrounded) {
                _rigidbody2D.AddForce(_normalizedDiveDirectionVector * jumpForce);
                isGrounded = false;
                return true;
            }
            return false;
        }

        private bool Kick() {
            if (!isGrounded) {
                _rigidbody2D.AddForce(_normalizedKickDirectionVector * kickForce);
                isKicking = true;
                return true;
            }
            return false;
        }

        // These RPC's are buffered to give the client and master the most sychronization
        private void StartDive(HardwareInput input) {
            _photonView.RPC("HandleStartDive", RpcTarget.AllViaServer);
        }

        private void StartKick(HardwareInput input) {
            _photonView.RPC("HandleStartKick", RpcTarget.AllViaServer);
        }

        private bool ShouldFlipPlayerModel() {
            // If this player is already flipped, we do the inverse of the calculations we would normally do
            float modifier = _isPlayerModelFlipped ? -1.0f : 1.0f;

            switch (NetworkManager.playerNumber) {
                // Player 1 should always be on the right, player 2 should always be on the left
                // Inverse for if they players are already switched
                case PlayerNumber.Player1:  {
                    if (_photonView.IsMine) {
                        return (modifier * gameObject.transform.position.x) < (modifier * MatchManager.instance.otherPlayerGameObject.transform.position.x);
                    } else {
                        return (modifier * gameObject.transform.position.x) > (modifier * MatchManager.instance.playerGameObject.transform.position.x);
                    }
                }
                case PlayerNumber.Player2: {
                    if (_photonView.IsMine) {
                        return (modifier * gameObject.transform.position.x) > (modifier * MatchManager.instance.otherPlayerGameObject.transform.position.x);
                    } else {
                        return (modifier * gameObject.transform.position.x) < (modifier * MatchManager.instance.playerGameObject.transform.position.x);
                    }
                }
                // If you are here this is an error
                default: {
                    Debug.LogError("Missing player numbers for determining model orientation");
                    return false;
                }
            }
        }

        public void FlipPlayerModel() {
            // Fancy little XOR for the flag
            _isPlayerModelFlipped ^= true;

            _animator.transform.localScale = new Vector3(-1.0f * _animator.transform.localScale.x, _animator.transform.localScale.y, _animator.transform.localScale.z);
            _colliderController.transform.localScale = new Vector3(-1.0f * _colliderController.transform.localScale.x, _colliderController.transform.localScale.y, _colliderController.transform.localScale.z);
            _normalizedKickDirectionVector = new Vector2(-1.0f * _normalizedKickDirectionVector.x, _normalizedKickDirectionVector.y);
        }

        public void ResetPlayerModel() {
            if (_isPlayerModelFlipped) {
                FlipPlayerModel();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collision.enabled) {
                isGrounded = true;
                isKicking = false;

                // Reset the other triggers
                _animator.ResetTrigger(_kickAnimatorHash);
                _animator.ResetTrigger(_diveAnimatorHash);

                _animator.SetTrigger(_idleAnimatorHash);

                // Stop the movement of the player, to prevent rigidbodies from falling over

                if (collision.gameObject.layer == LayerMask.NameToLayer("ArenaGround")) {
                    _rigidbody2D.velocity = Vector2.zero;
                    _rigidbody2D.angularVelocity = 0f;
                    _rigidbody2D.Sleep();
                }

                if (ShouldFlipPlayerModel()) {
                    MatchManager.instance.playerGameObject.GetComponent<FighterController>().FlipPlayerModel();
                    MatchManager.instance.otherPlayerGameObject.GetComponent<FighterController>().FlipPlayerModel();
                }
            }
        }

        // Resizes collider to match sprite bounds
        public void ResizeGeometryCollider() {
            Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

            if (_boxCollider.size != spriteSize) {
                _boxCollider.size = spriteSize;
                _boxCollider.offset = _spriteRenderer.sprite.bounds.center;
            }
        }

        public void OnPlayerWonRound() {
            MatchManager.instance.SetPlayerWonRound(NetworkManager.playerNumber);
        }

        private void Update() {
            // TODO: Find a better way to do this other than checking every frame
            ResizeGeometryCollider();
        }
    }
}
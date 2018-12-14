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
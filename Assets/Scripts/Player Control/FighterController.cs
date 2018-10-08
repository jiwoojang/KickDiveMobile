using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Hardware;

namespace KickDive.Fighter {
    [RequireComponent(typeof(Rigidbody2D))]
    public class FighterController : MonoBehaviour {

        public float jumpForce;
        public float kickForce;
        public bool isGrounded { get; private set; }
        public bool isKicking { get; private set; }
        public Vector2 kickDirectionVector = new Vector2(1.0f, -1.0f);

        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private BoxCollider2D _boxCollider;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _normalizedKickDirectionVector;
        private int _diveAnimatorHash;
        private int _kickAnimatorHash;
        private int _idleAnimatorHash;

        private void Awake() {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _normalizedKickDirectionVector = kickDirectionVector.normalized;
            _diveAnimatorHash = Animator.StringToHash("Dive");
            _kickAnimatorHash = Animator.StringToHash("Kick");
            _idleAnimatorHash = Animator.StringToHash("Idle");
        }

        private void OnEnable() {
            // Debug purposes
            if (InputManager.instance != null) {
                InputManager.instance.gameInput.OnPrimaryButtonStarted += StartDive;
                InputManager.instance.gameInput.OnSecondaryButtonStarted += StartKick;
            }
        }

        private void OnDisable() {
            if (InputManager.instance != null) {
                InputManager.instance.gameInput.OnPrimaryButtonStarted -= StartDive;
                InputManager.instance.gameInput.OnSecondaryButtonStarted -= StartKick;
            }
        }

        private bool Dive() {
            if (isGrounded) {
                _rigidbody2D.AddForce(Vector2.up * jumpForce);
                isGrounded = false;
                return true;
            }
            return false;
        }

        private bool Kick() {
            if (!isGrounded) {
                _rigidbody2D.AddForce(kickDirectionVector * kickForce);
                isKicking = true;
                return true;
            }
            return false;
        }

        private void StartDive(HardwareInput input) {
            if (Dive()) {
                // Reset the other triggers
                _animator.ResetTrigger(_idleAnimatorHash);
                _animator.ResetTrigger(_kickAnimatorHash);

                _animator.SetTrigger(_diveAnimatorHash);
            }
        }

        private void StartKick(HardwareInput input) {
            if (Kick()) {
                // Reset the other triggers
                _animator.ResetTrigger(_idleAnimatorHash);
                _animator.ResetTrigger(_diveAnimatorHash);

                _animator.SetTrigger(_kickAnimatorHash);
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
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
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

        private void Update() {
            // TODO: Find a better way to do this other than checking every frame
            ResizeGeometryCollider();
        }
    }
}
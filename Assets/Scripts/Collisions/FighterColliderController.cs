using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Utility;

// Base class for managing fighter collider groups, and changing collider group states based on animation frames
namespace KickDive.Fighter {
    public class FighterColliderController : MonoBehaviour {

        public delegate void FighterWonRound();

        public event FighterWonRound OnPlayerWonRound;

        [SerializeField]
        private AnimationEventRepeater _animationEventController;
        [SerializeField]
        protected List<FighterColliderGroup> _colliderGroups;

        private void OnEnable() {
            if (_animationEventController != null){
                _animationEventController.OnAnimationEventTriggered += HandleAnimationEvent;
            } else {
                Debug.LogError("Attempting to intialize a ColliderController with no animation event repeater to subscribe to");
            }

            if (_colliderGroups.Count != 0){
                foreach(FighterColliderGroup colliderGroup in _colliderGroups){
                    colliderGroup.OnHitDetected += HandleHitDetected;
                }
            }
        }

        private void OnDisable() {
            if (_animationEventController != null) {
                _animationEventController.OnAnimationEventTriggered -= HandleAnimationEvent;
            }

            if (_colliderGroups.Count != 0) {
                foreach (FighterColliderGroup colliderGroup in _colliderGroups) {
                    colliderGroup.OnHitDetected -= HandleHitDetected;
                }
            }
        }

        // Override methods

        // Note that this method will handle animation changes AFTER the update method for this frame has been called
        // Any changes to colliders will affect physics for the NEXT FixedUpdate call, and thus the next frame
        public virtual void HandleAnimationEvent(AnimationEventRepeater.AnimationEventArgs eventArgs){ }

        public virtual void HandleHitDetected(FighterColliderGroup.CollisionType collisionType){
            if (collisionType == FighterColliderGroup.CollisionType.Hit) {
                Debug.Log(gameObject.name + " TOOK a hit");
            } else if (collisionType == FighterColliderGroup.CollisionType.Hurt) {
                Debug.Log(gameObject.name + " GAVE a hit");

                // Emit round won event for winner
                if (OnPlayerWonRound != null)
                    OnPlayerWonRound();
            }
        }
    }
}
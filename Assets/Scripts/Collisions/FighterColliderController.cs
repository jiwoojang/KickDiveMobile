using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KickDive.Utility;


namespace KickDive.Fighter {
    public class FighterColliderController : MonoBehaviour {

        [SerializeField]
        private AnimationEventRepeater _animationEventController;
        [SerializeField]
        private List<FighterColliderGroup> _colliderGroups;

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

        // Note that this method will handle animation changes AFTER the update method for this frame has been called
        // Any changes to colliders will affect physics for the NEXT FixedUpdate call, and thus the next frame
        private void HandleAnimationEvent(AnimationEventRepeater.AnimationEventArgs eventArgs){
            //Debug.Log("Animation Event Handling");
        }

        private void HandleHitDetected(FighterColliderGroup.CollisionType collisionType){
            if (collisionType == FighterColliderGroup.CollisionType.Hit) {
                Debug.Log(gameObject.name + " TOOK a hit");
            } else if (collisionType == FighterColliderGroup.CollisionType.Hurt) {
                Debug.Log(gameObject.name + " GAVE a hit");
            }
        }
    }
}
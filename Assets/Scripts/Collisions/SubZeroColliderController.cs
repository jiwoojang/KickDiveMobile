using System.Collections;
using System.Collections.Generic;
using KickDive.Utility;
using UnityEngine;

// Wrapper for Sub Zero specific collider groups
namespace KickDive.Fighter {
    public class SubZeroColliderController : FighterColliderController {

        // Indices corresponding to animation states
        // 0 = Idle
        // 1 = DiveStart
        // 2 = DiveEnd
        // 3 = KickStart
        // 4 = KickEnd
        private int _animationState = 0;

        private void Start() {
            if (_colliderGroups.Count == 0) {
                Debug.LogError("Sub Zero initiated collider controller with no collider groups to reference");
            }
        }

        public override void HandleAnimationEvent(AnimationEventRepeater.AnimationEventArgs eventArgs) {
            int newStateIndex = eventArgs.intValue;

            // Only when changing animation states
            if (newStateIndex != _animationState) {
                if (_colliderGroups[newStateIndex] != null) {
                    _colliderGroups[_animationState].gameObject.SetActive(false);
                    _colliderGroups[newStateIndex].gameObject.SetActive(true);

                    _animationState = newStateIndex;
                } else {
                    Debug.LogError("Accesing collider group at index " + newStateIndex + " that does not exist");
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract group for holding and managing multuple colliders in a configuration
namespace KickDive.Fighter {
    public class FighterColliderGroup : MonoBehaviour {

        public enum CollisionType {
            None, 
            Hit,
            Hurt
        }

        public delegate void CollisionDetected(CollisionType collisionType);
        public event CollisionDetected OnHitDetected;

        [SerializeField]
        private List<FighterCollider> _fighterColliders;

        private void OnEnable() {
            if (_fighterColliders.Count != 0) {
                foreach (FighterCollider fighterCollider in _fighterColliders) {
                    fighterCollider.OnFighterCollision += HandleCollisionDetected;
                }
            } else {
                Debug.LogError("Fighter Collider Group has no colliders");
            }
        }

        private void OnDisable() {
            if (_fighterColliders.Count != 0) {
                foreach (FighterCollider fighterCollider in _fighterColliders) {
                    fighterCollider.OnFighterCollision -= HandleCollisionDetected;
                }
            }
        }

        // TAKE a hit
        // GIVE a hurt
        private void HandleCollisionDetected(FighterCollider.ColliderType thisColliderType, FighterCollider.ColliderType collidedColliderType) {
            // If a HURTBOX hit a HITBOX
            if (thisColliderType == FighterCollider.ColliderType.Hurtbox && collidedColliderType == FighterCollider.ColliderType.Hitbox){
                if (OnHitDetected != null) {
                    OnHitDetected(CollisionType.Hit);
                }
            }

            // If a HITBOX hit a HURTBOX
            if (thisColliderType == FighterCollider.ColliderType.Hitbox && collidedColliderType == FighterCollider.ColliderType.Hurtbox){
                if (OnHitDetected != null) {
                    OnHitDetected(CollisionType.Hurt);
                }
            }
        }
    }
}
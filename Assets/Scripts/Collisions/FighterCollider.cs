using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Collision events for hitboxes and hurtboxes ONLY. Player collisions with map geometry are handled in FighterController.cs

[RequireComponent(typeof(Collider2D))]
public class FighterCollider : MonoBehaviour {

    public enum ColliderType{
        None,
        Hitbox,
        Hurtbox
    }

    public delegate void FighterCollision(ColliderType thisColliderType, ColliderType collidedColliderType);
    public event FighterCollision OnFighterCollision;

    public ColliderType colliderType;

    private Collider2D  _collider;

    private void Awake() {
        _collider = GetComponent<Collider2D>();

        if (colliderType == ColliderType.None){
            Debug.LogError("This FighterCollider has not been given a type! This will cause errors");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // OnCollision events can be sent to disabled GameObjects, check if this is one we care about
        if (collision.enabled){
            FighterCollider collidedFighterCollider = collision.gameObject.GetComponent<FighterCollider>();

            if (collidedFighterCollider != null){
                if (OnFighterCollision != null){
                    OnFighterCollision(colliderType, collidedFighterCollider.colliderType);
                }
            } else {
                Debug.Log("Colliding with non-FighterCollider");
            }
        }
    }
}

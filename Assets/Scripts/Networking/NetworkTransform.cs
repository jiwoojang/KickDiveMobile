using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Transform))]
public class NetworkTransform : MonoBehaviour, IPunObservable {

    public struct NetworkTransformSnapshot {
        public Vector3 position;
        public float timestamp;

        public NetworkTransformSnapshot (Vector3 position, float timestamp) {
            this.position = position;
            this.timestamp = timestamp;
        }
    }

    private Rigidbody _rigidbody;

    // Caches the most recently sent network snapshot
    // TODO: Cache network snapshots in a ring buffer and interpolate across a wider window.
    private NetworkTransformSnapshot _recentSnapshot;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _recentSnapshot = new NetworkTransformSnapshot(transform.position, Time.time);
    }

    // Note that the transform serializes via the Unreliable On Change protocol:
    // See https://doc.photonengine.com/en-us/pun/v1/getting-started/feature-overview
    // Unreliable on Change will check each update for changes. If all values stay the same as previously sent, one update will be sent as reliable 
    // and then the owner stops sending updates unless things change again.This is good for GameObjects that might stop moving and that don't 
    // create further updates for a while. Like Boxes that are no longer moved after finding their place.

   void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(info.timestamp);
        }
        else {
            _recentSnapshot.position = (Vector3)stream.ReceiveNext();
            _recentSnapshot.timestamp = (float)stream.ReceiveNext();
        }
    }

    private void FixedUpdate() {
        // If this object has a rigidbody, use the rigidbody methods to move object
        if (_rigidbody != null) {
            _rigidbody.MovePosition(_recentSnapshot.position);
        } else {
            transform.position = Vector3.Lerp(transform.position, _recentSnapshot.position, Time.fixedDeltaTime);
        }
    }
}

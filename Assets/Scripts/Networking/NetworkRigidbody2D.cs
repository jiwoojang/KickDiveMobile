using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkRigidbody2D : MonoBehaviour, IPunObservable {

    public struct NetworkTransformSnapshot {
        public Vector2  position;
        public float    rotation; 
        public Vector2  velocity;
        public float    angularVelocity;
        public double   timestamp;
    }

    private float       _teleportThresholdDistance = 2.0f;
    private Rigidbody2D _rigidbody;
    private PhotonView  _photonView;
    private float       _recentTraveledDistance;
    private float       _recentTraveledAngle;

    // Caches the most recently sent network snapshot
    // TODO: Cache network snapshots in a ring buffer and interpolate across a wider window.
    private NetworkTransformSnapshot _recentSnapshot;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
        _photonView = GetComponent<PhotonView>();
        _recentSnapshot = new NetworkTransformSnapshot();
    }

    // Note that the transform serializes via the Unreliable On Change protocol:
    // See https://doc.photonengine.com/en-us/pun/v1/getting-started/feature-overview
    // Unreliable on Change will check each update for changes. If all values stay the same as previously sent, one update will be sent as reliable 
    // and then the owner stops sending updates unless things change again.This is good for GameObjects that might stop moving and that don't 
    // create further updates for a while. Like Boxes that are no longer moved after finding their place.

   public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_rigidbody.position);
            stream.SendNext(_rigidbody.rotation);
            
            stream.SendNext(_rigidbody.velocity);
            stream.SendNext(_rigidbody.angularVelocity);

            stream.SendNext(info.timestamp);
        }
        else {

            float trasmissionTime = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));

            _recentSnapshot.position = (Vector2)stream.ReceiveNext();
            _recentSnapshot.rotation = (float)stream.ReceiveNext();

            _recentSnapshot.velocity = (Vector2)stream.ReceiveNext();
            _recentSnapshot.angularVelocity = (float)stream.ReceiveNext();

            _recentSnapshot.timestamp = (double)stream.ReceiveNext();

            // Cache the distance traveled this frame
            _recentTraveledDistance = Vector2.Distance(_rigidbody.position, _recentSnapshot.position);
            _recentTraveledAngle = Mathf.Abs(_rigidbody.rotation - _recentSnapshot.rotation);

            // Teleport for significant distances
            if (Vector2.Distance(_rigidbody.position, _recentSnapshot.position) > (_teleportThresholdDistance)) {
                _rigidbody.position = _recentSnapshot.position;
            }

            // Update rigidbody with most recent values
            _rigidbody.velocity = _recentSnapshot.velocity;
            _rigidbody.angularVelocity = _recentSnapshot.angularVelocity;

            // Interpolate position
            // This assumes velocity is constant until new velocity values are recieved from owner

            // d = v * t
            _recentSnapshot.position += _recentSnapshot.velocity * trasmissionTime;
            _recentSnapshot.rotation += _recentSnapshot.angularVelocity * trasmissionTime;
        }
    }

    private void FixedUpdate() {
        // Synchronize other client positions
        if (!_photonView.IsMine) {
            _rigidbody.position = Vector2.MoveTowards(_rigidbody.position, _recentSnapshot.position, (_recentTraveledDistance / PhotonNetwork.SerializationRate));
            _rigidbody.rotation = Mathf.MoveTowards(_rigidbody.rotation, _recentSnapshot.rotation, (_recentTraveledAngle/ PhotonNetwork.SerializationRate));
        }
    }
}

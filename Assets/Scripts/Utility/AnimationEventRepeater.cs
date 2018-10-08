using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Short wrapper for getting animation events from the animator on components not directly attached to one the animator is on
// Get a reference to this if you want animation events from an animator
namespace KickDive.Utility {
    [RequireComponent(typeof(Animator))]
    public class AnimationEventRepeater : MonoBehaviour {

        public struct AnimationEventArgs {
            public int intValue;
            public float floatValue;
            public string stringValue;
            public object objectReference;

            public AnimationEventArgs(int intVal, float floatVal) {
                intValue = intVal;
                floatValue = floatVal;
                stringValue = "";
                objectReference = null;
            }

            public AnimationEventArgs(int intVal, float floatVal, string stringVal) {
                intValue = intVal;
                floatValue = floatVal;
                stringValue = stringVal;
                objectReference = null;
            }

            public AnimationEventArgs(int intVal, float floatVal, string stringVal, object objectRef) {
                intValue = intVal;
                floatValue = floatVal;
                stringValue = stringVal;
                objectReference = objectRef;
            }
        }

        public delegate void AnimationEventTriggered(AnimationEventArgs eventArts);
        public event AnimationEventTriggered OnAnimationEventTriggered;

        public void TriggerAnimationEvent(AnimationEvent animationEvent) {
            if (OnAnimationEventTriggered != null) {
                AnimationEventArgs eventArgs = new AnimationEventArgs(animationEvent.intParameter, animationEvent.floatParameter);

                if (animationEvent.stringParameter != "") {
                    eventArgs.stringValue = animationEvent.stringParameter;
                }

                if (animationEvent.objectReferenceParameter != null) {
                    eventArgs.objectReference = animationEvent.objectReferenceParameter;
                }

                OnAnimationEventTriggered(eventArgs);
            }
        }
    }
}
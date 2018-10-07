using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hardware {
    public class HardwareInput {

        public delegate void PrimaryButtonStarted(HardwareInput sender);
        public delegate void PrimaryButtonPress(HardwareInput sender);
        public delegate void PrimaryButtonEnded(HardwareInput sender);

        public delegate void SecondaryButtonStarted(HardwareInput sender);
        public delegate void SecondaryButtonPress(HardwareInput sender);
        public delegate void SecondaryButtonEnded(HardwareInput sender);

        public event PrimaryButtonStarted OnPrimaryButtonStarted;
        public event PrimaryButtonPress OnPrimaryButtonPress;
        public event PrimaryButtonEnded OnPrimaryButtonEnded;

        public event SecondaryButtonStarted OnSecondaryButtonStarted;
        public event SecondaryButtonPress OnSecondaryButtonPress;
        public event SecondaryButtonEnded OnSecondaryButtonEnded;

        // Override methods for firing events
        protected virtual void FirePrimaryButtonStarted() {
            if (OnPrimaryButtonStarted != null) {
                OnPrimaryButtonStarted(this);
            }
        }

        protected virtual void FirePrimaryButtonPress() {
            if (OnPrimaryButtonPress != null) {
                OnPrimaryButtonPress(this);
            }
        }

        protected virtual void FirePrimaryButtonEnded() {
            if (OnPrimaryButtonEnded != null) {
                OnPrimaryButtonEnded(this);
            }
        }

        protected virtual void FireSecondaryButtonStarted() {
            if (OnSecondaryButtonStarted != null) {
                OnSecondaryButtonStarted(this);
            }
        }

        protected virtual void FireSecondaryButtonPress() {
            if (OnSecondaryButtonPress != null) {
                OnSecondaryButtonPress(this);
            }
        }

        public virtual void FireSecondaryButtonEnded() {
            if (OnSecondaryButtonEnded != null) {
                OnSecondaryButtonEnded(this);
            }
        }

        // Overrides for use in update functions
        public virtual void GetPrimaryButtonStatus() { }
        public virtual void GetSecondaryButtonStatus() { }
    }
}

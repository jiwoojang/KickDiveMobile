using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.Hardware {
    public class PCDebugInput : HardwareInput {
        private KeyCode _primaryButton = KeyCode.A;
        private KeyCode _secondaryButton = KeyCode.D;

        public override void GetPrimaryButtonStatus() {
            if (Input.GetKeyDown(_primaryButton)) {
                FirePrimaryButtonStarted();
            } else if (Input.GetKey(_primaryButton)) {
                FirePrimaryButtonPress();
            } else if (Input.GetKeyUp(_primaryButton)) {
                FirePrimaryButtonEnded();
            }
        }

        public override void GetSecondaryButtonStatus() {
            if (Input.GetKeyDown(_secondaryButton)) {
                FireSecondaryButtonStarted();
            } else if (Input.GetKey(_secondaryButton)) {
                FireSecondaryButtonPress();
            } else if (Input.GetKeyUp(_secondaryButton)) {
                FireSecondaryButtonEnded();
            }
        }
    }
}
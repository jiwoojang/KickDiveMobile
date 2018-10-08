using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KickDive.Hardware{
    public class XboxControllerInput : HardwareInput {
        private string _primaryButton = "xboxA";    // A button
        private string _secondaryButton = "xboxB";    // B button

        public override void GetPrimaryButtonStatus() {
            if (Input.GetButtonDown(_primaryButton)) {
                FirePrimaryButtonStarted();
            } else if (Input.GetButton(_primaryButton)) {
                FirePrimaryButtonPress();
            } else if (Input.GetButtonUp(_primaryButton)) {
                FirePrimaryButtonEnded();
            }
        }

        public override void GetSecondaryButtonStatus() {
            if (Input.GetButtonDown(_secondaryButton)) {
                FireSecondaryButtonStarted();
            } else if (Input.GetButton(_secondaryButton)) {
                FireSecondaryButtonPress();
            } else if (Input.GetButtonUp(_secondaryButton)) {
                FireSecondaryButtonEnded();
            }
        }
    }
}
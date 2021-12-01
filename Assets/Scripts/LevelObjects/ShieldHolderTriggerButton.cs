using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldHolderTriggerButton : TriggerButton, IShieldExitTriggerHandler
{
    bool shieldHeld = false;

    public override void OnShieldEnterTrigger(Shield shield)
    {
        // Ignore if the shield has been recalled
        if (shield.IsRecalled)
            return;

        shieldHeld = true;
        shield.HoldInPlace(transform);
        ToggleButton();
    }

    public void OnShieldExitTrigger(Shield shield)
    {
        if (shieldHeld)
        {
            shieldHeld = false;
            ToggleButton();
        }
            
    }
}

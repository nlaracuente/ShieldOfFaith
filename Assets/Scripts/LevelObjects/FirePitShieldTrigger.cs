using UnityEngine;

/// <summary>
/// For some strange reason the shield is not colliding/triggering with the Fire Pit
/// Since the shield trigger is getting picked up by the shield which then looks for 
/// an 'IShieldEnterTriggerHandler' component, I opted for this solution to solve the problem for now
/// </summary>
public class FirePitShieldTrigger : MonoBehaviour, IShieldEnterTriggerHandler
{
    FirePit firePit;
    FirePit FirePit
    {
        get
        {
            if (firePit == null)
                firePit = GetComponentInParent<FirePit>();
            return firePit;
        }
    }
    public void OnShieldEnterTrigger(Shield shield) => FirePit.OnShieldEnterTrigger(shield);
}

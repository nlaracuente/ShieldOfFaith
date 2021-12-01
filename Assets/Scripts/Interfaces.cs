using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShieldEnterTriggerHandler
{
    void OnShieldEnterTrigger(Shield shield);
}

public interface IShieldExitTriggerHandler
{
    void OnShieldExitTrigger(Shield shield);
}

public interface IShieldCollisionHandler
{
    void OnShieldCollisionEnter(Shield shield);
}
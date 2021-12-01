using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorListener : MonoBehaviour
{
    public void PlayPlayerWalk() => AudioManager.instance.Play(SFXLibrary.instance.playerWalk);

}

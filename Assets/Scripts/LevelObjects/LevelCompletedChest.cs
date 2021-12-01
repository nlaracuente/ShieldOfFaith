using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletedChest : MonoBehaviour
{
    KeyPad keyPad;
    KeyPad KeyPad
    {
        get
        {
            if (keyPad == null)
                keyPad = FindObjectOfType<KeyPad>();
            return keyPad;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Shield") && KeyPad.KeyCollected)
            GameManager.instance.ReloadScene();
    }
}

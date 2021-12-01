using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeeJewel : MonoBehaviour
{
    [SerializeField]
    Text counter;

    [SerializeField]
    GameObject jewelModel;

    [SerializeField]
    Collider triggerCollider;

    [SerializeField]
    Animator animator;

    [SerializeField]
    Renderer jewelRenderer;

    MaterialPropertyBlock propertyBlock;

    bool pickedUp = false;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        jewelRenderer.GetPropertyBlock(propertyBlock);
    }

    private void Update()
    {
        var total = GameManager.instance.TotalBees;
        var current = GameManager.instance.BeesKilled;
        var showJewel = (current == total);
        
        if(showJewel)
            // Hide Counter since the jewel is collectable 
            counter.text = "";
        else 
            // Update Counter
            counter.text = $"{current}/{total}";

        // Enable trigger/show the jewel
        triggerCollider.enabled = showJewel;

        var color = propertyBlock.GetColor("_Main");
        var alpha = showJewel ? 1f : .25f;
        var nColor = new Color(color.r, color.g, color.b, alpha);
        propertyBlock.SetColor("_Main", nColor);
    }

    private void OnTriggerStay(Collider other)
    {
        if(!pickedUp && other.CompareTag("Player"))
            PickUp();
    }

    void PickUp()
    {
        pickedUp = true;
        jewelModel.SetActive(false);
        GameManager.instance.TriggerGameOver();
    }
}

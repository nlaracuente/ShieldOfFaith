using System.Collections.Generic;
using UnityEngine;

public class BeeMaterial : MonoBehaviour
{
    [SerializeField]
    Renderer beeRenderer;
    Renderer BeeRenderer
    {
        get
        {
            if (beeRenderer == null)
                beeRenderer = GetComponentInChildren<Renderer>();
            return beeRenderer;
        }
    }

    MaterialPropertyBlock propertyBlock;
    MaterialPropertyBlock PropertyBlock
    {
        get
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();
            return propertyBlock;
        }
    }

    [System.Serializable]
    struct BeeMaterials
    {
        public BeeColor beeColor;
        public Texture texture;
    }

    [SerializeField, Tooltip("List all the shield particles available for us to play")]
    List<BeeMaterials> beeMaterials;
    Dictionary<BeeColor, Texture> materialMapping;
    Dictionary<BeeColor, Texture> MaterialMapping
    {
        get
        {
            if (materialMapping == null)
            {
                materialMapping = new Dictionary<BeeColor, Texture>();
                foreach (var beeMaterial in beeMaterials)
                    materialMapping[beeMaterial.beeColor] = beeMaterial.texture;
            }
            return materialMapping;
        }
    }

    /// <summary>
    /// Changing the "material" directly was causing performance issues 
    /// Switches to using property blocks and changing the texture as it is the recommended
    /// way to modify a material for performance.
    /// </summary>
    /// <param name="beeColor"></param>
    public void SetMaterial(BeeColor beeColor)
    {
        if (BeeRenderer != null && MaterialMapping.ContainsKey(beeColor))
        {
            var texture = MaterialMapping[beeColor];
            if(texture != null)
            {
                BeeRenderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetTexture("_MainTex", texture);
                BeeRenderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }
}

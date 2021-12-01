using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BeePath : MonoBehaviour
{
    [SerializeField]
    List<PathNode> nodes;
    public List<PathNode> Nodes
    {
        get
        {
            if (nodes == null)
                nodes = GetComponentsInChildren<PathNode>().ToList();
            return nodes;
        }
    }
}

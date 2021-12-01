using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{    
    public static readonly List<Vector2> cardinalDirections = new List<Vector2>()
    {
        Vector2.up,
        Vector2.left,
        Vector2.down,
        Vector2.right
    };

    public static List<Vector2> AdjacentPoints(Vector2 origin)
    {
        var tiles = new List<Vector2>();
        foreach (var point in cardinalDirections)
            tiles.Add(origin + point);

        return tiles;
    }
}

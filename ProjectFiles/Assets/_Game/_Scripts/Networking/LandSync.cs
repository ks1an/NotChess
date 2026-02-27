using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LandSync : NetworkBehaviour
{
    [Rpc(SendTo.NotMe)]
    public void GeneratedLandRpc(string name, int[] tilesX, int[] tilesY)
    {
        Board.Instance.landscapeGenerator.GenerateLandscapeByName(name, tilesX, tilesY);
    }

    [Rpc(SendTo.NotMe)]
    public void GeneratedInteractLandRpc(string name, int xCenter, int yCenter, int[] tilesX, int[] tilesY, int pattern)
    {
        List<Vector2Int> workzone = new();
        Vector2Int offset = new(xCenter, yCenter);
        workzone.Add(offset);
        for(int i = 0; i < tilesX.Length; i++)
        {
            offset.x = tilesX[i];
            offset.y = tilesY[i];
            workzone.Add(offset);
        }
        Board.Instance.interactLandscapeGenerator.GenerateInteractLandscapeByName(name, xCenter, yCenter, workzone, pattern);
    }
}

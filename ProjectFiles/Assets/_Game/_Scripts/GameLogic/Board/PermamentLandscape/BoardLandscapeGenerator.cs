using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardLandscapeGenerator : MonoBehaviour
{
    [SerializeField] List<LandscapeObject> landscapeObjects = new();

    Board board;
    List<Vector2Int> tiles = new();

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public void GenerateRandomLandscape()
    {
        if (landscapeObjects.Count == 0) return;

        foreach (Tile tile in board.tilesController.tiles)
            tiles.Add(tile.coord);

        MathOperations math = MathOperations.GetInstance();
        foreach (LandscapeObject obj in landscapeObjects)
        {
            List<int> indexOfTilesWithLandscape = new();
            if (obj.chance >= UnityEngine.Random.Range(0, 100))
            {
                for (int i = 0; i < math.GetSafeRandom(obj.minAmount, obj.maxAmount + 1, true); i++)
                {
                    int index = UnityEngine.Random.Range(0, tiles.Count);
                    if (!indexOfTilesWithLandscape.Contains(index))
                        indexOfTilesWithLandscape.Add(index);
                }
            }
            int[] tilesX = new int[indexOfTilesWithLandscape.Count],
                tilesY = new int[indexOfTilesWithLandscape.Count];

            for (int i = 0; i < indexOfTilesWithLandscape.Count; i++)
            {
                tilesX[i] = tiles[indexOfTilesWithLandscape[i]][0];
                tilesY[i] = tiles[indexOfTilesWithLandscape[i]][1];
            }

            if (GameController.Instance.states.isNetMatch)
                GameController.Instance.netMatch.landSync.GeneratedLandRpc(obj.landName, tilesX, tilesY);
            GenerateLandscape(obj, tilesX, tilesY);
        }

        tiles.Clear();
    }


    public void GenerateLandscapeByName(string name, int[] indexesX, int[] indexesY)
    {
        LandscapeObject obj = null;
        foreach (var land in landscapeObjects)
            if (land.landName == name)
                obj = land;

        if (obj != null)
            GenerateLandscape(obj, indexesX, indexesY);
        else
            Debug.LogError($"Land with name:({name}) in not find! Critical error.");
    }

    public void GenerateLandscape(LandscapeObject obj, int[] indexesX, int[] indexesY)
    {
        for (int i = 0; i < indexesX.Length; i++)
        {
            LandscapeObject land = Instantiate(obj.gameObject).GetComponent<LandscapeObject>();
            land.Init(indexesX[i], indexesY[i]);
            tiles.Remove(board.tilesController.tiles[indexesX[i], indexesY[i]].coord);
        }
        Board.Instance.SetLandReady(true);
    }
}
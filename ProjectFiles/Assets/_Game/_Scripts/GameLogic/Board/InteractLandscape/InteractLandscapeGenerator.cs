using System.Collections.Generic;
using UnityEngine;

public class InteractLandscapeGenerator : MonoBehaviour
{
    [SerializeField] List<InteractLandscapeObject> landscapeObjects = new();
    Board board;
    List<Vector2Int> freeTiles = new();

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    public void TryGenerateInteractLandscape()
    {
        if (landscapeObjects.Count == 0) return;

        InteractLandscapeObject land = null;
        foreach (var obj in landscapeObjects)
            if (obj.CheckConditionOfExistence())
            {
                land = obj;
                break;
            }
        if (land == null) return;

        foreach (Tile tile in board.tilesController.tiles)
            if (!IsTileHasPiece(tile) && !tile.Stats.IsUnderBuffs())
                freeTiles.Add(tile.coord);

        List<Vector2Int> workzone;
        MathOperations.GetInstance().ShuffleList(freeTiles);
        foreach (Vector2Int freeTile in freeTiles)
        {
            workzone = land.CheckConditionOfPlacing(freeTile.x, freeTile.y, freeTiles);
            if (workzone != null && workzone.Count > 0)
            {
                int pattern = Random.Range(0, land.patterns.Count);
                if (GameController.Instance.states.isNetMatch)
                {
                    TurnTimer.GetInstance().StartTimer(
                        1,
                        () => { GenerateInteractLandscape(land, freeTile.x, freeTile.y, workzone, pattern); },
                        null, out TurnTimerSubscriber sub);

                    int[] tilesX = new int[workzone.Count], tilesY = new int[workzone.Count];
                    for(int i = 0; i < workzone.Count; i++)
                    {
                        tilesX[i] = workzone[i].x;
                        tilesY[i] = workzone[i].y;
                    }

                    GameController.Instance.netMatch.landSync.GeneratedInteractLandRpc(
    land.landName, freeTile.x, freeTile.y, tilesX, tilesY, pattern);
                }
                else
                    GenerateInteractLandscape(land, freeTile.x, freeTile.y, workzone, pattern);

                break;
            }
        }
        freeTiles.Clear();
    }

    public void GenerateInteractLandscapeByName(string name, int x, int y, List<Vector2Int> workzone, int pattern)
    {
        InteractLandscapeObject obj = null;
        foreach (var land in landscapeObjects)
            if (land.landName == name)
                obj = land;

        if (obj != null)
            GenerateInteractLandscape(obj, x, y, workzone, pattern);
        else
            Debug.LogError($"Land with name:({name}) in not find! Critical error.");
    }

    public void GenerateInteractLandscape(InteractLandscapeObject obj, int x, int y, List<Vector2Int> workzone, int pattern)
    {
        InteractLandscapeObject land = Instantiate(obj.gameObject).GetComponent<InteractLandscapeObject>();
        land.Init(x, y, workzone, pattern);
        freeTiles.Remove(board.tilesController.tiles[x, y].coord);
    }

    bool IsTileHasPiece(Tile tile)
    {
        if (board.piecesController.pieces[tile.coord.x, tile.coord.y] == null)
            return false;
        return true;

    }
}
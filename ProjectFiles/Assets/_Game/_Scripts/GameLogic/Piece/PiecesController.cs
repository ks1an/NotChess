using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(TilesController))]
public sealed class PiecesController : MonoBehaviour
{
    [HideInInspector] public List<Vector2Int> availableMoves = new();
    [HideInInspector] public PieceData[,] pieces;
    [HideInInspector] public PieceData currentlySelectingPiece;

    [SerializeField] private ResumeOfPrefab[] namedPrefabs;
    Dictionary<string, GameObject> prefabDict;
    float upValueWhileSelectingPiece;

    TilesController tilesController;
    GameController match;

    public void SetSettings()
    {
        tilesController = GetComponent<TilesController>();
        match = GameController.Instance;

        upValueWhileSelectingPiece = match.player.upValueWhileSelectingPiece;
        pieces = new PieceData[match.matchSettings.tileCountX, match.matchSettings.tileCountY];

        prefabDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < namedPrefabs.Length; i++)
        {
            if (namedPrefabs[i].prefab != null)
            {
                namedPrefabs[i].id = namedPrefabs[i].prefab.name;
                prefabDict[namedPrefabs[i].id] = namedPrefabs[i].prefab;
            }
            else
                Debug.LogError("NamedPrefabs with num: " + i + " havent prefab!!!");
        }
    }

    public PieceData GeneratePiece(Team type, string prefabId)
    {
        PieceData pieceLogic = null;

        if (prefabDict.TryGetValue(prefabId, out GameObject prefab))
        {
            var pieceView = Instantiate(prefab, transform).GetComponent<PieceView>();
            pieceView.Init();
            pieceLogic = pieceView.runtimeData;
        }
        else
        {
            Debug.LogError($"Prefab with '{prefabId}' not finded in dictionary!");
        }

        pieceLogic.team = type;
        return pieceLogic;
    }

    #region TransformPiece

    public void SetPositionSinglePiece(int x, int y, bool instantly)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        match.states.move.SetUnitPos(x, y, tilesController.GetTileCenter(x, y), instantly);
    }

    public void MoveTo(int originalX, int originalY, int x, int y)
    {
        PieceData curPiece = pieces[originalX, originalY];

        if (pieces[x, y] != null && curPiece != null)
            match.states.move.TryDestroyUnit(x, y, true, curPiece.team, false, true);

        pieces[x, y] = curPiece;
        pieces[originalX, originalY] = null;

        if (match.states.isNetMatch)
        {
            match.netMatch.unitSync.UpdateUnitArrayOnClientsRpc(originalX, originalY, 0, Team.None);
            match.netMatch.unitSync.UpdateUnitArrayOnClientsRpc(x, y, curPiece.view.gameObject.GetComponent<NetworkObject>().NetworkObjectId, curPiece.team);
        }

        SetPositionSinglePiece(x, y, false);
    }

    #endregion

    #region Selecting

    public void OnSelectingPiece(Vector2Int hitPos)
    {
        if (!(pieces[hitPos.x, hitPos.y].team == Team.Zero && match.states.isMoveOfZero
            || pieces[hitPos.x, hitPos.y].team == Team.Cross && !match.states.isMoveOfZero))
            return;
        if (!(match.player.GetLocalPlayerTeam() == Team.Zero && match.states.isMoveOfZero ||
            match.player.GetLocalPlayerTeam() == Team.Cross && !match.states.isMoveOfZero))
            return;

        currentlySelectingPiece = pieces[hitPos.x, hitPos.y];
        availableMoves = currentlySelectingPiece.GetAvailableMoves(pieces);

        Vector3 tileCenter = tilesController.GetTileCenter(hitPos.x, hitPos.y);
        match.states.move.SetUnitPos(hitPos.x, hitPos.y, new Vector3(tileCenter.x, upValueWhileSelectingPiece, tileCenter.z));

        tilesController.HighlighTiles(availableMoves);
    }

    public void OnBreakSelectingPiece(Vector2Int hitPos)
    {
        Vector2Int previousPos = new(currentlySelectingPiece.currentX, currentlySelectingPiece.currentY);

        if (ContainsValidMove(ref availableMoves, new Vector2(hitPos.x, hitPos.y)))
        {
            match.states.move.MoveUnit(previousPos.x, previousPos.y, hitPos.x, hitPos.y);
        }
        else
            match.states.move.SetUnitPos(previousPos.x, previousPos.y, tilesController.GetTileCenter(previousPos.x, previousPos.y));

        currentlySelectingPiece = null;
        if (match.states.lastWinTeam == Team.None)
            tilesController.RemoveHighlightTiles(availableMoves);
    }
    #endregion

    public bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }
}

[Serializable]
public struct ResumeOfPrefab
{
    public string id;
    public GameObject prefab;
}

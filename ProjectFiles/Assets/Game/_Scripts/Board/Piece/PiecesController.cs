using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public sealed class PiecesController : MonoBehaviour
{
    [HideInInspector] public List<Vector2Int> availableMoves = new();
    [HideInInspector] public Piece[,] pieces;
    [HideInInspector] public Piece currentlySelectingPiece;

    GameObject crossPrefab, zeroPrefab;
    float upValueWhileSelectingPiece;

    TilesController tilesController;
    MatchController match;

    public void SetSettings()
    {
        tilesController = GetComponent<TilesController>();
        match = MatchController.Instance;

        zeroPrefab = match.player.zeroPrefab;
        crossPrefab = match.player.crossPrefab;
        upValueWhileSelectingPiece = match.player.upValueWhileSelectingPiece;
        pieces = new Piece[match.settings.tileCountX, match.settings.tileCountY];
    }

    public Piece GeneratePiece(Team type)
    {
        Piece piece;
        if (type == Team.Cross)
            piece = Instantiate(crossPrefab, transform).GetComponent<Piece>();
        else
            piece = Instantiate(zeroPrefab, transform).GetComponent<Piece>();

        piece.team = type;

        return piece;
    }

    #region TransformPiece

    public void SetPositionSinglePiece(int x, int y, bool instantly)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        match.states.SetUnitPos(x, y, tilesController.GetTileCenter(x, y), instantly);
    }

    public void MoveTo(int originalX, int originalY, int x, int y)
    {
        Piece curPiece = pieces[originalX, originalY];

        if (pieces[x, y] != null)
        {
            if (curPiece.team == pieces[x, y].team)
                return;
            match.states.TryDestroyUnit(x, y, true);
        }
        pieces[x, y] = curPiece;
        pieces[originalX, originalY] = null;

        if (match.states.isNetMatch)
        {
            match.netMatch.UpdateUnitArrayOnClientsRpc(originalX, originalY, 0, Team.None);
            match.netMatch.UpdateUnitArrayOnClientsRpc(x, y, curPiece.gameObject.GetComponent<NetworkObject>().NetworkObjectId, curPiece.team);
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
        availableMoves = currentlySelectingPiece.GetAvailableMoves(ref pieces, match.settings.tileCountX, match.settings.tileCountY);

        Vector3 tileCenter = tilesController.GetTileCenter(hitPos.x, hitPos.y);
        match.states.SetUnitPos(hitPos.x, hitPos.y, new Vector3(tileCenter.x, upValueWhileSelectingPiece, tileCenter.z));

        tilesController.HighlighTiles(availableMoves);
    }

    public void OnBreakSelectingPiece(Vector2Int hitPos)
    {
        Vector2Int previousPos = new(currentlySelectingPiece.currentX, currentlySelectingPiece.currentY);

        if (ContainsValidMove(ref availableMoves, new Vector2(hitPos.x, hitPos.y)))
        {
            match.states.MoveUnit(previousPos.x, previousPos.y, hitPos.x, hitPos.y);
        }
        else
            match.states.SetUnitPos(previousPos.x, previousPos.y, tilesController.GetTileCenter(previousPos.x, previousPos.y));

        currentlySelectingPiece = null;
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

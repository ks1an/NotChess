using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TilesController), typeof(PiecesController))]
public sealed class Board : MonoBehaviour
{
    public static Board Instance;
    public static Action onBoardGenerated;

    public TilesController tilesController;
    public PiecesController piecesController;

    GameController match;
    MatchSettings settings;
    GameObject modalWindow;

    Camera curCamera;
    Vector2Int curHoverTile;
    bool isBoardReady;
    bool isGameStart;
    bool isPlayerControl;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        DontDestroyOnLoad(this);
        match = GameController.Instance;
    }

    public void GenerateBoard()
    {
        settings = GameController.Instance.settings;
        modalWindow = ModalViewWindowController.Instance.modalWindow.gameObject;
        curHoverTile = -Vector2Int.one;
        curCamera = Camera.main;

        match.states.OnGameStarted += OnGameStart;
        match.states.OnGameTied += OnGameEnd;
        match.states.OnGameWin += OnGameEnd;
        match.states.OnLeaveMatch += DestroyBoard;

        tilesController.SetSettings();  //FirstSetSettings is tiles!
        tilesController.GenerateTiles();

        piecesController.SetSettings(); //Second

        isPlayerControl = !match.states.isDemonstrationMatchAiVsAi;
        isBoardReady = true;
        onBoardGenerated?.Invoke();
    }

    void Update()
    {
        if (!isGameStart || !isBoardReady || !isPlayerControl)
            return;

        if (!curCamera)
        {
            curCamera = Camera.main;
            return;
        }

        Ray ray = curCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 50, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            if (modalWindow.activeSelf)
                return;
            Vector2Int hitPos = tilesController.GetTileIndex(info.transform.gameObject);

            #region HoverTile
            if (curHoverTile == -Vector2Int.one)
            {
                curHoverTile = hitPos;
                tilesController.tiles[hitPos.x, hitPos.y].gameObject.layer = LayerMask.NameToLayer("Hover");
            }

            if (curHoverTile != hitPos)
            {
                tilesController.tiles[curHoverTile.x, curHoverTile.y].gameObject.layer = piecesController.ContainsValidMove(ref piecesController.availableMoves, curHoverTile) ?
                   LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                curHoverTile = hitPos;
                tilesController.tiles[hitPos.x, hitPos.y].gameObject.layer = LayerMask.NameToLayer("Hover");

            }
            #endregion

            #region CardMove
            if (PlayerDeck.Instance.hand.CurrentSelectCard != null)
            {
                List<Vector2Int> availabe = PlayerDeck.Instance.hand.CurrentSelectCard.GetAvailableMoves(settings.tileCountX, settings.tileCountY, hitPos.x, hitPos.y);
                tilesController.HighlighTiles(availabe);
                tilesController.RemoveAllHighlightExcludeCurrentOnes(availabe);
                return;
            }

            #endregion

            #region PutPiece

            if (Input.GetMouseButtonDown(0) && piecesController.currentlySelectingPiece == null && piecesController.pieces[hitPos.x, hitPos.y] == null
                && !tilesController.tiles[hitPos.x, hitPos.y].banPutUnitsOnTile)
            {
                if (match.player.IsMyTurnOrNot())
                    match.states.TryCreateUnitOnBoard(hitPos.x, hitPos.y, match.player.GetLocalPlayerTeam());

                return;
            }
            #endregion

            #region SelectPiece

            //TODO: NewInputSystem
            if (Input.GetMouseButtonDown(0) && piecesController.pieces[hitPos.x, hitPos.y] != null)
                piecesController.OnSelectingPiece(hitPos);

            if (Input.GetMouseButtonUp(0) && piecesController.currentlySelectingPiece != null)
                piecesController.OnBreakSelectingPiece(hitPos);
            #endregion
        }
        else
        {
            if (curHoverTile != -Vector2Int.one)
            {
                tilesController.tiles[curHoverTile.x, curHoverTile.y].gameObject.layer = piecesController.ContainsValidMove(ref piecesController.availableMoves, curHoverTile) ?
                    LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");

                curHoverTile = -Vector2Int.one;
            }

            if (PlayerDeck.Instance.hand.CurrentSelectCard != null)
            {
                List<Vector2Int> availabe = PlayerDeck.Instance.hand.CurrentSelectCard.GetAvailableMoves(settings.tileCountX, settings.tileCountY, -1, -1);
                tilesController.RemoveAllHighlight();
            }

            if (Input.GetMouseButtonUp(0) && piecesController.currentlySelectingPiece)
            {
                piecesController.currentlySelectingPiece.SetPos(tilesController.GetTileCenter(piecesController.currentlySelectingPiece.currentX,
                    piecesController.currentlySelectingPiece.currentY));
                piecesController.currentlySelectingPiece = null;
                tilesController.RemoveHighlightTiles(piecesController.availableMoves);
            }
        }
    }

    #region OnGameStates
    public bool CheckWin(int movedX, int movedY)
    {
        #region CheckCol

        int typeCount = 0;
        for (int YTile = 0; YTile < match.settings.tileCountY; YTile++)
        {
            if (piecesController.pieces[movedX, YTile] == null)
            {
                typeCount = 0;
                continue;
            }

            if (match.states.isMoveOfZero && piecesController.pieces[movedX, YTile].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[movedX, YTile].team == Team.Cross)
                typeCount++;
            else
                typeCount = 0;

            if (typeCount == match.settings.piecesWinSequence)
                return true;
        }
        #endregion

        #region CheckRow

        typeCount = 0;
        for (int XTile = 0; XTile < match.settings.tileCountX; XTile++)
        {
            if (piecesController.pieces[XTile, movedY] == null)
            {
                typeCount = 0;
                continue;
            }

            if (match.states.isMoveOfZero && piecesController.pieces[XTile, movedY].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[XTile, movedY].team == Team.Cross)
                typeCount++;
            else
                typeCount = 0;

            if (typeCount == match.settings.piecesWinSequence)
                return true;
        }
        #endregion

        #region Check Diagonal DownToUp
        int xEdge = 0, yEdge = 0;
        for (int x = movedX, y = movedY; x >= 0 && y >= 0; x--, y--) // Bot left edge
        {
            if (x == 0 || y == 0)
            {
                xEdge = x;
                yEdge = y;
                break;
            }
        }
        typeCount = 0;
        for (int x = xEdge, y = yEdge; x < match.settings.tileCountX && y < match.settings.tileCountY; x++, y++)
        {
            if (piecesController.pieces[x, y] == null)
            {
                typeCount = 0;
                continue;
            }

            if (match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Cross)
                typeCount++;
            else
                typeCount = 0;

            if (typeCount == match.settings.piecesWinSequence)
                return true;
        }
        #endregion

        #region Check Diagonal UpToDown
        xEdge = 0; yEdge = 0;
        for (int x = movedX, y = movedY; x >= 0 && y < match.settings.tileCountY; x--, y++)    //Top Left Edge
        {
            if (x == 0 || y == match.settings.tileCountY - 1)
            {
                xEdge = x;
                yEdge = y;
                break;
            }
        }

        typeCount = 0;
        for (int x = xEdge, y = yEdge; x < match.settings.tileCountX && y >= 0; x++, y--)
        {
            if (piecesController.pieces[x, y] == null)
            {
                typeCount = 0;
                continue;
            }

            if (match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Cross)
                typeCount++;
            else
                typeCount = 0;

            if (typeCount == match.settings.piecesWinSequence)
                return true;
        }
        #endregion

        return false;
    }
    public void SetDefaultBoardSettings()
    {
        piecesController.currentlySelectingPiece = null;
        tilesController.RemoveHighlightTiles(piecesController.availableMoves);
        piecesController.availableMoves = new List<Vector2Int>();

        for (int x = 0; x < match.settings.tileCountX; x++)
            for (int y = 0; y < match.settings.tileCountY; y++)
            {
                if (piecesController.pieces[x, y] != null)
                {
                    Destroy(piecesController.pieces[x, y].gameObject);
                    piecesController.pieces[x, y] = null;
                }
            }
    }

    public void DestroyBoard()
    {
        isGameStart = false;
        isBoardReady = false;

        match.states.OnGameStarted -= OnGameStart;
        match.states.OnGameTied -= OnGameEnd;
        match.states.OnGameWin -= OnGameEnd;
        match.states.OnLeaveMatch -= DestroyBoard;

        SetDefaultBoardSettings();
        tilesController.DestroyTiles();
    }

    void OnGameStart()
    {
        SetDefaultBoardSettings();
        isGameStart = true;
    }

    private void OnGameEnd(int arg1, int arg2, Team team)
    {
        isGameStart = false;
    }
    #endregion
}

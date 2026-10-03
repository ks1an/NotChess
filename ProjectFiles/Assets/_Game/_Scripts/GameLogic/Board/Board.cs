using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TilesController), typeof(PiecesController), typeof(BoardLandscapeGenerator))]
public partial class Board : MonoBehaviour
{
    public static Board Instance;
    public static Action onBoardGenerated;

    public TilesController tilesController;
    public PiecesController piecesController;
    [HideInInspector] public BoardLandscapeGenerator landscapeGenerator;
    [HideInInspector] public InteractLandscapeGenerator interactLandscapeGenerator;
    [HideInInspector] public bool landOnBoardReady;

    GameController match;
    MatchSettings settings;
    GameObject modalWindow;

    Camera curCamera;
    bool isGameStart;
    bool isPlayerControl;

    bool mouseDowned, waitToSkipMouseDown;

    Vector2Int curHoverTile = -Vector2Int.one;
    int tileLayerMask;
    PlayerCardHand playerHand;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Board > 1 on scene");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        match = GameController.Instance;
        landscapeGenerator = GetComponent<BoardLandscapeGenerator>();
        interactLandscapeGenerator = GetComponent<InteractLandscapeGenerator>();
        tileLayerMask = LayerMask.GetMask("Tile");
    }

    public void GenerateBoard()
    {
        settings = GameController.Instance.matchSettings;
        modalWindow = ModalViewWindowController.Instance.modalWindow.gameObject;
        curHoverTile = -Vector2Int.one;
        curCamera = Camera.main;

        match.states.OnSetSettings += OnSetSettings;
        match.states.OnGameStarted += OnGameStart;
        match.states.OnGameTied += OnGameEnd;
        match.states.OnGameWin += OnGameEnd;
        match.states.OnLeaveMatch += DestroyBoard;

        tilesController.SetSettings();  //FirstSetSettings is tiles!
        tilesController.GenerateTiles();

        piecesController.SetSettings(); //Second

        isPlayerControl = !match.states.isDemonstrationMatchAiVsAi;
        onBoardGenerated?.Invoke();
    }

    public void GenerateLandscape()
    {
        if (match.states.isNetMatch)
        {
            if (match.netMatch.IsServer) landscapeGenerator.GenerateRandomLandscape();
        }
        else
            landscapeGenerator.GenerateRandomLandscape();
    }

    public void SetLandReady(bool b) => landOnBoardReady = b;

    void Update()
    {
        if (!isGameStart || !landOnBoardReady || !isPlayerControl)
            return;
        if (!curCamera)
        {
            curCamera = Camera.main;
            return;
        }
        if (playerHand == null) playerHand = PlayerCardHand.Instance;
        if (playerHand == null) return;

        if (Input.GetMouseButtonDown(0))
            mouseDowned = true;

        if (waitToSkipMouseDown)
        {
            if (Input.GetMouseButtonUp(0))
            {
                waitToSkipMouseDown = false;
                mouseDowned = false;
            }
            return;
        }

        if (modalWindow.activeSelf)
        {
            if (mouseDowned)
                waitToSkipMouseDown = true;
            return;
        }

        Ray ray = curCamera.ScreenPointToRay(Input.mousePosition);
        Tile hitTile = null;
        bool hasHit = false;

        if (playerHand.CurrentHoverCard == null
            && Physics.Raycast(ray, out RaycastHit info, 50, tileLayerMask))
        {
            if (info.transform.TryGetComponent(out hitTile))
                hasHit = true;
        }

        if (hasHit)
        {
            Vector2Int hitPos = hitTile.coord;
            HoverTile(hitPos);

            #region CardMove
            if (playerHand.CurrentSelectCard != null)
            {
                tilesController.SetHoverPriority(false);
                List<Vector2Int> available;

                if (!tilesController.IsDarkAccentTile(hitPos))
                    available = playerHand.CurrentSelectCard.GetAvailableMoves(
                        settings.tileCountX, settings.tileCountY, hitPos.x, hitPos.y);
                else
                {
                    available = playerHand.CurrentSelectCard.GetAvailableMoves(
                        settings.tileCountX, settings.tileCountY, -1, -1);
                    tilesController.RemoveAllHighlightExcludeCurrentOnes(available);
                    available.Clear();
                }

                tilesController.HighlighTiles(available);
                tilesController.RemoveAllHighlightExcludeCurrentOnes(available);

                if (Input.GetMouseButtonUp(0))
                {
                    tilesController.RemoveHighlightTiles(available);
                    playerHand.CurrentSelectCard.DoOnMouseUp();
                }

                return;
            }
            #endregion

            #region PutPiece

            if (playerHand.CurrentSelectCard == null && Input.GetMouseButtonUp(0) &&
            piecesController.currentlySelectingPiece == null &&
            piecesController.pieces[hitPos.x, hitPos.y] == null &&
            tilesController.tiles[hitPos.x, hitPos.y].Stats.CurrentStats.canPutOnTile)
            {
                if (match.player.IsMyTurnOrNot())
                {
                    PieceView prefId = match.player.GetLocalPlayerTeam() == Team.Zero
                        ? match.player.zeroPawnPrefab
                        : match.player.crossPawnPrefab;

                    match.states.move.TryCreateUnitOnBoard(hitPos.x, hitPos.y,
                        match.player.GetLocalPlayerTeam(), prefId);
                }

                return;
            }
            #endregion

            #region SelectPiece

            //TODO: NewInputSystem
            if (Input.GetMouseButtonDown(0) && piecesController.pieces[hitPos.x, hitPos.y] != null)
            {
                piecesController.OnSelectingPiece(hitPos);
                tilesController.SetHoverPriority(true);
            }

            if (Input.GetMouseButtonUp(0) && piecesController.currentlySelectingPiece != null)
                piecesController.OnBreakSelectingPiece(hitPos);
            #endregion
        }
        else
        {
            ClearHoverTile();

            if (playerHand.CurrentSelectCard != null)
            {
                List<Vector2Int> available = playerHand.CurrentSelectCard.GetAvailableMoves(
                    settings.tileCountX, settings.tileCountY, -1, -1);

                tilesController.RemoveAllHighlightExcludeCurrentOnes(available);

                if (Input.GetMouseButtonUp(0))
                    playerHand.CurrentSelectCard.DoOnMouseUp();
            }

            if (Input.GetMouseButtonUp(0) && piecesController.currentlySelectingPiece)
            {
                piecesController.currentlySelectingPiece.SetPos(
                    tilesController.GetTileCenter(
                        piecesController.currentlySelectingPiece.currentX,
                        piecesController.currentlySelectingPiece.currentY));

                piecesController.currentlySelectingPiece = null;
                tilesController.RemoveHighlightTiles(piecesController.availableMoves);
            }
        }
    }

    void HoverTile(Vector2Int hoverPos)
    {
        if (curHoverTile == hoverPos) return;

        curHoverTile = hoverPos;
        tilesController.SetHoverTile(hoverPos);
    }
    void ClearHoverTile()
    {
        if (curHoverTile == -Vector2Int.one) return;

        curHoverTile = -Vector2Int.one;
        tilesController.ClearHover();
    }

    #region OnGameStates
    public List<Vector2Int> CheckWin(int movedX, int movedY)
    {
        List<Vector2Int> tileOnWinLine = new();

        #region CheckCol

        int typeCount = 0;
        tileOnWinLine.Clear();
        for (int YTile = 0; YTile < match.matchSettings.tileCountY; YTile++)
        {
            if (piecesController.pieces[movedX, YTile] == null)
            {
                tileOnWinLine.Clear();
                typeCount = 0;
                continue;
            }

            tileOnWinLine.Add(tilesController.tiles[movedX, YTile].coord);
            if (match.states.isMoveOfZero && piecesController.pieces[movedX, YTile].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[movedX, YTile].team == Team.Cross)
                typeCount++;
            else
            {
                tileOnWinLine.Clear();
                typeCount = 0;
            }

            if (typeCount == match.matchSettings.piecesWinSequence)
                return tileOnWinLine;
        }
        #endregion

        #region CheckRow

        typeCount = 0;
        tileOnWinLine.Clear();
        for (int XTile = 0; XTile < match.matchSettings.tileCountX; XTile++)
        {
            if (piecesController.pieces[XTile, movedY] == null)
            {
                tileOnWinLine.Clear();
                typeCount = 0;
                continue;
            }
            tileOnWinLine.Add(tilesController.tiles[XTile, movedY].coord);
            if (match.states.isMoveOfZero && piecesController.pieces[XTile, movedY].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[XTile, movedY].team == Team.Cross)
                typeCount++;
            else
            {
                tileOnWinLine.Clear();
                typeCount = 0;
            }

            if (typeCount == match.matchSettings.piecesWinSequence)
                return tileOnWinLine;
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
        tileOnWinLine.Clear();
        typeCount = 0;
        for (int x = xEdge, y = yEdge; x < match.matchSettings.tileCountX && y < match.matchSettings.tileCountY; x++, y++)
        {
            if (piecesController.pieces[x, y] == null)
            {
                tileOnWinLine.Clear();
                typeCount = 0;
                continue;
            }
            tileOnWinLine.Add(tilesController.tiles[x, y].coord);
            if (match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Cross)
                typeCount++;
            else
            {
                tileOnWinLine.Clear();
                typeCount = 0;
            }

            if (typeCount == match.matchSettings.piecesWinSequence)
                return tileOnWinLine;
        }
        #endregion

        #region Check Diagonal UpToDown
        xEdge = 0; yEdge = 0;
        for (int x = movedX, y = movedY; x >= 0 && y < match.matchSettings.tileCountY; x--, y++)    //Top Left Edge
        {
            if (x == 0 || y == match.matchSettings.tileCountY - 1)
            {
                xEdge = x;
                yEdge = y;
                break;
            }
        }

        typeCount = 0;
        tileOnWinLine.Clear();
        for (int x = xEdge, y = yEdge; x < match.matchSettings.tileCountX && y >= 0; x++, y--)
        {
            if (piecesController.pieces[x, y] == null)
            {
                tileOnWinLine.Clear();
                typeCount = 0;
                continue;
            }

            tileOnWinLine.Add(tilesController.tiles[x, y].coord);
            if (match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Zero)
                typeCount++;
            else if (!match.states.isMoveOfZero && piecesController.pieces[x, y].team == Team.Cross)
                typeCount++;
            else
            {
                tileOnWinLine.Clear();
                typeCount = 0;
            }

            if (typeCount == match.matchSettings.piecesWinSequence)
                return tileOnWinLine;
        }
        #endregion

        tileOnWinLine.Clear();
        return tileOnWinLine;
    }
    void SetDefaultBoardSettings()
    {
        piecesController.currentlySelectingPiece = null;
        tilesController.SetHoverPriority(false);
        tilesController.RemoveAllHighlight();
        tilesController.RemoveAllAccent();
        piecesController.availableMoves = new List<Vector2Int>();

        for (int x = 0; x < match.matchSettings.tileCountX; x++)
            for (int y = 0; y < match.matchSettings.tileCountY; y++)
            {
                if (piecesController.pieces[x, y] != null)
                {
                    Destroy(piecesController.pieces[x, y].view.gameObject);
                    piecesController.pieces[x, y] = null;
                }
            }
    }

    public void DestroyBoard()
    {
        isGameStart = false;
        SetLandReady(false);

        match.states.OnSetSettings -= OnSetSettings;
        match.states.OnGameStarted -= OnGameStart;
        match.states.OnGameTied -= OnGameEnd;
        match.states.OnGameWin -= OnGameEnd;
        match.states.OnLeaveMatch -= DestroyBoard;

        SetDefaultBoardSettings();
        tilesController.DestroyTiles();
    }

    void OnSetSettings() => SetDefaultBoardSettings();
    void OnGameStart()
    {
        if (mouseDowned)
            waitToSkipMouseDown = true;
        isGameStart = true;
    }

    void OnGameEnd(int arg1, int arg2, Team team)
    {
        isGameStart = false;
    }
    #endregion
}

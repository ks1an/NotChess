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

    [SerializeField] LayerMask interactableLayerMask;


    GameController match;
    MatchSettings settings;
    GameObject modalWindow;

    Camera curCamera;
    bool isGameStart;
    bool isPlayerControl;

    Vector2Int curHoverTile = -Vector2Int.one;
    List<Vector2Int> trackedHoverHighlights = new();
    int tileLayerMask;
    PlayerCardHand playerHand;
    InputService input;
    bool pieceSelectedByClick;
    bool lastModalActive;
    bool skipNextTap;

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

    void OnEnable()
    {
        if (input == null)
        {
            input = InputService.Instance;
            if (input != null)
            {
                input.OnTap += HandleTap;
                input.OnDragStart += HandleDragStart;
                input.OnDragEnd += HandleDragEnd;
            }
            else Debug.LogError("InputService is null!");
        }
    }

    void OnDisable()
    {
        if (input == null) return;
        input.OnTap -= HandleTap;
        input.OnDragStart -= HandleDragStart;
        input.OnDragEnd -= HandleDragEnd;
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
        if (match.states.gamemode == MatchStates.GameMode_Tutorial)
        {
            SetLandReady(true);
            return;
        }
        landscapeGenerator.GenerateRandomLandscape();
    }

    public void SetLandReady(bool b) => landOnBoardReady = b;

    void Update()
    {
        if (!isGameStart || !landOnBoardReady || !isPlayerControl) return;
        if (!curCamera) { curCamera = Camera.main; return; }
        if (playerHand == null) playerHand = PlayerCardHand.Instance;
        if (playerHand == null) return;
        if (modalWindow != null && modalWindow.activeSelf) return;

        bool modalActive = modalWindow != null && modalWindow.activeSelf;
        if (lastModalActive && !modalActive)
            skipNextTap = true;
        lastModalActive = modalActive;

        if (modalActive)
        {
            ClearHoverTile();
            ClearTrackedHoverHighlights();
            return;
        }

        if (input.IsPointerOverUI)
        {
            ClearHoverTile();
            ClearTrackedHoverHighlights();
            return;
        }

        Ray ray = curCamera.ScreenPointToRay(input.PointerPosition);
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
            HoverTile(hitTile.coord);
            UpdateSelectionHover(hitTile.coord);
        }
        else
        {
            ClearHoverTile();
            ClearTrackedHoverHighlights();
        }
    }

    #region Input
    void HandleTap(Vector2 screenPos)
    {
        if (skipNextTap) { skipNextTap = false; return; }
        if (!IsInputAllowed()) return;

        // Клик по UI (карты в руке, кнопки) обрабатывается через компоненты UI (например, Card.OnPointerDown).
        // Не мешаем выбору карты на тапе.
        if (input.IsPointerOverUI) return;

        if (TryRaycastTile(screenPos, out Vector2Int hitPos))
        {
            if (TutorialController.Instance != null
                && TutorialController.Instance.IsActive
                && !TutorialController.Instance.TryTapTile(hitPos))
            {
                TutorialController.Instance.NotifyWrongTarget(hitPos);
                TryResetSelection();
                return;
            }

            if (playerHand.CurrentSelectCard != null)
            {
                playerHand.CurrentSelectCard.DoOnMouseUp();
                if (TutorialController.Instance != null) TutorialController.Instance.NotifyCardUsed();
                return;
            }

            HandleTapOnPieceOrTile(hitPos);
            return;
        }

        if (IsTapOnInteractiveObject(screenPos))
        {
            if (playerHand.CurrentSelectCard != null)
            {
                if (playerHand.CurrentSelectCard.TargetCategory == TargetCategory.EnemyHand)
                {
                    playerHand.CurrentSelectCard.DoOnMouseUp();
                    if (TutorialController.Instance != null)
                        TutorialController.Instance.NotifyCardUsed();
                }
                else
                {
                    TryResetSelection();
                }
                return;
            }

            TryResetSelection();
            return;
        }

        TryResetSelection();
    }

    void HandleDragStart(Vector2 screenPos)
    {
        if (!IsInputAllowed()) return;
        if (input.IsPointerOverUI) return;

        if (TutorialController.Instance != null && TutorialController.Instance.IsActive)
        {
            if (!TutorialController.Instance.IsDragAllowed())
                return;
        }

        if (!TryRaycastTile(screenPos, out Vector2Int hitPos)) return;
        if (playerHand.CurrentSelectCard != null) return;

        if (pieceSelectedByClick && piecesController.currentlySelectingPiece != null)
        {
            var cur = piecesController.currentlySelectingPiece;
            if (cur.currentX != hitPos.x || cur.currentY != hitPos.y)
                ResetPersistentSelection();
        }

        if (piecesController.currentlySelectingPiece == null
            && piecesController.pieces[hitPos.x, hitPos.y] != null)
        {
            piecesController.OnSelectingPiece(hitPos);
            tilesController.SetHoverPriority(true);
        }
    }

    void HandleDragEnd(Vector2 screenPos)
    {
        if (!IsInputAllowed()) return;

        // При завершении драга (отпускании) над UI сбрасываем выделение,
        // чтобы юниты или карты не зависали в воздухе.
        if (input.IsPointerOverUI)
        {
            TryResetSelection();
            return;
        }

        if (TryRaycastTile(screenPos, out Vector2Int hitPos))
        {
            if (TutorialController.Instance != null
                && TutorialController.Instance.IsActive
                && !TutorialController.Instance.TryTapTile(hitPos))
            {
                TutorialController.Instance.NotifyWrongTarget(hitPos);
                TryResetSelection();
                return;
            }

            if (playerHand.CurrentSelectCard != null)
            {
                playerHand.CurrentSelectCard.DoOnMouseUp();
                if (TutorialController.Instance != null) TutorialController.Instance.NotifyCardUsed();
                return;
            }

            if (piecesController.currentlySelectingPiece != null)
            {
                piecesController.OnBreakSelectingPiece(hitPos);
                pieceSelectedByClick = false;
                tilesController.SetHoverPriority(false);
            }
            return;
        }

        if (IsTapOnInteractiveObject(screenPos))
        {
            if (playerHand.CurrentSelectCard != null)
            {
                if (playerHand.CurrentSelectCard.TargetCategory == TargetCategory.EnemyHand)
                {
                    playerHand.CurrentSelectCard.DoOnMouseUp();
                    if (TutorialController.Instance != null)
                        TutorialController.Instance.NotifyCardUsed();
                }
                else
                {
                    TryResetSelection();
                }
                return;
            }

            TryResetSelection();
            return;
        }

        TryResetSelection();
    }

    #region Helpers
    bool IsTapOnInteractiveObject(Vector2 screenPos)
    {
        Ray ray = curCamera.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, 50, interactableLayerMask);
    }
    bool IsInputAllowed()
    {
        if (!isGameStart || !landOnBoardReady || !isPlayerControl) return false;
        if (!curCamera) return false;
        if (modalWindow != null && modalWindow.activeSelf) return false;
        if (playerHand == null) playerHand = PlayerCardHand.Instance;
        if (playerHand == null) return false;
        return true;
    }

    bool TryRaycastTile(Vector2 screenPos, out Vector2Int coord)
    {
        coord = -Vector2Int.one;
        Ray ray = curCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit info, 50, tileLayerMask)) return false;
        if (!info.transform.TryGetComponent(out Tile tile)) return false;
        coord = tile.coord;
        return true;
    }

    void HandleTapOnPieceOrTile(Vector2Int hitPos)
    {
        bool pieceHere = piecesController.pieces[hitPos.x, hitPos.y] != null;

        if (pieceSelectedByClick && piecesController.currentlySelectingPiece != null)
        {
            var cur = piecesController.currentlySelectingPiece;

            if (cur.currentX == hitPos.x && cur.currentY == hitPos.y)
            {
                ResetPersistentSelection();
                return;
            }

            piecesController.OnBreakSelectingPiece(hitPos);
            pieceSelectedByClick = false;
            tilesController.SetHoverPriority(false);
            if (TutorialController.Instance != null) TutorialController.Instance.NotifyPlayerCapturedPiece(hitPos);
            return;
        }

        if (pieceHere && match.player.IsMyTurnOrNot())
        {
            piecesController.OnSelectingPiece(hitPos);
            tilesController.SetHoverPriority(true);
            pieceSelectedByClick = true;
            if(TutorialController.Instance != null) TutorialController.Instance.NotifyPlayerSelectedPiece(hitPos);
            return;
        }

        if (!pieceHere
            && piecesController.currentlySelectingPiece == null
            && tilesController.tiles[hitPos.x, hitPos.y].Stats.CurrentStats.canPutOnTile)
        {
            if (match.player.IsMyTurnOrNot())
            {
                PieceView prefId = match.player.GetTeam() == Team.Zero
                    ? match.player.zeroPawnPrefab
                    : match.player.crossPawnPrefab;

                match.states.move.TryCreateUnitOnBoard(
                    hitPos.x, hitPos.y,
                    match.player.GetTeam(), prefId);

                if (TutorialController.Instance != null)
                    TutorialController.Instance.NotifyPlayerPlacedPiece(hitPos);
            }
        }
    }

    void ResetPersistentSelection()
    {
        if (piecesController.currentlySelectingPiece == null)
        {
            pieceSelectedByClick = false;
            return;
        }

        var cur = piecesController.currentlySelectingPiece;
        cur.SetPos(tilesController.GetTileCenter(cur.currentX, cur.currentY));
        piecesController.currentlySelectingPiece = null;
        pieceSelectedByClick = false;
        tilesController.SetHoverPriority(false);
        tilesController.RemoveHighlightTiles(piecesController.availableMoves);
    }

    void TryResetSelection()
    {
        if (TutorialController.Instance != null
            && TutorialController.Instance.ShouldBlockSelectionReset())
            return;  

        ResetAnySelection();
    }

    void ResetAnySelection()
    {
        if (playerHand.CurrentSelectCard != null)
            playerHand.CurrentSelectCard.DoOnMouseUp(withoutAction: true);

        if (piecesController.currentlySelectingPiece != null)
            ResetPersistentSelection();
    }

    public void ClearPieceSelection()
    {
        if (piecesController.currentlySelectingPiece != null)
            ResetPersistentSelection();
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

    void UpdateSelectionHover(Vector2Int hitPos)
    {
        ClearTrackedHoverHighlights();

        if (playerHand.CurrentSelectCard == null) return;

        tilesController.SetHoverPriority(false);

        List<Vector2Int> available;
        if (!tilesController.IsDarkAccentTile(hitPos))
            available = playerHand.CurrentSelectCard.GetAvailableMoves(
                settings.tileCountX, settings.tileCountY, hitPos.x, hitPos.y);
        else
        {
            available = playerHand.CurrentSelectCard.GetAvailableMoves(
                settings.tileCountX, settings.tileCountY, -1, -1);
            available.Clear();
        }

        if (available.Count == 0) return;

        tilesController.HighlighTiles(available);
        trackedHoverHighlights = new List<Vector2Int>(available);
    }

    void ClearTrackedHoverHighlights()
    {
        if (trackedHoverHighlights.Count == 0) return;

        tilesController.RemoveHighlightTiles(trackedHoverHighlights);
        trackedHoverHighlights.Clear();
    }
    #endregion
    #endregion

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
        tilesController.ClearLastMoveHighlight();
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
        landscapeGenerator.DestroyLandscapes();
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
        isGameStart = true;
        skipNextTap = false;
        pieceSelectedByClick = false;
    }

    void OnGameEnd(int arg1, int arg2, Team team)
    {
        isGameStart = false;
    }
    #endregion
}

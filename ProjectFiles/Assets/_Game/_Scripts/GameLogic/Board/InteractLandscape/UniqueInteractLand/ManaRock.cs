using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class ManaRock : InteractLandscapeObject
{
    [SerializeField] int turnsForFirstSpawn, turnsSpawnPeriodAfterFirstSpawn;
    [SerializeField, Range(0, 100)] float chanceForSpawnOnPeriod, chanceForFirstSpawn;
    [SerializeField] int giveManaIfCaptureOnesTeam;
    [field: SerializeField] InstanceGameobject_SO_VB visualEffectOnCenterTile;

    List<Vector2Int> myWorkZone;
    Vector2Int center;
    int patternIndex;

    public override void Init(int tileX, int tileY, List<Vector2Int> workzone, int pattern)
    {
        base.Init(tileX, tileY, workzone, pattern);
        center = new(tileX, tileY);
        myWorkZone = workzone;
        Board.Instance.tilesController.AccentTiles(workzone, true);
        Tile centerTile = Board.Instance.tilesController.tiles[tileX, tileY];
        var dBuff = new DefferedActionBuff(centerTile.Stats, turnsWaiting, DoAfterWaitTurns, isCriticalBuff: true);
        centerTile.Stats.AddBuff(dBuff);

        foreach (Vector2Int coordTile in myWorkZone)
        {
            Tile tile = Board.Instance.tilesController.tiles[coordTile.x, coordTile.y];

            var putBuff = new BanPut_TileBuff(false, true);
            var tBuff = new TemporaryBuff(tile.Stats, putBuff, turnsWaiting, true);
            tile.Stats.AddBuff(tBuff);
        }

        patternIndex = pattern;
    }

    private void DoAfterWaitTurns(IBuffable buffable)
    {
        Board.Instance.tilesController.RemoveAccentTiles(myWorkZone, true);
        LandscapeVectorsPattern pattern = patterns[patternIndex];
        myWorkZone.Clear();
        foreach (var pos in pattern.positionsFromCenter)
            myWorkZone.Add(center + pos);
        Board.Instance.tilesController.AccentTiles(myWorkZone, false);

        Tile centerOfAction = Board.Instance.tilesController.tiles[center.x, center.y];

        List<IBuff> buffsOnTile = new()
                {
                    new BanPut_TileBuff(false, true),
                    new DefferedActionBuff(centerOfAction.Stats, turnsLiving,
            DoAfterLiving, isCriticalBuff: true)
                };
        var buffsOnTilePocket = new PocketBuff(false, false, buffsOnTile, true);
        var tBuff = new TemporaryBuff(centerOfAction.Stats, buffsOnTilePocket, turnsLiving, true);
        new VisualGameobjectBuffBehaviour(tBuff, visualEffectOnCenterTile, centerOfAction.tileCenter);
        centerOfAction.Stats.AddBuff(tBuff);
    }

    private void DoAfterLiving(IBuffable buffable)
    {
        int amount = 0;
        int playerTeamAmount = 0;
        foreach (Vector2Int coordTile in myWorkZone)
            if (Board.Instance.piecesController.pieces[coordTile.x, coordTile.y] != null)
            {
                amount++;
                if (Board.Instance.piecesController.pieces[coordTile.x, coordTile.y].team ==
                    GameController.Instance.player.GetLocalPlayerTeam())
                    playerTeamAmount++;
            }

        if (amount == myWorkZone.Count)
        {
            if (amount == playerTeamAmount)
                GameController.Instance.player.IncreaseMana(giveManaIfCaptureOnesTeam);
            else
                GameController.Instance.player.IncreaseMana((int)(giveManaIfCaptureOnesTeam / 2));
        }

        Board.Instance.tilesController.RemoveAccentTiles(myWorkZone, false);
    }

    #region ChecksConditions

    public override bool CheckConditionOfExistence()
    {
        if ((GameController.Instance.states.turnCount == turnsForFirstSpawn &&
            MathOperations.GetInstance().GetSafeRandom(0, 100) <= chanceForFirstSpawn) ||

            (GameController.Instance.states.turnCount % (turnsForFirstSpawn + turnsSpawnPeriodAfterFirstSpawn) == 0 &&
            MathOperations.GetInstance().GetSafeRandom(0, 100) <= chanceForSpawnOnPeriod)

            )
            return true;

        return false;
    }

    public override List<Vector2Int> CheckConditionOfPlacing(int tileX, int tileY, List<Vector2Int> freeTiles)
    {
        Vector2Int workspace = new(tileX, tileY);
        myWorkZone = new();

        #region CheckBorder
        workspace.x = tileX - 2;
        for (int i = -2; i <= 2; i++)
        {
            workspace.y = tileY + i;
            if (workspace.x > 0 && workspace.x < GameController.Instance.matchSettings.tileCountX &&
                workspace.y > 0 && workspace.y < GameController.Instance.matchSettings.tileCountY &&
                Board.Instance.piecesController.pieces[workspace.x, workspace.y] != null) 
                return null;
        }

        workspace.x = tileX + 2;
        for (int i = -2; i <= 2; i++)
        {
            workspace.y = tileY + i;
            if (workspace.x > 0 && workspace.x < GameController.Instance.matchSettings.tileCountX &&
                workspace.y > 0 && workspace.y < GameController.Instance.matchSettings.tileCountY &&
                Board.Instance.piecesController.pieces[workspace.x, workspace.y] != null)
                return null;
        }

        workspace.y = tileY - 2;
        for (int i = -2; i <= 2; i++)
        {
            workspace.x = tileX + i;
            if (workspace.x > 0 && workspace.x < GameController.Instance.matchSettings.tileCountX &&
                workspace.y > 0 && workspace.y < GameController.Instance.matchSettings.tileCountY &&
                Board.Instance.piecesController.pieces[workspace.x, workspace.y] != null)
                return null;
        }

        workspace.y = tileY + 2;
        for (int i = -2; i <= 2; i++)
        {
            workspace.x = tileX + i;
            if (workspace.x > 0 && workspace.x < GameController.Instance.matchSettings.tileCountX &&
                workspace.y > 0 && workspace.y < GameController.Instance.matchSettings.tileCountY &&
                Board.Instance.piecesController.pieces[workspace.x, workspace.y] != null)
                return null;
        }
        #endregion

        #region getWorkzone
        workspace.x = tileX - 1;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = tileY + i;
            if (!freeTiles.Contains(workspace)) return null;
            else myWorkZone.Add(workspace);
        }

        workspace.x = tileX + 1;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = tileY + i;
            if (!freeTiles.Contains(workspace)) return null;
            else myWorkZone.Add(workspace);
        }

        workspace.x = tileX;
        for (int i = -1; i < 2; i++)
        {
            workspace.y = tileY + i;
            if (!freeTiles.Contains(workspace)) return null;
            else myWorkZone.Add(workspace);
        }
        #endregion

        //Debug.LogWarning($"TRUE GENERATE INTERACT LANDSCAPE. MY CENTER: {tileX}, {tileY}");
        return myWorkZone;
    }

    #endregion

    void DestroyObject() => Destroy(gameObject);

    private void OnEnable()
    {
        GameController.Instance.states.OnGameStarted += DestroyObject;
    }
    private void OnDisable()
    {
        GameController.Instance.states.OnGameStarted -= DestroyObject;
    }
}

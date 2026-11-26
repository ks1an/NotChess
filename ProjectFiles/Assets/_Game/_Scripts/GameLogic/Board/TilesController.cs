using System.Collections.Generic;
using UnityEngine;

public sealed class TilesController : MonoBehaviour
{
    [SerializeField] Mesh tileMesh;
    [SerializeField] Color highlightColor, darkAccentColor, lightAccentColor, hoverColor;

    [Header("Last Move Highlight Settings")]
    [SerializeField] private Color putHighlightColor = new Color32(0x85, 0xB0, 0x9A, 0xFF);
    [SerializeField] private Color attackSourceColor = new Color32(0xD4, 0x83, 0x6A, 0xFF);
    [SerializeField] private Color attackTargetColor = new Color32(0xC9, 0x4A, 0x4A, 0xFF);

    private Vector2Int lastPutCoord = -Vector2Int.one;
    private Vector2Int lastMoveSource = -Vector2Int.one;
    private Vector2Int lastMoveTarget = -Vector2Int.one;

    public Tile[,] tiles;

    GameController match;
    int tileCountX;
    int tileCountY;
    float tileSize;
    Transform tileContainer;

    private readonly HashSet<Vector2Int> highlightTiles = new();
    private readonly HashSet<Vector2Int> darkAccentTiles = new();
    private readonly HashSet<Vector2Int> lightAccentTiles = new();

    Vector3 offset;
    Vector2Int hoverCoord = -Vector2Int.one;
    bool hoverOverHighlight = false;

    private static readonly int BaseEmissionId = Shader.PropertyToID("_BaseEmission");
    private static readonly int UVOffsetSeedId = Shader.PropertyToID("_UVOffsetSeed");
    private MaterialPropertyBlock sharedPropertyBlock;

    public void SetSettings()
    {
        match = GameController.Instance;

        tileCountX = match.matchSettings.tileCountX;
        tileCountY = match.matchSettings.tileCountY;
        tileSize = match.matchSettings.tileSize;
        offset = Vector3.zero;

        sharedPropertyBlock ??= new MaterialPropertyBlock();
    }

    #region GenerateAndDestroy
    public void DestroyTiles()
    {
        if (tiles != null)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] != null && tiles[x, y].gameObject != null)
                        Destroy(tiles[x, y].gameObject);
                }
            }
            tiles = null;
        }

        highlightTiles.Clear();
        darkAccentTiles.Clear();
        lightAccentTiles.Clear();
    }

    public void GenerateTiles()
    {
        tileContainer = new GameObject("Tiles container").transform;
        tiles = new Tile[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                Material mat = ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
                    ? match.player.tileFirstMaterial
                    : match.player.tileSecondMaterial;

                tiles[x, y] = GenerateTile(tileSize, x, y, mat);
            }
        }
    }

    Tile GenerateTile(float size, int x, int y, Material material)
    {
        GameObject tileObject = new($"X: {x}, Y: {y}");
        tileObject.transform.parent = tileContainer;
        tileObject.layer = LayerMask.NameToLayer("Tile");
        Tile tile = tileObject.AddComponent<Tile>();

        MeshFilter meshFilter = tileObject.AddComponent<MeshFilter>();
        meshFilter.mesh = tileMesh;

        MeshRenderer renderer = tileObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        tile.render = renderer;

        tileObject.AddComponent<BoxCollider>();

        tileObject.transform.localScale *= size;
        Vector3 scaledSize = Vector3.Scale(tileMesh.bounds.size, tileObject.transform.lossyScale);
        if (x == 0 && y == 0)
            offset = scaledSize;

        tileObject.transform.position = new Vector3((x + offset.x / 2) * size, -0.75f,
            (y + offset.z / 2) * size);

        tile.tileCenter = tileObject.transform.position +
            new Vector3(-scaledSize.x / 2, scaledSize.y, -scaledSize.z / 2);
        tile.coord = new Vector2Int(x, y);
        tile.uvOffsetSeed = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));

        ApplyTileVisual(tile);
        return tile;
    }
    #endregion

    #region Get
    public Vector2Int GetTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if (tiles[x, y] != null && tiles[x, y].gameObject == hitInfo)
                    return new Vector2Int(x, y);
            }
        }

        return -Vector2Int.one;
    }

    public Vector3 GetTileCenter(int x, int y)
    {
        return tiles[x, y].tileCenter;
    }
    #endregion

    public void SetHoverPriority(bool overHighlight)
    {
        if (hoverOverHighlight == overHighlight) return;
        hoverOverHighlight = overHighlight;

        if (hoverCoord != -Vector2Int.one)
            ApplyTileVisual(tiles[hoverCoord.x, hoverCoord.y]);
    }

    private void ApplyTileVisual(Tile tile)
    {
        Vector2Int coord = tile.coord;
        sharedPropertyBlock.Clear();
        sharedPropertyBlock.SetVector(UVOffsetSeedId, tile.uvOffsetSeed);

        bool isHighlight = highlightTiles.Contains(coord);
        bool isHover = hoverCoord == coord;

        if (isHover && (hoverOverHighlight || !isHighlight))
            sharedPropertyBlock.SetColor(BaseEmissionId, hoverColor);
        else if (isHighlight)
            sharedPropertyBlock.SetColor(BaseEmissionId, highlightColor);
        else if (coord == lastPutCoord)
            sharedPropertyBlock.SetColor(BaseEmissionId, putHighlightColor);
        else if (coord == lastMoveSource)
            sharedPropertyBlock.SetColor(BaseEmissionId, attackSourceColor);
        else if (coord == lastMoveTarget)
            sharedPropertyBlock.SetColor(BaseEmissionId, attackTargetColor);
        else if (darkAccentTiles.Contains(coord))
            sharedPropertyBlock.SetColor(BaseEmissionId, darkAccentColor);
        else if (lightAccentTiles.Contains(coord))
            sharedPropertyBlock.SetColor(BaseEmissionId, lightAccentColor);

        tile.render.SetPropertyBlock(sharedPropertyBlock);
    }

    public void SetHoverTile(Vector2Int coord)
    {
        if (hoverCoord == coord) return;

        Vector2Int oldHover = hoverCoord;
        hoverCoord = coord;

        if (oldHover != -Vector2Int.one)
            ApplyTileVisual(tiles[oldHover.x, oldHover.y]);

        if (coord != -Vector2Int.one)
            ApplyTileVisual(tiles[coord.x, coord.y]);
    }

    public void ClearHover() => SetHoverTile(-Vector2Int.one);

    #region HighLight
    public bool IsHighlighTile(Vector2Int tile) { return highlightTiles.Contains(tile); }
    public bool IsDarkAccentTile(Vector2Int tile) { return darkAccentTiles.Contains(tile); }
    public bool IsLightAccentTile(Vector2Int tile) { return lightAccentTiles.Contains(tile); }

    public void HighlighTiles(List<Vector2Int> availableMoves)
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            var coord = availableMoves[i];
            if (!highlightTiles.Add(coord)) continue;

            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }

    public void RemoveHighlightTiles(List<Vector2Int> needRemoveHighlightTiles)
    {
        for (int i = 0; i < needRemoveHighlightTiles.Count; i++)
        {
            var coord = needRemoveHighlightTiles[i];
            if (!highlightTiles.Remove(coord)) continue;

            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }

    public void RemoveAllHighlightExcludeCurrentOnes(List<Vector2Int> currentHighlightTiles)
    {
        List<Vector2Int> toRemove = null;
        foreach (var coord in highlightTiles)
        {
            if (!currentHighlightTiles.Contains(coord))
            {
                toRemove ??= new List<Vector2Int>();
                toRemove.Add(coord);
            }
        }

        if (toRemove != null)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                highlightTiles.Remove(toRemove[i]);
                ApplyTileVisual(tiles[toRemove[i].x, toRemove[i].y]);
            }
        }
    }

    public void RemoveAllHighlight()
    {
        if (highlightTiles.Count == 0) return;

        var temp = new List<Vector2Int>(highlightTiles);
        highlightTiles.Clear();

        for (int i = 0; i < temp.Count; i++)
        {
            ApplyTileVisual(tiles[temp[i].x, temp[i].y]);
        }
    }
    #endregion

    #region Accent
    public void AccentTiles(List<Vector2Int> availableMoves, bool dark)
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            var coord = availableMoves[i];
            if (dark)
            {
                if (!darkAccentTiles.Add(coord)) continue;
            }
            else
            {
                if (!lightAccentTiles.Add(coord)) continue;
            }

            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }

    public void RemoveAccentTiles(List<Vector2Int> needRemoveAccentTiles, bool dark)
    {
        for (int i = 0; i < needRemoveAccentTiles.Count; i++)
        {
            var coord = needRemoveAccentTiles[i];
            if (dark)
            {
                if (!darkAccentTiles.Remove(coord)) continue;
            }
            else
            {
                if (!lightAccentTiles.Remove(coord)) continue;
            }

            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }

    public void RemoveAllAccent()
    {
        bool hasAny = darkAccentTiles.Count > 0 || lightAccentTiles.Count > 0;
        if (!hasAny) return;

        var darkTemp = new List<Vector2Int>(darkAccentTiles);
        var lightTemp = new List<Vector2Int>(lightAccentTiles);
        darkAccentTiles.Clear();
        lightAccentTiles.Clear();

        for (int i = 0; i < darkTemp.Count; i++)
        {
            ApplyTileVisual(tiles[darkTemp[i].x, darkTemp[i].y]);
        }
        for (int i = 0; i < lightTemp.Count; i++)
        {
            ApplyTileVisual(tiles[lightTemp[i].x, lightTemp[i].y]);
        }
    }
    #endregion

    #region Last Move Highlight
    public void HighlightPut(Vector2Int coord)
    {
        ClearLastMoveHighlight();
        lastPutCoord = coord;
        ApplyTileVisual(tiles[coord.x, coord.y]);
    }

    public void HighlightMove(Vector2Int from, Vector2Int to, bool isAttack)
    {
        ClearLastMoveHighlight();
        lastMoveSource = from;
        lastMoveTarget = to;
        ApplyTileVisual(tiles[from.x, from.y]);
        ApplyTileVisual(tiles[to.x, to.y]);
    }

    public void ClearLastMoveHighlight()
    {
        if (lastPutCoord != -Vector2Int.one)
        {
            var t = tiles[lastPutCoord.x, lastPutCoord.y];
            lastPutCoord = -Vector2Int.one;
            ApplyTileVisual(t);
        }
        if (lastMoveSource != -Vector2Int.one)
        {
            var tFrom = tiles[lastMoveSource.x, lastMoveSource.y];
            lastMoveSource = -Vector2Int.one;
            ApplyTileVisual(tFrom);
        }
        if (lastMoveTarget != -Vector2Int.one)
        {
            var tTarget = tiles[lastMoveTarget.x, lastMoveTarget.y];
            lastMoveTarget = -Vector2Int.one;
            ApplyTileVisual(tTarget);
        }
    }
    #endregion
}
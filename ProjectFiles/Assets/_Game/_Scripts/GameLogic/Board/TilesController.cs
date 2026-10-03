using System.Collections.Generic;
using UnityEngine;

public sealed class TilesController : MonoBehaviour
{
    [SerializeField] Mesh tileMesh;
    [SerializeField] Color highlightColor,
        darkAccentColor, lightAccentColor, hoverColor;

    public Tile[,] tiles;

    GameController match;
    int tileCountX;
    int tileCountY;
    float tileSize;
    Transform tileContainer;
    List<Vector2Int> highlightTiles = new();
    List<Vector2Int> darkAccentTiles = new();
    List<Vector2Int> lightAccentTiles = new();
    Vector3 offset;
    Vector2Int hoverCoord = -Vector2Int.one;
    bool hoverOverHighlight = false;

    MaterialPropertyBlock highlightBlock;
    MaterialPropertyBlock darkAccentBlock;
    MaterialPropertyBlock lightAccentBlock;
    MaterialPropertyBlock hoverBlock;

    public void SetSettings()
    {
        match = GameController.Instance;

        tileCountX = match.matchSettings.tileCountX;
        tileCountY = match.matchSettings.tileCountY;
        tileSize = match.matchSettings.tileSize;
        offset = new();

        highlightBlock = new MaterialPropertyBlock();
        highlightBlock.SetColor("_BaseEmission", highlightColor);

        darkAccentBlock = new MaterialPropertyBlock();
        darkAccentBlock.SetColor("_BaseEmission", darkAccentColor);

        lightAccentBlock = new MaterialPropertyBlock();
        lightAccentBlock.SetColor("_BaseEmission", lightAccentColor);

        hoverBlock = new MaterialPropertyBlock();
        hoverBlock.SetColor("_BaseEmission", hoverColor);
    }

    #region GenerateAndDestroy
    public void DestroyTiles()
    {
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                Destroy(tiles[x, y].gameObject);
        Destroy(tileContainer.gameObject);
    }

    public void GenerateTiles()
    {
        tileContainer = new GameObject(string.Format($"Tiles container")).transform;
        tiles = new Tile[tileCountX, tileCountY];

        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
                    tiles[x, y] = GenerateTile(tileSize, x, y, match.player.tileFirstMaterial);
                else
                    tiles[x, y] = GenerateTile(tileSize, x, y, match.player.tileSecondMaterial);
    }

    Tile GenerateTile(float size, int x, int y, Material material)
    {
        GameObject tileObject = new(string.Format($"X: {x}, Y: {y}"));
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

        return tile;
    }
    #endregion

    #region Get
    public Vector2Int GetTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
            {
                if (tiles[x, y].gameObject == hitInfo)
                    return new Vector2Int(x, y);
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
        bool isHighlight = highlightTiles.Contains(tile.coord);
        bool isHover = hoverCoord == tile.coord;

        if (isHover && (hoverOverHighlight || !isHighlight))
            tile.render.SetPropertyBlock(hoverBlock);
        else if (isHighlight)
            tile.render.SetPropertyBlock(highlightBlock);
        else if (darkAccentTiles.Contains(tile.coord))
            tile.render.SetPropertyBlock(darkAccentBlock);
        else if (lightAccentTiles.Contains(tile.coord))
            tile.render.SetPropertyBlock(lightAccentBlock);
        else
            tile.render.SetPropertyBlock(null);
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
            var tile = tiles[availableMoves[i].x, availableMoves[i].y];
            if (highlightTiles.Contains(tile.coord)) continue;

            highlightTiles.Add(tile.coord);
            ApplyTileVisual(tile);
        }
    }

    public void RemoveHighlightTiles(List<Vector2Int> needRemoveHighlightTiles)
    {
        for (int i = 0; i < needRemoveHighlightTiles.Count; i++)
        {
            var tile = tiles[needRemoveHighlightTiles[i].x, needRemoveHighlightTiles[i].y];
            if (!highlightTiles.Contains(tile.coord)) continue;

            highlightTiles.Remove(tile.coord);
            ApplyTileVisual(tile);
        }
    }

    public void RemoveAllHighlightExcludeCurrentOnes(List<Vector2Int> currentHighlightTiles)
    {
        for (int i = highlightTiles.Count - 1; i >= 0; i--)
        {
            var coord = highlightTiles[i];
            if (!currentHighlightTiles.Contains(coord))
            {
                highlightTiles.RemoveAt(i);
                ApplyTileVisual(tiles[coord.x, coord.y]);
            }
        }
    }

    public void RemoveAllHighlight()
    {
        for (int i = highlightTiles.Count - 1; i >= 0; i--)
        {
            var coord = highlightTiles[i];
            highlightTiles.RemoveAt(i);
            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }
    #endregion

    #region Accent

    public void AccentTiles(List<Vector2Int> availableMoves, bool dark)
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            var tile = tiles[availableMoves[i].x, availableMoves[i].y];

            if (dark)
            {
                if (darkAccentTiles.Contains(tile.coord)) continue;
                darkAccentTiles.Add(tile.coord);
            }
            else
            {
                if (lightAccentTiles.Contains(tile.coord)) continue;
                lightAccentTiles.Add(tile.coord);
            }

            ApplyTileVisual(tile);
        }
    }

    public void RemoveAccentTiles(List<Vector2Int> needRemoveAccentTiles, bool dark)
    {
        for (int i = 0; i < needRemoveAccentTiles.Count; i++)
        {
            var tile = tiles[needRemoveAccentTiles[i].x, needRemoveAccentTiles[i].y];

            if (dark)
            {
                if (!darkAccentTiles.Contains(tile.coord)) continue;
                darkAccentTiles.Remove(tile.coord);
            }
            else
            {
                if (!lightAccentTiles.Contains(tile.coord)) continue;
                lightAccentTiles.Remove(tile.coord);
            }

            ApplyTileVisual(tile);
        }
    }

    public void RemoveAllAccent()
    {
        for (int i = darkAccentTiles.Count - 1; i >= 0; i--)
        {
            var coord = darkAccentTiles[i];
            darkAccentTiles.RemoveAt(i);
            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
        for (int i = lightAccentTiles.Count - 1; i >= 0; i--)
        {
            var coord = lightAccentTiles[i];
            lightAccentTiles.RemoveAt(i);
            ApplyTileVisual(tiles[coord.x, coord.y]);
        }
    }
    #endregion
}

using System.Collections.Generic;
using UnityEngine;

public sealed class TilesController : MonoBehaviour
{
    [SerializeField] Mesh tileMesh;

    public Tile[,] tiles;

    GameController match;
    int tileCountX;
    int tileCountY;
    float tileSize;
    Transform tileContainer;
    List<Vector2Int> highlightTiles = new();
    Vector3 offset;

    public void SetSettings()
    {
        match = GameController.Instance;

        tileCountX = match.settings.tileCountX;
        tileCountY = match.settings.tileCountY;
        tileSize = match.settings.tileSize;
        offset = new();
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
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

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

    #region HighLight
    public void HighlighTiles(List<Vector2Int> availableMoves)
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].gameObject.layer = LayerMask.NameToLayer("Highlight");
            highlightTiles.Add(new Vector2Int(availableMoves[i].x, availableMoves[i].y));
        }
    }

    public void RemoveHighlightTiles(List<Vector2Int> needRemoveHighlightTiles)
    {
        for (int i = 0; i < needRemoveHighlightTiles.Count; i++)
        {
            tiles[needRemoveHighlightTiles[i].x, needRemoveHighlightTiles[i].y].gameObject.layer = LayerMask.NameToLayer("Tile");
            highlightTiles.Remove(new Vector2Int(needRemoveHighlightTiles[i].x, needRemoveHighlightTiles[i].y));
        }
        needRemoveHighlightTiles.Clear();
    }

    public void RemoveAllHighlightExcludeCurrentOnes(List<Vector2Int> currentHighlightTiles)
    {
        for (int i = 0; i < highlightTiles.Count; i++)
        {
            if (!currentHighlightTiles.Contains(highlightTiles[i]))
            {
                tiles[highlightTiles[i].x, highlightTiles[i].y].gameObject.layer = LayerMask.NameToLayer("Tile");
                highlightTiles.Remove(highlightTiles[i]);
            }
        }
    }

    public void RemoveAllHighlight()
    {
        foreach(var tileCoord in highlightTiles)
            tiles[tileCoord.x, tileCoord.y].gameObject.layer = LayerMask.NameToLayer("Tile");
        highlightTiles.Clear();
    }
    #endregion
}

using System.Collections.Generic;
using UnityEngine;

public sealed class TilesController : MonoBehaviour
{
    public Tile[,] tiles;

    GameController match;
    int tileCountX;
    int tileCountY;
    float tileSize;
    Transform tileContainer;
    List<Vector2Int> highlightTiles = new();

    public void SetSettings()
    {
        match = GameController.Instance;

        tileCountX = match.settings.tileCountX;
        tileCountY = match.settings.tileCountY;
        tileSize = match.settings.tileSize;
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

        #region Mesh

        Mesh mesh = new();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * size, 0, y * size);
        vertices[1] = new Vector3(x * size, 0, (y + 1) * size);
        vertices[2] = new Vector3((x + 1) * size, 0, y * size);
        vertices[3] = new Vector3((x + 1) * size, 0, (y + 1) * size);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        #endregion

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        Tile tile = tileObject.AddComponent<Tile>();
        tile.tileCenter = GetTileCenter(x, y);
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
        return new Vector3(x * tileSize, 0, y * tileSize) + new Vector3(tileSize / 2, 0, tileSize / 2);
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
        for(int i = 0; i < highlightTiles.Count; i++)
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
        for(int i = 0; i < highlightTiles.Count; i++)
        {
            tiles[highlightTiles[i].x, highlightTiles[i].y].gameObject.layer = LayerMask.NameToLayer("Tile");
            highlightTiles.Remove(highlightTiles[i]);
        }
    }
    #endregion
}

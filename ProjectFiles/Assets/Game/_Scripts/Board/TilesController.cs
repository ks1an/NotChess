using System.Collections.Generic;
using UnityEngine;

public sealed class TilesController : MonoBehaviour
{
    public GameObject[,] tiles;
    [SerializeField] Transform tileContainer;

    MatchController match;
    int tileCountX;
    int tileCountY;
    float tileSize;

    public void SetSettings()
    {
        match = MatchController.Instance;

        tileCountX = match.settings.tileCountX;
        tileCountY = match.settings.tileCountY;
        tileSize = match.settings.tileSize;
    }


    public void DestroyTiles()
    {
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                Destroy(tiles[x, y]);
    }

    public void GenerateTiles()
    {
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
                    tiles[x, y] = GenerateTile(tileSize, x, y, match.player.tileFirstMaterial);
                else
                    tiles[x, y] = GenerateTile(tileSize, x, y, match.player.tileSecondMaterial);
    }

    GameObject GenerateTile(float size, int x, int y, Material material)
    {
        GameObject tileObject = new(string.Format($"X: {x}, Y{y}"));
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
        return tileObject;
    }

    #region Get
    public Vector2Int GetTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

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
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }

    public void RemoveHighlighTiles(List<Vector2Int> availableMoves)
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        availableMoves.Clear();
    }
    #endregion
}

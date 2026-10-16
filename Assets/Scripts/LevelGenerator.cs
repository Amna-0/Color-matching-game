
using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    [Header("Tile Colors")]
    public Color[] colors =
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.blue,
        new Color(0.6f, 0.2f, 0.9f),
        new Color(1f, 0.45f, 0.1f),
        new Color(1f, 0.4f, 0.7f)
    };

    public string[] colorNames =
    {
        "Red", "Yellow", "Green", "Blue",
        "Purple", "Orange", "Pink"
    };

    [Header("Level Settings")]
    public bool generateOnStart = true;
    public Color safeColor = new Color(0.25f, 0.55f, 1f);
    public float tileThickness = 0.3f;

    private class TileInfo
    {
        public Tile tile;
        public int section;
        public int row;
    }

    private readonly List<TileInfo> allTiles =
        new List<TileInfo>();

    private Transform levelRoot;
    private int nextRow = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (generateOnStart)
            GenerateLevel();
    }

    public void GenerateLevel()
    {
        allTiles.Clear();
        nextRow = 0;

        if (levelRoot != null)
            Destroy(levelRoot.gameObject);

        GameObject root = new GameObject("GeneratedLevel");
        levelRoot = root.transform;
        levelRoot.SetParent(transform, false);

        CreateSafePlatform(
            "START",
            new Vector3(0f, -0.2f, 0f),
            new Vector3(4.5f, 0.4f, 4.5f),
            "Checkpoint"
        );

        // Section 1: Large square tiles
        CreateSquareSection(
    7, 6, 1.3f, 1.4f, 3.4f, 0
);

        // Safe bridge
        CreateSafePlatform(
    "Bridge 1",
    new Vector3(0f, -0.2f, 12f),
    new Vector3(8.5f, 0.4f, 3.5f),
    "Checkpoint"
);

    
       // Section 2: Square tiles
CreateSquareSection(
    7, 6, 1.3f, 1.4f, 14.5f, 1
);

        // Safe bridge
        CreateSafePlatform(
    "Bridge 2",
    new Vector3(0f, -0.2f, 23.1f),
    new Vector3(8.5f, 0.4f, 3.5f),
    "Checkpoint"
);

        // Section 3: Smaller square tiles
       CreateSquareSection(
    9, 8, 0.9f, 1.05f, 25.5f, 2
);

        // Finish platform
        CreateSafePlatform(
    "FINISH Platform",
    new Vector3(0f, -0.2f, 35f),
    new Vector3(8.5f, 0.4f, 4.5f),
    "Finish"
);

        CreateFinishMark();
        ShuffleColors(0);

        Debug.Log(
            "Level generated! Total tiles: " + allTiles.Count
        );
    }

    void CreateSquareSection(
        int columns,
        int rows,
        float tileSize,
        float spacing,
        float startZ,
        int section
    )
    {
        float startX = -((columns - 1) * spacing) / 2f;

        for (int r = 0; r < rows; r++)
        {
            int rowID = nextRow++;

            for (int c = 0; c < columns; c++)
            {
                Vector3 position = new Vector3(
                    startX + c * spacing,
                    -tileThickness / 2f,
                    startZ + r * spacing
                );

                GameObject obj = GameObject.CreatePrimitive(
                    PrimitiveType.Cube
                );

                obj.name = "Square_Tile_S" + section
                    + "_R" + r + "_C" + c;

                obj.transform.SetParent(levelRoot, false);
                obj.transform.position = position;
                obj.transform.localScale = new Vector3(
                    tileSize, tileThickness, tileSize
                );

                Tile tile = obj.AddComponent<Tile>();

                allTiles.Add(new TileInfo
                {
                    tile = tile,
                    section = section,
                    row = rowID
                });
            }
        }
    }

    void CreateTriangleSection(
        int columns,
        int rows,
        float tileSize,
        float spacing,
        float startZ,
        int section
    )
    {
        float startX = -((columns - 1) * spacing) / 2f;

        for (int r = 0; r < rows; r++)
        {
            int rowID = nextRow++;

            for (int c = 0; c < columns; c++)
            {
                Vector3 position = new Vector3(
                    startX + c * spacing,
                    -tileThickness / 2f,
                    startZ + r * spacing
                );

                GameObject obj = new GameObject(
                    "Triangle_Tile_S" + section
                    + "_R" + r + "_C" + c
                );

                obj.transform.SetParent(levelRoot, false);
                obj.transform.position = position;

                Mesh mesh = CreateTriangleMesh(
                    tileSize,
                    tileThickness,
                    (r + c) % 2 == 0
                );

                MeshFilter filter = obj.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;

                MeshRenderer renderer =
                    obj.AddComponent<MeshRenderer>();

                renderer.material = CreateMaterial(Color.white);

                MeshCollider meshCollider =
                    obj.AddComponent<MeshCollider>();

                meshCollider.sharedMesh = mesh;

                Tile tile = obj.AddComponent<Tile>();

                allTiles.Add(new TileInfo
                {
                    tile = tile,
                    section = section,
                    row = rowID
                });
            }
        }
    }

    Mesh CreateTriangleMesh(
        float size,
        float thickness,
        bool pointForward
    )
    {
        float half = size / 2f;
        float bottomY = -thickness / 2f;
        float topY = thickness / 2f;

        float frontZ = pointForward ? half : -half;
        float backZ = -frontZ;

        Vector3[] vertices =
        {
            new Vector3(-half, topY, backZ),
            new Vector3( half, topY, backZ),
            new Vector3(0f, topY, frontZ),

            new Vector3(-half, bottomY, backZ),
            new Vector3( half, bottomY, backZ),
            new Vector3(0f, bottomY, frontZ)
        };

        int[] triangles =
        {
            0, 2, 1,
            3, 4, 5,

            0, 1, 4, 0, 4, 3,
            1, 2, 5, 1, 5, 4,
            2, 0, 3, 2, 3, 5
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void CreateSafePlatform(
        string platformName,
        Vector3 position,
        Vector3 size,
        string triggerTag
    )
    {
        GameObject platform = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        platform.name = platformName;
        platform.transform.SetParent(levelRoot, false);
        platform.transform.position = position;
        platform.transform.localScale = size;

        Renderer renderer = platform.GetComponent<Renderer>();
        renderer.material = CreateMaterial(safeColor);

        // A separate trigger lets the player stand on the platform.
        GameObject trigger = new GameObject(
            platformName + "_Trigger"
        );

        trigger.transform.SetParent(platform.transform, false);
        trigger.transform.localPosition = new Vector3(
            0f, 0.5f, 0f
        );

        BoxCollider triggerCollider =
            trigger.AddComponent<BoxCollider>();

        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(
            1f, 0.5f, 1f
        );

        TrySetTag(trigger, triggerTag);
    }

    void CreateFinishMark()
    {
        float platformZ = 35f;
        float platformWidth = 8.4f;
        float squareSize = 0.3f;

        // Checkered strip near the far end.
        for (int x = 0; x < 28; x++)
        {
            GameObject square = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            square.name = "Finish_Checker_" + x;
            square.transform.SetParent(levelRoot, false);

            square.transform.position = new Vector3(
                -platformWidth / 2f + squareSize / 2f
                    + x * squareSize,
                0.015f,
                platformZ + 1.5f
            );

            square.transform.localScale = new Vector3(
                squareSize, 0.03f, 0.45f
            );

            Renderer renderer = square.GetComponent<Renderer>();

            renderer.material = CreateMaterial(
                x % 2 == 0 ? Color.black : Color.white
            );
        }

        GameObject label = new GameObject("FINISH_Label");
        label.transform.SetParent(levelRoot, false);

        label.transform.position = new Vector3(
            0f, 0.65f, platformZ + 0.3f
        );

        label.transform.rotation = Quaternion.Euler(
            0f, 180f, 0f
        );

        TextMesh text = label.AddComponent<TextMesh>();
        text.text = "FINISH";
        text.fontSize = 80;
        text.characterSize = 0.12f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.black;
    }

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");

        Material material = new Material(shader);
        material.color = color;

        return material;
    }

    void TrySetTag(GameObject obj, string tagName)
    {
        try
        {
            obj.tag = tagName;
        }
        catch (UnityException)
        {
            Debug.LogError(
                "Tag '" + tagName
                + "' does not exist. Add it in Tags and Layers."
            );
        }
    }

    public void ShuffleColors(int targetColorIndex)
    {
        if (colors == null || colors.Length == 0)
            return;

        // Give every tile a random color.
        foreach (TileInfo info in allTiles)
        {
            int randomIndex = Random.Range(0, colors.Length);

            info.tile.SetColor(
                randomIndex,
                colors[randomIndex]
            );
        }

        // Guarantee at least one target-color tile in every row.
        Dictionary<int, List<TileInfo>> rows =
            new Dictionary<int, List<TileInfo>>();

        foreach (TileInfo info in allTiles)
        {
            if (!rows.ContainsKey(info.row))
                rows[info.row] = new List<TileInfo>();

            rows[info.row].Add(info);
        }

        foreach (KeyValuePair<int, List<TileInfo>> row in rows)
        {
            if (row.Value.Count == 0)
                continue;

            int chosen = Random.Range(0, row.Value.Count);

            row.Value[chosen].tile.SetColor(
                targetColorIndex,
                colors[targetColorIndex]
            );
        }
    }

    public void HideWrongTiles(int targetColorIndex)
    {
        foreach (TileInfo info in allTiles)
        {
            if (info.tile.colorIndex != targetColorIndex)
                info.tile.Hide();
        }
    }

    public void ShowAllTiles()
    {
        foreach (TileInfo info in allTiles)
        {
            info.tile.Show();
        }
    }

    public int GetSectionIndex(Vector3 position)
    {
        // Approximate section boundaries along the Z axis.
        if (position.z >= 2.7f && position.z < 11.5f)
            return 0;

        if (position.z >= 13.6f && position.z < 22.3f)
            return 1;

        if (position.z >= 24.9f && position.z < 33.5f)
            return 2;

        return -1;
    }
}




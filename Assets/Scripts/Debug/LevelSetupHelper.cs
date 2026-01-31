// File: Scripts/Debug/LevelSetupHelper.cs
using UnityEngine;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Debug helper to quickly set up a test level in the editor.
    /// Attach to an empty GameObject and click context menu to generate.
    /// </summary>
    public class LevelSetupHelper : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 5;
        [SerializeField] private float tileSpacing = 1f;

        [Header("Special Tiles (Grid Coords)")]
        [SerializeField] private Vector2Int startCoord = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int exitCoord = new Vector2Int(4, 4);
        [SerializeField] private Vector2Int keyCoord = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int[] trapCoords = new Vector2Int[] 
        { 
            new Vector2Int(1, 2), 
            new Vector2Int(3, 1) 
        };

        [Header("References")]
        [SerializeField] private Transform tilesParent;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject keyPrefab;

        [ContextMenu("Generate Test Level")]
        public void GenerateTestLevel()
        {
            // Create tiles parent if not set
            if (tilesParent == null)
            {
                GameObject parent = new GameObject("Tiles");
                tilesParent = parent.transform;
            }

            // Clear existing tiles
            while (tilesParent.childCount > 0)
            {
                DestroyImmediate(tilesParent.GetChild(0).gameObject);
            }

            // Generate grid
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Vector3 worldPos = new Vector3(x * tileSpacing, 0, y * tileSpacing);

                    GameObject tile = TileFactory.CreateTile(coord, worldPos, tilesParent);
                    TileData data = tile.GetComponent<TileData>();

                    // Set special tile flags via reflection or serialized fields
                    // For now, we'll use a helper method
                    SetTileFlags(tile, coord);
                }
            }

            Debug.Log($"[LevelSetupHelper] Generated {gridWidth}x{gridHeight} grid.");
        }

        private void SetTileFlags(GameObject tile, Vector2Int coord)
        {
            // Access serialized fields via SerializedObject in editor
            // For runtime, we'll add a setup component
            TileData data = tile.GetComponent<TileData>();

            // Check if this is a special tile
            bool isStart = coord == startCoord;
            bool isExit = coord == exitCoord;
            bool isKey = coord == keyCoord;
            bool isTrap = System.Array.Exists(trapCoords, c => c == coord);

            // Set visual indicator via name for now
            if (isStart) tile.name += "_START";
            if (isExit) tile.name += "_EXIT";
            if (isKey) tile.name += "_KEY";
            if (isTrap) tile.name += "_TRAP";

            // Add trap component if needed
            if (isTrap)
            {
                tile.AddComponent<TrapTile>();
            }
        }

        [ContextMenu("Create Player")]
        public void CreatePlayer()
        {
            GameObject player;
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab);
            }
            else
            {
                // Create primitive player
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            player.name = "Player";
            player.AddComponent<PlayerGridMover>();

            // Position at start
            Vector3 startPos = new Vector3(startCoord.x * tileSpacing, 0.5f, startCoord.y * tileSpacing);
            player.transform.position = startPos;

            // Set different color
            Renderer rend = player.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Standard"));
                rend.material.color = Color.cyan;
            }

            Debug.Log("[LevelSetupHelper] Player created.");
        }

        [ContextMenu("Create Key")]
        public void CreateKey()
        {
            GameObject key;
            if (keyPrefab != null)
            {
                key = Instantiate(keyPrefab);
            }
            else
            {
                // Create primitive key
                key = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                key.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
            }

            key.name = "Key";
            key.AddComponent<KeyPickup>();
            key.AddComponent<MaskVisibleGroup>();

            // Position at key spawn
            Vector3 keyPos = new Vector3(keyCoord.x * tileSpacing, 0.3f, keyCoord.y * tileSpacing);
            key.transform.position = keyPos;

            // Set golden color
            Renderer rend = key.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Standard"));
                rend.material.color = Color.yellow;
            }

            Debug.Log("[LevelSetupHelper] Key created.");
        }

        [ContextMenu("Create All Managers")]
        public void CreateAllManagers()
        {
            // Create managers parent
            GameObject managers = new GameObject("--- MANAGERS ---");

            // GameManager
            GameObject gm = new GameObject("GameManager");
            gm.transform.SetParent(managers.transform);
            gm.AddComponent<GameManager>();

            // MaskManager
            GameObject mm = new GameObject("MaskManager");
            mm.transform.SetParent(managers.transform);
            mm.AddComponent<MaskManager>();

            // GridManager
            GameObject grid = new GameObject("GridManager");
            grid.transform.SetParent(managers.transform);
            grid.AddComponent<GridManager>();

            // LevelTimer
            GameObject timer = new GameObject("LevelTimer");
            timer.transform.SetParent(managers.transform);
            timer.AddComponent<LevelTimer>();

            // AudioManager
            GameObject audio = new GameObject("AudioManager");
            audio.transform.SetParent(managers.transform);
            audio.AddComponent<SimpleAudioManager>();

            Debug.Log("[LevelSetupHelper] All managers created.");
        }
    }
}

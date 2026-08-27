using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class BackgroundGeneration : MonoBehaviour
{

    [Serializable] public struct RenderDistance
    {
        public int x;
        public int y;
    }

    // Player
    public GameObject player;
    private Rigidbody2D playerRb;
    private Vector2 playerPos;

    // World generation
    public int worldSeed = SeedManager.WorldSeed;
    public Vector2Int chunkSize = new Vector2Int(20, 20);
    [SerializeField] public RenderDistance renderDistance = new RenderDistance();
    private Dictionary<Vector2Int, Chunk> loadedChunks;
    private HashSet<Vector2Int> requiredChunks;
    private List<Vector2Int> chunksToUnload;
    private bool loadChunks;
    private Vector2Int currentPlayerChunk;
    private Vector2Int _lastPlayerChunk;
    public int minStars = 5;
    public int maxStars = 30;

    // Chunk rendering
    [SerializeField] private Sprite sprite;
    Queue<SpriteRenderer> rendererQueue = new Queue<SpriteRenderer>();
    Dictionary<Vector2Int, SpriteRenderer> activeChunks = new Dictionary<Vector2Int, SpriteRenderer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
        renderDistance.x = 3;
        renderDistance.y = 3;

        loadedChunks = new Dictionary<Vector2Int, Chunk>();
        requiredChunks = new HashSet<Vector2Int>();
        chunksToUnload = new List<Vector2Int>();
        if (loadedChunks.Count <= 0 || loadedChunks == null)
            loadChunks = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerRb.linearVelocity.magnitude > 0 || loadedChunks.Count <= 0)
        {
            playerPos = player.transform.position;
            currentPlayerChunk = Vector2Int.RoundToInt(playerPos / chunkSize);

            if (currentPlayerChunk != _lastPlayerChunk)
                loadChunks = true;
        }

        if (loadChunks)
        {
            requiredChunks.Clear();
            for(int y = -renderDistance.y / 2; y <= renderDistance.y / 2; y++)
            {
                for(int x = -renderDistance.x / 2; x <= renderDistance.x / 2; x++)
                {
                    // The chunkID is it's position in space
                    var chunkId = currentPlayerChunk + new Vector2Int(x,y);
                    requiredChunks.Add(chunkId);
                }
            }

            chunksToUnload.Clear();
            foreach (var chunkId in loadedChunks.Keys)
            {
                if (!requiredChunks.Contains(chunkId))
                    chunksToUnload.Add(chunkId);
            }

            foreach (var chunkId in chunksToUnload) UnLoadChunk(chunkId);

            foreach (var chunkId in requiredChunks)
            {
                if (!loadedChunks.ContainsKey(chunkId))
                    LoadChunk(chunkId);
            }

            string debug = string.Empty;
            for(int i = 0; i < loadedChunks.Count; i++)
            {
                debug += "[ " + loadedChunks.ElementAt(i).Key + " ]\n";
            }
            Debug.Log("Current player chunk: " + currentPlayerChunk);
            Debug.Log("Number of elements: " + loadedChunks.Count);
            Debug.Log(debug);

            loadChunks = false;
        }

        _lastPlayerChunk = currentPlayerChunk;
    }

    private void LoadChunk(Vector2Int chunkId)
    {
        // Initialize the chunk's seed
        var chunkSeed = SeedManager.GetChunkSeed(chunkId);
        UnityEngine.Random.InitState(chunkSeed);

        // The max is exclusive so i add 1 to the range. To improve readability with variables in Unity Editor
        // (so that you have to write 30 for 30 max stars and not 31)
        var starCount = UnityEngine.Random.Range(minStars, (maxStars + 1));
        List<Star> stars = new List<Star>();
        for (int k = 0; k < starCount; k++)
        {
            var spMinX = (chunkId.x * chunkSize.x) - (chunkSize.x / 2);
            var spMaxX = (chunkId.x * chunkSize.x) + (chunkSize.x / 2);
            var spMinY = (chunkId.y * chunkSize.y) - (chunkSize.y / 2);
            var spMaxY = (chunkId.y * chunkSize.y) + (chunkSize.y / 2);
            var spX = UnityEngine.Random.Range(spMinX, spMaxX);
            var spY = UnityEngine.Random.Range(spMinY, spMaxY);
            Vector2 spawnPoint = new Vector2(spX, spY);
            var size = Mathf.RoundToInt(UnityEngine.Random.Range(1, 11));
            stars.Add(new Star(spawnPoint, size));
        }
        Chunk chunk = new Chunk(chunkId, starCount, stars);
        loadedChunks.Add(chunkId, chunk);
        RenderChunk(chunkId);
    }

    private SpriteRenderer GetSpriteRenderer()
    {
        SpriteRenderer renderer;
        if (rendererQueue.Count > 0)
        {
            renderer = rendererQueue.Dequeue();
            renderer.gameObject.SetActive(true);
        }
        else
        {
            GameObject gameObject = new GameObject("Chunk");
            renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
        }
        return renderer;
    }

    private void RenderChunk(Vector2Int chunkId)
    {
        var renderer = GetSpriteRenderer();

        // Calculate the chunk position 
        Vector2 chunkPos = (chunkId * chunkSize);
        renderer.transform.position = chunkPos;

        // Set chunk size
        renderer.transform.localScale = new Vector3(chunkSize.x, chunkSize.y, 1f);

        renderer.color = new Color
            (
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value
            );

        renderer.sortingOrder = -1;

        activeChunks[chunkId] = renderer;
    }

    private void EnqueueChunk(SpriteRenderer renderer)
    {
        renderer.gameObject.SetActive(false);
        rendererQueue.Enqueue(renderer);
    }

    private void UnLoadChunk(Vector2Int chunkId)
    {
        // Dispose del chunk
        if (activeChunks.TryGetValue(chunkId, out SpriteRenderer rend))
        {
            EnqueueChunk(rend);
            activeChunks.Remove(chunkId);
        }

        loadedChunks.Remove(chunkId);
    }
}

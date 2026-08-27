using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public static class SeedManager
    {
        public static int WorldSeed { get; set; } = 1234;
        public static int GetChunkSeed(Vector2Int chunkId)
        {
            unchecked
            {
                int chunkSeed = WorldSeed;
                chunkSeed = (chunkSeed * 31) + chunkId.x;
                chunkSeed = (chunkSeed * 31) + chunkId.y;

                return chunkSeed;
            }
        }
    }
}
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Chunk
    {
        public Vector2Int ChunkId {  get; private set; }
        public int StarCount { get; private set; }
        public List<Star> Stars { get; private set; }
        public Chunk(Vector2Int chunkId, int starCount, List<Star> stars)
        {
            this.ChunkId = chunkId;
            this.StarCount = starCount;
            this.Stars = stars;
        }
    }
}
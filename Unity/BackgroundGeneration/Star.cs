using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Star
    {
        public Vector2 SpawnPoint {  get; set; }
        public float Size { get; set; }

        public Star(Vector2 spawnPoint, float size)
        {
            this.SpawnPoint = spawnPoint;
            this.Size = size;
        }
    }
}
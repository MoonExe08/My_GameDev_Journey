using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Star
    {
        public Vector2 SpawnPoint {  get; set; }
        public int Size { get; set; }

        public Star(Vector2 spawnPoint, int size)
        {
            this.SpawnPoint = spawnPoint;
            this.Size = size;
        }
    }
}
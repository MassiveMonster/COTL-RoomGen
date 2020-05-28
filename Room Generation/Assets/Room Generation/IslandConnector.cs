using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MMRoomGeneration
{
    public class IslandConnector : MonoBehaviour
    {
       // public float Angle;
        public bool Active = false;
        public Direction MyDirection;
        public enum Direction { North, East, South, West }

        private void OnEnable()
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}

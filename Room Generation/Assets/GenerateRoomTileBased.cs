using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GenerateRoomTileBased : MonoBehaviour
{

    public bool North = false;
    public bool East = false;
    public bool South = false;
    public bool West = false;

    public List<TilePiece> Tiles;
    public class TilePiece
    {
        public int x, y, tile;
        public TilePiece(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool Exists(int x, int y, List<TilePiece> Tiles)
        {
            foreach (TilePiece t in Tiles)
                if (t.x == x && t.y == y)
                    return true;

            return false;
        }

        public static void SetTile(int x, int y, List<TilePiece> Tiles, int tile)
        {
            foreach (TilePiece t in Tiles)
                if (t.x == x && t.y == y)
                    t.tile = tile;
        }
    }

    public int Seed = 0;
    System.Random RandomSeed;

    [Button("Generate Random Seed")]
    void GenerateRandomSeed()
    {
        Seed = Random.Range(0, int.MaxValue);
        Generate();
    }

    [Button("GENERATE!", ButtonSizes.Gigantic)]
    void Generate()
    {
        RandomSeed = new System.Random(Seed);

        Tiles = new List<TilePiece>();
        Tiles.Add(new TilePiece(0, 0));

        if (North)
            GeneratePath(0, 0, 90);
        if (East)
            GeneratePath(0, 0, 0);
        if (South)
            GeneratePath(0, 0, 270);
        if (West)
            GeneratePath(0, 0, 180);

        PlacePrefabs();
    }

    public GameObject Prefab;
    public GameObject DoorPrefab;
    void PlacePrefabs()
    {
        int i = -1;
        while (++i < transform.childCount)
            Destroy(transform.GetChild(i).gameObject);

        foreach (TilePiece t in Tiles)
        {
            Instantiate(t.tile == 0 ? Prefab : DoorPrefab, new Vector3(t.x, t.y), Quaternion.identity, transform);
        }
    }


    void GeneratePath(int x, int y, float Angle)
    {
        int Steps = RandomSeed.Next(2,4);
        while (--Steps >= 0)
        {
            x += (int)(1 * Mathf.Cos(Angle * Mathf.Deg2Rad));
            y += (int)(1 * Mathf.Sin(Angle * Mathf.Deg2Rad));
            Tiles.Add(new TilePiece(x, y));

            int RandomDirection = RandomSeed.Next(0, 4);
            switch (RandomDirection)
            {
                case 0:
                    int Diversion = RandomSeed.Next(1, 4);
                    while (--Diversion >= 0)
                    {
                        int StepX = (int)(1 * Mathf.Cos((Angle + 90) * Mathf.Deg2Rad));
                        int StepY = (int)(1 * Mathf.Sin((Angle + 90) * Mathf.Deg2Rad));
                        if (TilePiece.Exists(x + StepX, y + StepY, Tiles))
                            break;
                        else
                        {
                            x += StepX;
                            y += StepY;
                            Tiles.Add(new TilePiece(x, y));
                        }
                    }
                    break;
                case 1:
                    Diversion = RandomSeed.Next(1, 4);
                    while (--Diversion >= 0)
                    {
                        int StepX = (int)(1 * Mathf.Cos((Angle - 90) * Mathf.Deg2Rad));
                        int StepY = (int)(1 * Mathf.Sin((Angle - 90) * Mathf.Deg2Rad));
                        if (TilePiece.Exists(x + StepX, y + StepY, Tiles))
                            break;
                        else
                        {
                            x += StepX;
                            y += StepY;
                            Tiles.Add(new TilePiece(x, y));
                        }
                    }
                    break;
            }
        }

        //set door
        TilePiece.SetTile(x, y, Tiles, 1);

    }
}

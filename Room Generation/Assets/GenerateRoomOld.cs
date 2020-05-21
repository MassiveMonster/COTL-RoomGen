using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class GenerateRoomOld : MonoBehaviour
{

    public bool North = false;
    public bool East = false;
    public bool South = false;
    public bool West = false;

    public int Width = 5;
    public int Height = 5;
    int[,] Grid;

    System.Random RandomSeed;
    public GameObject Prefab;
    public GameObject DoorPrefab;

    public int Seed = 0;

    [Button("Generate Random Seed")]
    void GenerateRandomSeed()
    {
        Seed = Random.Range(0, int.MaxValue);
        GenerateGrid();
    }

    [Button("GENERATE!", ButtonSizes.Gigantic)]
    void GenerateGrid()
    {
        RandomSeed = new System.Random(Seed);
        //CREATE PATH
        Width = RandomSeed.Next(3, 7);
        Height = RandomSeed.Next(3, 7);
        Grid = new int[Width, Height];
        Vector2Int PrevTile;
        if (North)
        {
            //if south and east
            //if south and west
            if (South)
            {
                PrevTile = CreatePath(RandomSeed.Next(1, Width - 1), 0, 90, true); //CreatePath(RandomSeed.Next(1, Width - 1), Height - 1, 270, true);
                if (East && West)
                {
                    CreatePath(Width - 1, RandomSeed.Next(1, PrevTile.y - 1), 180, true);
                }
            }
            //if east and west
            //if east
            //if west

        } else if (East)
        {
            if (West)
                CreatePath(Width - 1, RandomSeed.Next(1, Height - 1), 180, true);

            //if west and south

            //if south
        }
        /*
        if (South)
            CreatePath(RandomSeed.Next(1, Width - 1), 0, 90);

        if (West)
            CreatePath(0, RandomSeed.Next(1, Height - 1), 0);
            */
        PlacePrefabs();
    }

    void PlacePrefabs()
    {
        int i = -1;
        while (++i < transform.childCount)
            Destroy(transform.GetChild(i).gameObject);

        int y = -1;
        while (++y < Height)
        {
            int x = -1;
            while (++x < Width)
            {
                if (Grid[x, y] == 1)
                    Instantiate(Prefab, new Vector3(x, y), Quaternion.identity, transform);
                if (Grid[x, y] == 2)
                    Instantiate(DoorPrefab, new Vector3(x, y), Quaternion.identity, transform);
            }
        }
    }

    Vector2Int CreatePath(int x, int y, float Angle, bool PlaceEndDoor, int MaxX = -1, int MaxY = -1)
    {
        bool Loop = true;
        bool PlacedFirst = false;
        Vector2Int PrevTile = new Vector2Int();
        while (Loop)
        {
            Debug.Log(x + "  " + y);
            SetTile(x, y, (!PlacedFirst ? 2 : 1));
            PrevTile = new Vector2Int(x, y);
            PlacedFirst = true;
            x += (int)(1 * Mathf.Cos(Angle * Mathf.Deg2Rad));
            y += (int)(1 * Mathf.Sin(Angle * Mathf.Deg2Rad));

            if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                if (PlaceEndDoor)
                    SetTile(PrevTile.x, PrevTile.y, 2);
                return PrevTile;
            }
            else
            {
                int RandomDirection = RandomSeed.Next(0, 4);
                Debug.Log("Ran Dir: " + RandomDirection);

                if (Angle == 90 || Angle == 270)
                {
                    switch (RandomDirection)
                    {
                        case 0:
                            if (x < Width - 1)
                            {
                                int Distance = RandomSeed.Next(x, Width);
                                SetTile(x, y, 1);
                                while (++x < Width - 2)
                                {
                                    SetTile(x, y, 1);
                                    PrevTile = new Vector2Int(x, y);
                                }
                                if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
                                    break;
                            }
                            break;
                        case 1:
                            if (x > 0)
                            {
                                int Distance = RandomSeed.Next(0, x);
                                SetTile(x, y, 1);
                                while (--x > 1)
                                {
                                    SetTile(x, y, 1);
                                    PrevTile = new Vector2Int(x, y);
                                }
                                if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
                                    break;
                            }

                            break;
                    }
                }
                else
                {
                    switch (RandomDirection)
                    {
                        case 0:
                            if (y < Height - 1)
                            {
                                int Distance = RandomSeed.Next(y, Height);
                                SetTile(x, y, 1);
                                while (++y < Height - 2)
                                {
                                    SetTile(x, y, 1);
                                    PrevTile = new Vector2Int(x, y);
                                }
                                if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
                                    break;
                            }
                            break;
                        case 1:
                            if (y > 0)
                            {
                                int Distance = RandomSeed.Next(0, y);
                                SetTile(x, y, 1);
                                while (--y > 1)
                                {
                                    SetTile(x, y, 1);
                                    PrevTile = new Vector2Int(x, y);
                                }
                                if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
                                    break;
                            }

                            break;
                    }
                }

            }
        } return Vector2Int.zero;
    }

    void SetTile(int x, int y, int Num)
    {
       // if (Grid[x, y] == 0 || Num == 2) 
            Grid[x, y] = Num;
    }
}

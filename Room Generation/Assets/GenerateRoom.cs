using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Diagnostics;
public class GenerateRoom : MonoBehaviour
{
    public CompositeCollider2D RoomTransform;

    public int Scale = 2;

    public bool North = false;
    public bool East = false;
    public bool South = false;
    public bool West = false;

    public int Seed = 0;
    System.Random RandomSeed;

    public IslandPiece NorthIsland;
    public IslandPiece EastIsland;
    public IslandPiece SouthIsland;
    public IslandPiece WestIsland;

    public List<IslandPiece> StartPieces = new List<IslandPiece>();
    public List<IslandPiece> IslandPieces = new List<IslandPiece>();

    public List<IslandPiece> NorthIslandPieces;
    public List<IslandPiece> EastIslandPieces;
    public List<IslandPiece> SouthIslandPieces;
    public List<IslandPiece> WestIslandPieces;

    List<IslandPiece> Pieces = new List<IslandPiece>();
    IslandPiece StartPiece;
    IslandPiece CurrentPiece;
    IslandPiece PrevPiece;

    List<int> PreviousSeeds = new List<int>();

    private void Start()
    {
        PreviousSeeds.Add(Seed);
    }

    [Button("<< Previous Seed")]
    void GeneratePreviousSeed()
    {
        if (PreviousSeeds.Count > 1)
        {
            PreviousSeeds.RemoveAt(PreviousSeeds.Count - 1);
            Seed = PreviousSeeds[PreviousSeeds.Count - 1];
            Generate();
        }
    }

    [Button("Generate Random Seed")]
    void GenerateRandomSeed()
    {
        Seed = Random.Range(0, int.MaxValue);
        PreviousSeeds.Add(Seed);
        Generate();
    }

  
    Stopwatch stopwatch = new Stopwatch();
    float PrevTime;
    [Button("GENERATE!", ButtonSizes.Gigantic)]
    void Generate()
    {
        //ClearPrefabs();
        Pieces = new List<IslandPiece>();

        RandomSeed = new System.Random(Seed);

        StartPiece = CreateStartPiece();
        Pieces.Add(StartPiece);

        CollateLists();

        if (North)
            GeneratePath(IslandConnector.Direction.North);
        if (East)
            GeneratePath(IslandConnector.Direction.East);
        if (South)
            GeneratePath(IslandConnector.Direction.South);
        if (West)
            GeneratePath(IslandConnector.Direction.West);

        CompositeColliders();

        Physics2D.SyncTransforms();

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
        RoomTransform.GenerateGeometry();
        Physics2D.SyncTransforms();


        PlaceDecorations();

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Outlines;
        RoomTransform.GenerateGeometry();
    }

    void CompositeColliders()
    {
        foreach (IslandPiece i in Pieces)
            i.Collider.usedByComposite = true;
    }

    void CollateLists()
    {
        NorthIslandPieces = new List<IslandPiece>();
        EastIslandPieces = new List<IslandPiece>();
        SouthIslandPieces = new List<IslandPiece>();
        WestIslandPieces = new List<IslandPiece>();
        foreach (IslandPiece i in IslandPieces)
        {
            if (i.GetConnectorsDirection(IslandConnector.Direction.North, false).Count > 0) NorthIslandPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.East, false).Count > 0) EastIslandPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.South, false).Count > 0) SouthIslandPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.West, false).Count > 0) WestIslandPieces.Add(i);
        }
    }

    IslandPiece GetIslandListByDirection(IslandConnector.Direction Direction)
    {
        switch (Direction)
        {
            case IslandConnector.Direction.North: return NorthIslandPieces[RandomSeed.Next(0,NorthIslandPieces.Count)];
            case IslandConnector.Direction.East: return EastIslandPieces[RandomSeed.Next(0, EastIslandPieces.Count)];
            case IslandConnector.Direction.South: return SouthIslandPieces[RandomSeed.Next(0, SouthIslandPieces.Count)];
            case IslandConnector.Direction.West: return WestIslandPieces[RandomSeed.Next(0, WestIslandPieces.Count)];
        }
        return null;
    }

    [System.Serializable]
    public class ListOfDecorations
    {
        public List<DecorationAndProbability> DecorationAndProabilies;
        int[] Weights;
        int Index;
        public GameObject GetRandomGameObject(double RandomSeed)
        {
            Weights = new int[DecorationAndProabilies.Count];
            int i = -1;
            while (++i < DecorationAndProabilies.Count)
                Weights[i] = DecorationAndProabilies[i].Probability;

            Index = GetRandomWeightedIndex(Weights, RandomSeed);

            return DecorationAndProabilies[Index].gameObject;
        }
    }

    [System.Serializable]
    public class DecorationAndProbability
    {
        [Range(0,100)]
        public int Probability = 50;
        public GameObject gameObject;
    }

    public static int GetRandomWeightedIndex(int[] weights, double Random)
    {
        if (weights == null || weights.Length == 0) return -1;

        int total = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] >= 0) total += weights[i];
        }
    
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] <= 0f) continue;

            s += (float)weights[i] / total;
            if (s >= Random) return i;
        }

        return -1;
    }

    [Title("1x1 Decoration Pieces")]
    [HideLabel]
    public ListOfDecorations DecorationPiece = new ListOfDecorations();

    [Title("2x2 Decoration Pieces")]
    [HideLabel]
    public ListOfDecorations DecorationPiece2x2 = new ListOfDecorations();

    [Title("3x3 Decoration Pieces")]
    [HideLabel]
    public ListOfDecorations DecorationPiece3x3 = new ListOfDecorations();

    [Title("3x3 Decoration Pieces - Tall", "Tall pieces for the back of the level")]
    [HideLabel]
    public ListOfDecorations DecorationPiece3x3Tall = new ListOfDecorations();

    [Button("Generate Decorations")]
    void PlaceDecorations()
    {
        List<List<int>> DecorationGrid = new List<List<int>>();
        int width = (int)(Mathf.Max(RoomTransform.bounds.size.x, RoomTransform.bounds.size.y));
        int height = width;
        for (int y = -height; y < height; y+= Scale)
        {
            List<int> Row = new List<int>();
            for (int x = -width; x < width; x+= Scale)
            {
                if (RoomTransform.ClosestPoint(new Vector2(x , y - (Scale*0.5f))) != new Vector2(x , y - (Scale * 0.5f)) && RoomTransform.ClosestPoint(new Vector2(x, y + (Scale * 0.5f))) != new Vector2(x, y + (Scale * 0.5f)))
                    Row.Add(1);
                else
                    Row.Add(0);
            }
            DecorationGrid.Add(Row);
        }

        //plots next to empty plots must be 1 piece 
        for (int y = 0; y < DecorationGrid.Count; y++)
            for (int x = 0; x < DecorationGrid.Count; x++)
            {
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                    {
                        if (DecorationGrid[x][y] != 0 && !(i == 0 && j == 0) && (x + i >= 0) && (y + j >= 0) && (x + i < DecorationGrid.Count) && (y + j < DecorationGrid.Count))
                        {
                            if (DecorationGrid[x + i][ y + j] == 0)
                                DecorationGrid[x][y] = 2;
                        }
                    }
            }


        //3x3 grids
        int GridSize = 3;
        for (int y = 0; y < DecorationGrid.Count; y++)
            for (int x = 0; x < DecorationGrid.Count; x++)
            {
                bool PlaceGridTile = true;
                for (int i = 0; i < GridSize; i++)
                    for (int j = 0; j < GridSize; j++)
                    {
                        if (DecorationGrid[x][y] != 1 || (x + i >= DecorationGrid.Count) || (y + j >= DecorationGrid.Count) || DecorationGrid[x + i][y + j] != 1 )
                            PlaceGridTile = false;
                    }

                if (PlaceGridTile)
                {
                    DecorationGrid[x][y] = 4;
                    for (int i = 0; i < GridSize; i++)
                        for (int j = 0; j < GridSize; j++)
                        {
                            if (!(i == 0 && j == 0))
                                DecorationGrid[x + i][y + j] = 999;// this will be part of the 4x4 so don't place anything
                        }
                }

            }

        //2x2 grids
        GridSize = 2;
        for (int y = 0; y < DecorationGrid.Count; y++)
            for (int x = 0; x < DecorationGrid.Count; x++)
            {
                bool PlaceGridTile = true;
                for (int i = 0; i < GridSize; i++)
                    for (int j = 0; j < GridSize; j++)
                    {
                        if (DecorationGrid[x][y] != 1 || (x + i >= DecorationGrid.Count) || (y + j >= DecorationGrid.Count) || DecorationGrid[x + i][y + j] != 1)
                            PlaceGridTile = false;
                    }

                if (PlaceGridTile)
                {
                    DecorationGrid[x][y] = 3;
                    for (int i = 0; i < GridSize; i++)
                        for (int j = 0; j < GridSize; j++)
                        {
                            if (!(i == 0 && j == 0))
                                DecorationGrid[x + i][y + j] = 999;// this will be part of the 4x4 so don't place anything
                        }
                }
                else if (DecorationGrid[x][y] == 1) DecorationGrid[x][y] = 2;
            }

        Vector3 Position;
        Vector3 ClosestPosition;
        for (int y = 0; y < DecorationGrid.Count; y++)
            for (int x = 0; x < DecorationGrid.Count; x++)
            {
                Position = new Vector3((x * Scale) - width, (y * Scale) - height);
                ClosestPosition = RoomTransform.ClosestPoint(Position);
                if (Vector3.Distance(ClosestPosition, Position) < 5 * Scale)
                {
                    if (DecorationGrid[y][x] == 2)
                        Instantiate(DecorationPiece.GetRandomGameObject(RandomSeed.NextDouble()), Position , Quaternion.identity, RoomTransform.transform);

                    if (DecorationGrid[y][x] == 3)
                        Instantiate(DecorationPiece2x2.GetRandomGameObject(RandomSeed.NextDouble()), Position, Quaternion.identity, RoomTransform.transform);

                    if (DecorationGrid[y][x] == 4)
                        if (ClosestPosition.y > Position.y)
                            Instantiate(DecorationPiece3x3.GetRandomGameObject(RandomSeed.NextDouble()), Position, Quaternion.identity, RoomTransform.transform);
                        else
                            Instantiate(DecorationPiece3x3Tall.GetRandomGameObject(RandomSeed.NextDouble()), Position, Quaternion.identity, RoomTransform.transform);
                }
                
            }

    }


    IslandPiece CreateStartPiece()
    {
        return Instantiate(StartPieces[RandomSeed.Next(0, StartPieces.Count)], Vector3.zero, Quaternion.identity, RoomTransform.transform);
    }

    List<IslandConnector> Connectors;
    IslandConnector CurrentConnector;
    IslandConnector Connector;
    IslandPiece RandomPiece;
    List<Collider2D> Collisions;
    void GeneratePath(IslandConnector.Direction Direction)
    {
        //1) Set start piece as starting location ----------------------------------------------------------------------------------------------------------------||
        CurrentPiece = StartPiece;

        //2) 1 in 3 chance to pick another starting position -----------------------------------------------------------------------------------------------------||
        if (RandomSeed.Next(0, 3) <= 0)
        {
            //search all unused connections in the direction I'm going
            //pick a random one as my start piece
            List<IslandPiece> RandomCurrentPiece = new List<IslandPiece>();
            foreach (IslandPiece i in Pieces)
                if (i.GetConnectorsDirection(Direction, false).Count > 0)
                    RandomCurrentPiece.Add(i);
            CurrentPiece = RandomCurrentPiece[RandomSeed.Next(0, RandomCurrentPiece.Count)];
        }

        // 3) Set how many steps (islands to place) --------------------------------------------------------------------------------------------------------------||
        int Steps = RandomSeed.Next(2, 4);


        while (--Steps >= 0)
        {
            //4) Get the current (last placed) island's connection for this direction. Accept connections in a different direction if necessary. -----------------||
            Connectors = CurrentPiece.GetConnectorsDirection(Direction, true);

            if (Connectors == null || Connectors.Count <= 0) return;// there are no connections so end function.

            //5) Mark the new connection as used -----------------------------------------------------------------------------------------------------------------||
            Connector = Connectors[RandomSeed.Next(0, Connectors.Count)];
            CurrentPiece.MarkConnectorAsUsed(Connector);
            PrevPiece = CurrentPiece;

            //6) If this is the final step, check to see if you can place a door. If not, continue to place islands ----------------------------------------------||
            if (Steps == 0)
            {
                if (Connector.MyDirection == Direction)
                {
                    CurrentPiece = Instantiate(GetDirectionDoor(Direction), Vector3.zero, Quaternion.identity, RoomTransform.transform);
                    PositionIsland();
                    Pieces.Add(CurrentPiece);
                }
                else ++Steps; //The connection direction I need isn't available, so add another step to place another island until it is
            }

            //7) Pick a new island with the correct required connector  ------------------------------------------------------------------------------------------||
            //8) Make sure island doesn't overlap with existing pieces  ------------------------------------------------------------------------------------------||
            ///////
            if (Steps > 0)
            {
                RandomPiece = GetIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                PositionIsland();

                CurrentPiece.Collider.usedByComposite = false;

                while (true)
                {
                    Collisions = new List<Collider2D>();

                    Physics2D.SyncTransforms();

                    if (CurrentPiece.Collider.OverlapCollider(new ContactFilter2D(), Collisions) > 0)
                    {
                        UnityEngine.Debug.Log(CurrentPiece.gameObject.name + "   >>>>>>>  >>>>>>>  >>>>>>>  >>>>>>>  COLLIDED TRY AGAIN!");

                        Destroy(CurrentPiece.gameObject);

                        RandomPiece = GetIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                        CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                        PositionIsland();
                    }
                    else break;
                }

                Pieces.Add(CurrentPiece);

            }

            CurrentPiece.MarkConnectorAsUsed(CurrentConnector);
        }
    }

    void PositionIsland()
    {
        Connectors = CurrentPiece.GetConnectorsDirection(GetOppositeDirection(Connector.MyDirection), false);
        CurrentConnector = Connectors[RandomSeed.Next(0, Connectors.Count)];
        CurrentPiece.transform.position = Connector.transform.position - CurrentConnector.transform.position;
    }

    IslandPiece GetDirectionDoor(IslandConnector.Direction Direction)
    {
        switch (Direction)
        {
            case IslandConnector.Direction.North: return NorthIsland;
            case IslandConnector.Direction.East: return EastIsland;
            case IslandConnector.Direction.South: return SouthIsland;
            case IslandConnector.Direction.West: return WestIsland;
        }
        return null;
        
    }

    IslandConnector.Direction GetOppositeDirection(IslandConnector.Direction Direction)
    {
        switch (Direction)
        {
            case IslandConnector.Direction.North: return IslandConnector.Direction.South;
            case IslandConnector.Direction.East: return IslandConnector.Direction.West;
            case IslandConnector.Direction.South: return IslandConnector.Direction.North;
            case IslandConnector.Direction.West: return IslandConnector.Direction.East;
        }
        return IslandConnector.Direction.North;
    }

    [Button("Clear Prefabs")]
    void ClearPrefabs()
    {
        int i = -1;
        while (++i < RoomTransform.transform.childCount)
            Destroy(RoomTransform.transform.GetChild(i).gameObject);
        Physics2D.SyncTransforms();
        RoomTransform.GenerateGeometry();

    }

}

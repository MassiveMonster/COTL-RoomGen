using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Diagnostics;
using UnityEngine.U2D;
public class GenerateRoom : MonoBehaviour
{
    public SpriteShape SpriteShape;
    public SpriteShapeController spriteShapeController;

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

    List<IslandPiece> NorthIslandPieces;
    List<IslandPiece> EastIslandPieces;
    List<IslandPiece> SouthIslandPieces;
    List<IslandPiece> WestIslandPieces;

    List<IslandPiece> NorthIslandEncounterPieces;
    List<IslandPiece> EastIslandEncounterPieces;
    List<IslandPiece> SouthIslandEncounterPieces;
    List<IslandPiece> WestIslandEncounterPieces;

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

    bool Testing = false;
    public void GenerateRandomSeedTest()
    {
        Testing = true;
        GenerateRandomSeed();
        Testing = false;
    }

    [Button("Generate Random Seed")]
    void GenerateRandomSeed()
    {
        Seed = Random.Range(0, int.MaxValue);
        PreviousSeeds.Add(Seed);
        Generate();
    }

    List<RoomPath> Paths;
    public class RoomPath
    {
        public IslandConnector.Direction Direction;
        public int Encounters;
        public bool Door;
        public RoomPath(IslandConnector.Direction Direction, bool Door)
        {
            this.Direction = Direction;
            this.Door = Door;
        }
    }

    Stopwatch stopwatch = new Stopwatch();
    float PrevTime;
    [Button("GENERATE!", ButtonSizes.Gigantic)]
    void Generate()
    {
        ClearPrefabs();
        Pieces = new List<IslandPiece>();

        RandomSeed = new System.Random(Seed);

        StartPiece = CreateStartPiece();
        Pieces.Add(StartPiece);

        CollateLists();

        CreatePaths();

        PlaceDoors();

        CompositeColliders();

       // Physics2D.SyncTransforms();

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
        RoomTransform.GenerateGeometry();
        Physics2D.SyncTransforms();


        PlaceDecorations();

        PlaceNoiseDecorations();

        SpawnDecorations();

        CreateSpriteShape();

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Outlines;
        RoomTransform.GenerateGeometry();

        if (Testing)
        {
            RoomTransform.enabled = false;
            Physics2D.SyncTransforms();
        }
    }

    void CreatePaths()
    {
        Paths = new List<RoomPath>();
        if (North) Paths.Add(new RoomPath(IslandConnector.Direction.North, true));
        if (East) Paths.Add(new RoomPath(IslandConnector.Direction.East, true));
        if (South) Paths.Add(new RoomPath(IslandConnector.Direction.South, true));
        if (West) Paths.Add(new RoomPath(IslandConnector.Direction.West, true));

        int TotalEncounters = RandomSeed.Next(1, 5);

        if (Paths.Count <= 2 && RandomSeed.NextDouble() < 0.5f)
            Paths.Add(new RoomPath(PathGetUnusedDirection(), false));

        while (--TotalEncounters > 0)
            ++Paths[RandomSeed.Next(0, Paths.Count)].Encounters;

        foreach (RoomPath Path in Paths)
            GeneratePath(Path);
    }

    IslandConnector.Direction PathGetUnusedDirection()
    {
        List<IslandConnector.Direction> UnusedDirections = new List<IslandConnector.Direction> { IslandConnector.Direction.North, IslandConnector.Direction.East, IslandConnector.Direction.South, IslandConnector.Direction.West };
        foreach (RoomPath r in Paths)
            UnusedDirections.Remove(r.Direction);
        return UnusedDirections[RandomSeed.Next(0, UnusedDirections.Count)];
    }

    void CompositeColliders()
    {
        foreach (IslandPiece i in Pieces)
            i.Collider.usedByComposite = true;
    }

    [Button("Create SpriteShape")]
    void CreateSpriteShape()
    {
        spriteShapeController.spriteShape = SpriteShape;
        spriteShapeController.spline.Clear();

        Vector2[] points = new Vector2[RoomTransform.GetPathPointCount(0)];
        RoomTransform.GetPath(0, points);
        int p = 0;
        spriteShapeController.spline.InsertPointAt(0, points[0]);
        while (++p < points.Length)
        {
            if ( Vector2.Distance(RoomTransform.transform.TransformPoint(points[p]), RoomTransform.transform.TransformPoint(points[p - 1])) > 0.1f)
                spriteShapeController.spline.InsertPointAt(spriteShapeController.spline.GetPointCount() - 1, RoomTransform.transform.TransformPoint( points[p] ));
        }
    }

    [Button("Combine SpriteShapes")]
    void CombineSpriteShapes()
    {
        List<SpriteShapeController> spriteShapeControllers = new List< SpriteShapeController > (GetComponentsInChildren<SpriteShapeController>());
        SpriteShapeController StartPieceSSController = StartPiece.GetComponentInChildren<SpriteShapeController>();
        spriteShapeControllers.Remove(StartPieceSSController);

        while (spriteShapeControllers.Count > 0)
        {

            //find closest to StartPieceSpriteShapeController
            SpriteShapeController Closest = null;
            float Distance = float.MaxValue;
            foreach (SpriteShapeController s in spriteShapeControllers)
            {
                //todo search through points rather that sprite shapes
                float CheckDist = Vector2.Distance(s.transform.position, StartPieceSSController.transform.position);
                if (CheckDist < Distance)
                {
                    Closest = s;
                    Distance = CheckDist;
                }
            }

            int StartClosestIndex= 0;
            int ClosestIndex = 0;
            Distance = float.MaxValue;

            int i = -1;
            while (++i < StartPieceSSController.spline.GetPointCount())
            {
                int j = -1;
                while (++j < Closest.spline.GetPointCount())
                {
                    UnityEngine.Debug.Log(StartPieceSSController.transform.TransformPoint(StartPieceSSController.spline.GetPosition(i)) + "  " +  Closest.transform.TransformPoint(Closest.spline.GetPosition(j)));
                    float CheckDist = Vector2.Distance(StartPieceSSController.transform.TransformPoint(StartPieceSSController.spline.GetPosition(i)), Closest.transform.TransformPoint(Closest.spline.GetPosition(j)));
                    if (CheckDist < Distance)
                    {
                        Distance = CheckDist;
                        StartClosestIndex = i;
                        ClosestIndex = j;
                    }
                }
            }

            while (ClosestIndex < Closest.spline.GetPointCount() )
            {
                StartPieceSSController.spline.InsertPointAt(StartClosestIndex, Closest.transform.TransformPoint(Closest.spline.GetPosition(ClosestIndex)));

                ++ClosestIndex;
            }

            int k = 0;
            while (k < ClosestIndex - 1)
            {
                StartPieceSSController.spline.InsertPointAt(StartClosestIndex, Closest.transform.TransformPoint(Closest.spline.GetPosition(k)));
                ++k;
            }

            UnityEngine.Debug.Log("AA " + spriteShapeControllers.Count);
            Closest.enabled = false;
            spriteShapeControllers.Remove(Closest);
        }

        //find 2 closest points of ClosestSpriteShapeController and StartPieceSpriteShapeController
        //Insert(ClosestSSController's point to



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

        NorthIslandEncounterPieces = new List<IslandPiece>();
        EastIslandEncounterPieces = new List<IslandPiece>();
        SouthIslandEncounterPieces = new List<IslandPiece>();
        WestIslandEncounterPieces = new List<IslandPiece>();

        foreach (IslandPiece i in StartPieces)
        {
            if (i.GetConnectorsDirection(IslandConnector.Direction.North, false).Count > 0) NorthIslandEncounterPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.East, false).Count > 0) EastIslandEncounterPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.South, false).Count > 0) SouthIslandEncounterPieces.Add(i);
            if (i.GetConnectorsDirection(IslandConnector.Direction.West, false).Count > 0) WestIslandEncounterPieces.Add(i);
        }

    }

    IslandPiece GetIslandListByDirection(IslandConnector.Direction Direction)
    {
        switch (Direction)
        {
            case IslandConnector.Direction.North: return NorthIslandPieces[RandomSeed.Next(0, NorthIslandPieces.Count)];
            case IslandConnector.Direction.East: return EastIslandPieces[RandomSeed.Next(0, EastIslandPieces.Count)];
            case IslandConnector.Direction.South: return SouthIslandPieces[RandomSeed.Next(0, SouthIslandPieces.Count)];
            case IslandConnector.Direction.West: return WestIslandPieces[RandomSeed.Next(0, WestIslandPieces.Count)];
        }
        return null;
    }

    IslandPiece GetEncounterIslandListByDirection(IslandConnector.Direction Direction)
    {
        switch (Direction)
        {
            case IslandConnector.Direction.North: return NorthIslandEncounterPieces[RandomSeed.Next(0, NorthIslandEncounterPieces.Count)];
            case IslandConnector.Direction.East: return EastIslandEncounterPieces[RandomSeed.Next(0, EastIslandEncounterPieces.Count)];
            case IslandConnector.Direction.South: return SouthIslandEncounterPieces[RandomSeed.Next(0, SouthIslandEncounterPieces.Count)];
            case IslandConnector.Direction.West: return WestIslandEncounterPieces[RandomSeed.Next(0, WestIslandEncounterPieces.Count)];
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

    List<List<int>> DecorationGrid;
    int DecorationGridWidth;
    int DecorationGridHeight;
    [Button("Generate Decorations")]
    void PlaceDecorations()
    {
        DecorationGrid = new List<List<int>>();
        DecorationGridWidth = (int)(Mathf.Max(RoomTransform.bounds.size.x, RoomTransform.bounds.size.y));
        DecorationGridHeight = DecorationGridWidth;
        for (int y = -DecorationGridHeight; y < DecorationGridHeight; y += Scale)
        {
            List<int> Row = new List<int>();
            for (int x = -DecorationGridWidth; x < DecorationGridWidth; x += Scale)
            {
                if (RoomTransform.ClosestPoint(new Vector2(x, y - (Scale * 0.5f))) != new Vector2(x, y - (Scale * 0.5f)) && RoomTransform.ClosestPoint(new Vector2(x, y + (Scale * 0.5f))) != new Vector2(x, y + (Scale * 0.5f)))
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
                            if (DecorationGrid[x + i][y + j] == 0)
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
                        if (DecorationGrid[x][y] != 1 || (x + i >= DecorationGrid.Count) || (y + j >= DecorationGrid.Count) || DecorationGrid[x + i][y + j] != 1)
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


        
       

    }

    void PlaceNoiseDecorations()
    {
        /*
         * float NoiseScale = 10f;
                    float Noise = Mathf.PerlinNoise(((float)x / (float)DecorationGridWidth) * NoiseScale, ((float)y / (float)DecorationGridHeight) * NoiseScale);
                    UnityEngine.Debug.Log(Noise);
                    if (Noise > 0.5f)
                        DecorationGrid[y][x] = 0;

        */
    }

    void SpawnDecorations()
    { 
        Vector3 Position;
        Vector3 ClosestPosition;
        for (int y = 0; y < DecorationGrid.Count; y++)
            for (int x = 0; x < DecorationGrid.Count; x++)
            {
                Position = new Vector3((x * Scale) - DecorationGridWidth, (y * Scale) - DecorationGridHeight);
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

    void PlaceDoors()
    {
        foreach (RoomPath Path in Paths)
        {
            if (!Path.Door) continue;

            List<IslandConnector> RandomCurrentPiece = new List<IslandConnector>();
            foreach (IslandPiece i in Pieces)
            {
                List<IslandConnector> Connectors = i.GetConnectorsDirection(Path.Direction, false);
                foreach (IslandConnector c in Connectors)
                    RandomCurrentPiece.Add(c);

            }

            float Distance = float.MaxValue;
            IslandConnector ClosestConnector = null;
            switch (Path.Direction)
            {
                case IslandConnector.Direction.North:
                    Distance = -float.MaxValue;
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.y > Distance)
                        {
                            Distance = c.transform.position.y;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.East:
                    Distance = -float.MaxValue;
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.x > Distance)
                        {
                            Distance = c.transform.position.x;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.South:
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.y < Distance)
                        {
                            Distance = c.transform.position.y;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.West:
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.x < Distance)
                        {
                            Distance = c.transform.position.x;
                            ClosestConnector = c;
                        }
                    }
                    break;
            }

            if (ClosestConnector != null)
            {
                Connector = ClosestConnector;//.transform.parent.GetComponent<IslandPiece>();
                CurrentPiece = Instantiate(GetDirectionDoor(Path.Direction), Vector3.zero, Quaternion.identity, RoomTransform.transform);
                PositionIsland();
            }
            else UnityEngine.Debug.Log("NO PLACE TO PUT DOOR!");
        }
    }

    List<IslandConnector> Connectors;
    IslandConnector CurrentConnector;
    IslandConnector Connector;
    IslandPiece RandomPiece;
    List<Collider2D> Collisions;
    void GeneratePath(RoomPath Path)
    {
        //1) Set start piece as starting location ----------------------------------------------------------------------------------------------------------------||
        CurrentPiece = StartPiece;

        //todo: Re make this to only get usable connectors -> too often 'impossible' connectors are chosen.
        //2) 1 in 3 chance to pick another starting position -----------------------------------------------------------------------------------------------------||
        if (true)//RandomSeed.Next(0, 3) <= 0)
        {
            //search all unused connections in the direction I'm going
            //pick a random one as my start piece
            List<IslandConnector> RandomCurrentPiece = new List<IslandConnector>();
            foreach (IslandPiece i in Pieces)
            {
                List<IslandConnector> Connectors = i.GetConnectorsDirection(Path.Direction, false);
                foreach (IslandConnector c in Connectors)
                    RandomCurrentPiece.Add(c);

            }

            float Distance = float.MaxValue;
            IslandConnector ClosestConnector = null;
            switch (Path.Direction)
            {
                case IslandConnector.Direction.North:
                    Distance = -float.MaxValue;
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.y > Distance)
                        {
                            Distance = c.transform.position.y;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.East:
                    Distance = -float.MaxValue;
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.x > Distance)
                        {
                            Distance = c.transform.position.x;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.South:
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.y < Distance)
                        {
                            Distance = c.transform.position.y;
                            ClosestConnector = c;
                        }
                    }
                    break;

                case IslandConnector.Direction.West:
                    foreach (IslandConnector c in RandomCurrentPiece)
                    {
                        if (c.transform.position.x < Distance)
                        {
                            Distance = c.transform.position.x;
                            ClosestConnector = c;
                        }
                    }
                    break;
            }

            if (ClosestConnector != null)
                CurrentPiece = ClosestConnector.transform.parent.GetComponent<IslandPiece>();


        }


        // 3) Set how many steps (islands to place) --------------------------------------------------------------------------------------------------------------||
        int Steps = RandomSeed.Next(2, 4);
        bool PrevEncounter = true;

        while (--Steps >= 0)
        {
            //4) Get the current (last placed) island's connection for this direction. Accept connections in a different direction if necessary. -----------------||
            Connectors = CurrentPiece.GetConnectorsDirection(Path.Direction, true);

            if (Connectors == null || Connectors.Count <= 0) return;// there are no connections so end function.

            //5) Mark the new connection as used -----------------------------------------------------------------------------------------------------------------||
            Connector = Connectors[RandomSeed.Next(0, Connectors.Count)];
            if (Steps > 0)
                CurrentPiece.MarkConnectorAsUsed(Connector);
            PrevPiece = CurrentPiece;

            //6) If this is the final step, check to see if you can place a door. If not, continue to place islands ----------------------------------------------||
            if (Steps == 0)
            {
                if (Connector.MyDirection == Path.Direction)
                {

                    if (!Path.Door)
                    {
                        RandomPiece = GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                        CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                        PositionIsland();
                        CurrentPiece.Collider.usedByComposite = false;
                        while (true)
                        {
                            Collisions = new List<Collider2D>();
                            Physics2D.SyncTransforms();
                            if (CurrentPiece.Collider.OverlapCollider(new ContactFilter2D(), Collisions) > 0)
                            {
                                UnityEngine.Debug.Log("COLLISION! " + CurrentPiece.transform.gameObject.name);
                                Destroy(CurrentPiece.gameObject);
                                RandomPiece = GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                                CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                                PositionIsland();
                            }
                            else break;
                        }
                    }
                   
                    /*
                    if (Path.Door)
                    {
                        CurrentPiece = Instantiate(GetDirectionDoor(Path.Direction), Vector3.zero, Quaternion.identity, RoomTransform.transform);
                    }
                    else
                    {
                        bool Collided = PlaceIslandAndCheckCollision(GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection)));
                        while (Collided)
                            Collided = PlaceIslandAndCheckCollision(GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection)));
                    }
                   
                    PositionIsland();
                     */
        //  Pieces.Add(CurrentPiece);
    }
                else ++Steps; //The connection direction I need isn't available, so add another step to place another island until it is
            }

            //7) Pick a new island with the correct required connector  ------------------------------------------------------------------------------------------||
            //8) Make sure island doesn't overlap with existing pieces  ------------------------------------------------------------------------------------------||
            ///////
            bool DoEncounter = (Path.Encounters > 0  && !PrevEncounter);
            PrevEncounter = DoEncounter;
            if (Steps > 0)
            {
                RandomPiece = DoEncounter ? GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection)) : GetIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                PositionIsland();
                CurrentPiece.Collider.usedByComposite = false;
                while (true)
                {
                    Collisions = new List<Collider2D>();
                    Physics2D.SyncTransforms();
                    if (CurrentPiece.Collider.OverlapCollider(new ContactFilter2D(), Collisions) > 0)
                    {
                     //   UnityEngine.Debug.Log("COLLISION! " + CurrentPiece.transform.gameObject.name);
                        Destroy(CurrentPiece.gameObject);
                        RandomPiece = DoEncounter ? GetEncounterIslandListByDirection(GetOppositeDirection(Connector.MyDirection)) : GetIslandListByDirection(GetOppositeDirection(Connector.MyDirection));
                        CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                        PositionIsland();
                    }
                    else break;
                }
                CurrentPiece.MarkConnectorAsUsed(CurrentConnector);
            }

            Pieces.Add(CurrentPiece);
        }
    }

    bool PlaceIslandAndCheckCollision(IslandPiece Prefab)
    {
        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
        CurrentPiece = Instantiate(Prefab, Vector3.zero, Quaternion.identity, transform);
        PositionIsland();
        CurrentPiece.Collider.usedByComposite = false;
        Physics2D.SyncTransforms();

        foreach (Vector2 p in CurrentPiece.Collider.points)
        {
            //if (RoomTransform.OverlapPoint(p + (Vector2)CurrentPiece.transform.position))
            if (RoomTransform.ClosestPoint(p) == p)
            {
                UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>     OVERLAP!!!");
                Destroy(CurrentPiece.gameObject);
               // break;
                return true;
            }
        }
        CurrentPiece.transform.parent = RoomTransform.transform;
        return false;


        /*
        UnityEngine.Debug.Log(Prefab.gameObject.name);
        CurrentPiece = Instantiate(Prefab, Vector3.zero, Quaternion.identity, RoomTransform.transform);
        PositionIsland();

        CurrentPiece.Collider.usedByComposite = false;

        Physics2D.SyncTransforms();

        Collisions = new List<Collider2D>();
        if (CurrentPiece.Collider.OverlapCollider(new ContactFilter2D(), Collisions) > 0)
        {
           
            UnityEngine.Debug.Log(CurrentPiece.name +  "   "  +Collisions[0].transform.parent.name);
            Destroy(CurrentPiece.gameObject);
            return true;
        } 

        Pieces.Add(CurrentPiece);
        return false;
        */
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

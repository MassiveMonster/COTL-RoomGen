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

    public int AngleVariation = 180;
    public int Seed = 0;
    System.Random RandomSeed;

    public IslandPiece NorthIsland;
    public IslandPiece EastIsland;
    public IslandPiece SouthIsland;
    public IslandPiece WestIsland;

    public List<IslandPiece> StartPieces = new List<IslandPiece>();
    public List<IslandPiece> IslandPieces = new List<IslandPiece>();
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
        UnityEngine.Debug.Log("Begin Generation!");

        stopwatch.Start();

        ClearPrefabs();
        Pieces = new List<IslandPiece>();

        RandomSeed = new System.Random(Seed);

        StartPiece = CreateStartPiece();
        Pieces.Add(StartPiece);

        if (North)
            GeneratePath(IslandConnector.Direction.North);
        if (East)
            GeneratePath(IslandConnector.Direction.East);
        if (South)
            GeneratePath(IslandConnector.Direction.South);
        if (West)
            GeneratePath(IslandConnector.Direction.West);



        Physics2D.SyncTransforms();

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Polygons;
        RoomTransform.GenerateGeometry();
        Physics2D.SyncTransforms();

        PrevTime = (stopwatch.ElapsedMilliseconds - PrevTime);
        UnityEngine.Debug.Log("Create Rooms: " + stopwatch.ElapsedMilliseconds + "ms");

        PlaceDecorations();

        UnityEngine.Debug.Log("Place Decorations: " + stopwatch.ElapsedMilliseconds + "ms");

        RoomTransform.geometryType = CompositeCollider2D.GeometryType.Outlines;
        RoomTransform.GenerateGeometry();

        stopwatch.Stop();
        UnityEngine.Debug.Log("Function took: " + stopwatch.ElapsedMilliseconds + "ms");

    }

    [System.Serializable]
    public class ListOfDecorations
    {
        public List<DecorationAndProbability> DecorationAndProabilies;
        int[] Weights;
        int Index;
        public GameObject GetRandomGameObject()
        {
            Weights = new int[DecorationAndProabilies.Count];
            int i = -1;
            while (++i < DecorationAndProabilies.Count)
                Weights[i] = DecorationAndProabilies[i].Probability;
            UnityEngine.Debug.Log(Weights.Length);

            Index = Utils.GetRandomWeightedIndex(Weights);
            UnityEngine.Debug.Log(Weights.Length);

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
                        Instantiate(DecorationPiece.GetRandomGameObject(), Position , Quaternion.identity, RoomTransform.transform);

                    if (DecorationGrid[y][x] == 3)
                        Instantiate(DecorationPiece2x2.GetRandomGameObject(), Position, Quaternion.identity, RoomTransform.transform);

                    if (DecorationGrid[y][x] == 4)
                        if (ClosestPosition.y < Position.y)
                            Instantiate(DecorationPiece3x3.GetRandomGameObject(), Position, Quaternion.identity, RoomTransform.transform);
                        else
                            Instantiate(DecorationPiece3x3Tall.GetRandomGameObject(), Position, Quaternion.identity, RoomTransform.transform);
                }
                
            }

    }


    IslandPiece CreateStartPiece()
    {
        return Instantiate(StartPieces[RandomSeed.Next(0, StartPieces.Count)], Vector3.zero, Quaternion.identity, RoomTransform.transform);
    }

    List<IslandConnector> Connectors;
    void GeneratePath(IslandConnector.Direction Direction)
    {
        CurrentPiece = StartPiece;

        //search all unused connections in the direction I'm going
        //pick a random one as my start piece
        if (RandomSeed.Next(0, 3) <= 0)
        {
            List<IslandPiece> RandomCurrentPiece = new List<IslandPiece>();
            foreach (IslandPiece i in Pieces)
                if (i.GetConnectorsDirection(Direction, false).Count > 0)
                    RandomCurrentPiece.Add(i);
            CurrentPiece = RandomCurrentPiece[RandomSeed.Next(0, RandomCurrentPiece.Count)];
        }
        
      //  int Steps = RandomSeed.Next(1, 5);
        int Steps = RandomSeed.Next(2, 4);

        while (--Steps >= 0)
        {
            Connectors = CurrentPiece.GetConnectorsDirection(Direction, true);
            if (Connectors == null || Connectors.Count <= 0) return;// there are no connections so end function.
            IslandConnector Connector = Connectors[RandomSeed.Next(0, Connectors.Count)];
            CurrentPiece.ActivateConnector(Connector);
            PrevPiece = CurrentPiece;

            if (Steps == 0)
            {
                //make sure the correct connector direction is available for the door, otherwise continue to place pieces!
                if (Connector.MyDirection == Direction)
                {
                    CurrentPiece = Instantiate(GetDirectionDoor(Direction), Vector3.zero, Quaternion.identity, RoomTransform.transform);
                    Pieces.Add(CurrentPiece);
                }
                else ++Steps;
            }

            if (Steps > 0)
            {
                //loop through random pieces to make sure they have the required connection piece
                IslandPiece RandomPiece = IslandPieces[RandomSeed.Next(0, IslandPieces.Count)];
                while (RandomPiece.GetConnectorsDirection(GetOppositeDirection(Connector.MyDirection), false).Count <= 0)
                {
                    RandomPiece = IslandPieces[RandomSeed.Next(0, IslandPieces.Count)];
                }
                //Place New Piece
                CurrentPiece = Instantiate(RandomPiece, Vector3.zero, Quaternion.identity, RoomTransform.transform);
                Pieces.Add(CurrentPiece);
            }
            

            //Find Position For New Piece From Connectors
            Connectors = CurrentPiece.GetConnectorsDirection(GetOppositeDirection(Connector.MyDirection), false);
            if (Connectors == null || Connectors.Count <= 0)
            {
                UnityEngine.Debug.Log("UNACCEPTABLE!");
                //to do place new pieces until acceptable
                Destroy(CurrentPiece);
                CurrentPiece = PrevPiece;
                return;
            }

            IslandConnector CurrentConnector = Connectors[RandomSeed.Next(0, Connectors.Count)];
            CurrentConnector.Active = true;
            CurrentPiece.ActivateConnector(CurrentConnector);

            CurrentPiece.transform.position = Connector.transform.position - CurrentConnector.transform.position;
        }

       

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

    void ClearPrefabs()
    {
        int i = -1;
        while (++i < RoomTransform.transform.childCount)
            Destroy(RoomTransform.transform.GetChild(i).gameObject);
    }

}

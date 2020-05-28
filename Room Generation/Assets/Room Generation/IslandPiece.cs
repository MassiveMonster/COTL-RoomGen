using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.U2D;
namespace MMRoomGeneration
{
    public class IslandPiece : MonoBehaviour
    {
        public IslandConnector[] Connectors;
        public List<IslandConnector> NorthConnectors = new List<IslandConnector>();
        public List<IslandConnector> EastConnectors = new List<IslandConnector>();
        public List<IslandConnector> SouthConnectors = new List<IslandConnector>();
        public List<IslandConnector> WestConnectors = new List<IslandConnector>();

        public PolygonCollider2D Collider
        {
            get
            {
                if (_Collider == null) _Collider = GetComponentInChildren<PolygonCollider2D>();
                if (_Collider == null) _Collider = GetComponent<PolygonCollider2D>();
                return _Collider;
            }
        }
        public PolygonCollider2D _Collider;


        [Button("Get Connectors", ButtonSizes.Gigantic)]
        void GetConnectors()
        {
            Connectors = GetComponentsInChildren<IslandConnector>();
            NorthConnectors.Clear();
            EastConnectors.Clear();
            SouthConnectors.Clear();
            WestConnectors.Clear();
            foreach (IslandConnector i in Connectors)
            {
                switch (i.MyDirection)
                {
                    case IslandConnector.Direction.North:
                        NorthConnectors.Add(i); break;
                    case IslandConnector.Direction.East:
                        EastConnectors.Add(i); break;
                    case IslandConnector.Direction.South:
                        SouthConnectors.Add(i); break;
                    case IslandConnector.Direction.West:
                        WestConnectors.Add(i); break;
                }
            }
        }

        GameObject SpriteShapeGO = null;
        [Button("Set SpriteShape")]
        public void SetSpriteShape()
        {
            GenerateRoom generateRoom = GameObject.FindObjectOfType<GenerateRoom>();
            if (SpriteShapeGO == null)
            {
                SpriteShapeGO = new GameObject();
                SpriteShapeGO.transform.parent = transform;
                SpriteShapeGO.transform.localPosition = Vector3.zero;
                SpriteShapeGO.name = "Sprite Shape";
            }

            UnityEngine.U2D.SpriteShapeController s = SpriteShapeGO.AddComponent<UnityEngine.U2D.SpriteShapeController>();
            s.spriteShape = generateRoom.SpriteShape;
            s.spline.Clear();
            int p = -1;
            while (++p < Collider.points.Length)
                s.spline.InsertPointAt(p, Collider.points[p]);
            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderer)
                sr.enabled = false;
        }


        [Title("Random Sprite Shape To Place")]
        public ListOfGameObjectAndProbability SpriteShapes = new ListOfGameObjectAndProbability();

        [Title("Encounter Prefab To Place")]
        public ListOfGameObjectAndProbability Encounters = new ListOfGameObjectAndProbability();

        [System.Serializable]
        public class GameObjectAndProbability
        {
            public GameObject gameObject;
            [Range(0,100)]
            public int Probability = 50;
        }

        [System.Serializable]
        public class ListOfGameObjectAndProbability
        {
            public List<GameObjectAndProbability> ObjectList = new List<GameObjectAndProbability>();
            int[] Weights;
            int Index;
            public GameObject GetRandomGameObject(double RandomSeed)
            {
                Weights = new int[ObjectList.Count];
                int i = -1;
                while (++i < ObjectList.Count)
                    Weights[i] = ObjectList[i].Probability;

                Index = GenerateRoom.GetRandomWeightedIndex(Weights, RandomSeed);

                return ObjectList[Index].gameObject;
            }
        }
        


        public void InitIsland(System.Random Seed, SpriteShape SecondarySpriteShape)
        {
            
            HideSprites();
            if (SpriteShapes.ObjectList.Count > 0)
            {
                GameObject g = Instantiate(SpriteShapes.GetRandomGameObject(Seed.NextDouble()), transform);
                SpriteShapeController s = g.GetComponent<SpriteShapeController>();
                if (s != null)
                    s.spriteShape = SecondarySpriteShape;
            }

            if (Encounters.ObjectList.Count > 0)
                Instantiate(Encounters.GetRandomGameObject(Seed.NextDouble()), transform);

        }

        public void HideSprites()
        {
            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderer)
                sr.enabled = false;
        }

        [Button("Show Sprites")]
        void ShowSprites()
        {
            if (SpriteShapeGO != null)
            {
                if (Application.isEditor)
                    DestroyImmediate(SpriteShapeGO);
                else
                    Destroy(SpriteShapeGO);
            }
            SpriteShapeGO = null;

            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderer)
                sr.enabled = true;
        }

        [Button("Round Colliders")]
        void RoundColliders()
        {
            List<Vector2> Points = new List<Vector2>();
            int i = -1;
            while (++i < Collider.points.Length)
            {
                float x = Mathf.Round(Collider.points[i].x * 2) / 2;
                float y = Mathf.Round(Collider.points[i].y * 2) / 2;
                Points.Add(new Vector2(x, y));
            }
            Collider.SetPath(0, Points);
        }

        public List<IslandConnector> GetConnectorsDirection(IslandConnector.Direction Direction, bool AcceptOthers)
        {
            switch (Direction)
            {
                case IslandConnector.Direction.North:
                    if (AcceptOthers && NorthConnectors.Count <= 0)
                        return GetRandomConnector();
                    return NorthConnectors;
                case IslandConnector.Direction.East:
                    if (AcceptOthers && EastConnectors.Count <= 0)
                        return GetRandomConnector();
                    return EastConnectors;
                case IslandConnector.Direction.South:
                    if (AcceptOthers && SouthConnectors.Count <= 0)
                        return GetRandomConnector();
                    return SouthConnectors;
                case IslandConnector.Direction.West:
                    if (AcceptOthers && WestConnectors.Count <= 0)
                        return GetRandomConnector();
                    return WestConnectors;
            }
            return null;
        }

        public void MarkConnectorAsUsed(IslandConnector islandConnector)
        {
            islandConnector.Active = true;
            NorthConnectors.Remove(islandConnector);
            EastConnectors.Remove(islandConnector);
            SouthConnectors.Remove(islandConnector);
            WestConnectors.Remove(islandConnector);
        }

        List<IslandConnector> ReturnConnectors;
        public List<IslandConnector> GetRandomConnector()
        {
            ReturnConnectors = new List<IslandConnector>();
            foreach (IslandConnector i in Connectors)
            {
                if (!i.Active) ReturnConnectors.Add(i);
            }
            return ReturnConnectors;
        }

    }

}
 
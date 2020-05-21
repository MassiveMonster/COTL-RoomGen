using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class IslandPiece : MonoBehaviour
{
   public IslandConnector[] Connectors;
   public List<IslandConnector> NorthConnectors = new List<IslandConnector>();
   public List<IslandConnector> EastConnectors = new List<IslandConnector>();
   public List<IslandConnector> SouthConnectors = new List<IslandConnector>();
   public List<IslandConnector> WestConnectors = new List<IslandConnector>();
    [Button("Get Connectors")]
   void GetConnectors()
    {
        Connectors = GetComponentsInChildren<IslandConnector>();
        NorthConnectors.Clear();
        EastConnectors.Clear();
        SouthConnectors.Clear();
        WestConnectors.Clear();
        foreach (IslandConnector i in Connectors)
        {
            i.Angle = Utils.GetAngle(transform.position, i.transform.position);
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



    List<IslandConnector> ReturnConnectors;
    public List<IslandConnector> GetConnectorsFromAngle(float Angle, float AngleVariation)
    {
        ReturnConnectors = new List<IslandConnector>();
        foreach (IslandConnector i in Connectors)
        {
            if (!i.Active && (Mathf.Abs(Angle - i.Angle) <  0 + AngleVariation || Mathf.Abs(Angle - i.Angle) > 360 - AngleVariation))
                ReturnConnectors.Add(i);
        }
        if (ReturnConnectors.Count <= 0)
        {
            foreach (IslandConnector i in Connectors)
                if (!i.Active)
                    ReturnConnectors.Add(i);
        }
        return ReturnConnectors;
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

    public void ActivateConnector(IslandConnector islandConnector)
    {
        islandConnector.Active = true;
        NorthConnectors.Remove(islandConnector);
        EastConnectors.Remove(islandConnector);
        SouthConnectors.Remove(islandConnector);
        WestConnectors.Remove(islandConnector);
    }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Diagnostics;

public class Test : MonoBehaviour
{
    public GenerateRoom GenerateRoomPrefab;
    int NumTest = 50;
    Stopwatch stopwatch = new Stopwatch();
    [Button("Test")]
    void DoTest()
    {
        stopwatch.Restart();
        int i = NumTest;
        while (--i > 0)
        {
            GenerateRoom g = Instantiate(GenerateRoomPrefab).GetComponent<GenerateRoom>();
            g.GenerateRandomSeedTest();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Time taken " + stopwatch.ElapsedMilliseconds + "ms");
    }
}

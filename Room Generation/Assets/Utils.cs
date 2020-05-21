using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {
    static Matrix4x4 matrix = Matrix4x4.identity;
    public static bool gizmos = true;

    static void SetColor(Color color)
    {
        if (gizmos && UnityEngine.Gizmos.color != color) UnityEngine.Gizmos.color = color;
    }

    public static void DrawLine(Vector3 a, Vector3 b, Color color)
    {
        SetColor(color);
        if (gizmos) UnityEngine.Gizmos.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b));
        else UnityEngine.Debug.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b), color);
    }

    public static void DrawCircleXY(Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = 2 * Mathf.PI, int steps = 40)
    {
        while (startAngle > endAngle) startAngle -= 2 * Mathf.PI;

        Vector3 prev = new Vector3(Mathf.Cos(startAngle) * radius, 0, Mathf.Sin(startAngle) * radius);
        for (int i = 0; i <= steps; i++)
        {
            Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, i / (float)steps)) * radius, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, i / (float)steps)) * radius, 0);
            DrawLine(center + prev, center + c, color);
            prev = c;
        }
    }

    public static void DrawCircleXZ(Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = 2 * Mathf.PI, int steps = 40)
    {
        while (startAngle > endAngle) startAngle -= 2 * Mathf.PI;

        Vector3 prev = new Vector3(Mathf.Cos(startAngle) * radius, 0, Mathf.Sin(startAngle) * radius);
        for (int i = 0; i <= steps; i++)
        {
            Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, i / (float)steps)) * radius, 0, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, i / (float)steps)) * radius);
            DrawLine(center + prev, center + c, color);
            prev = c;
        }
    }

    public static float GetAngle(Vector3 fromPosition, Vector3 toPosition)
    {
        return Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x) * Mathf.Rad2Deg;
    }

    public enum Direction { Up, Down, Left, Right};
    public static Direction GetAngleDirection(float Angle)
    {
        if (Angle >= -45 && Angle < 45)
            return Direction.Right;
        else if (Angle >= 45 && Angle < 135)
            return Direction.Up;
        else if (Angle >= 135 || Angle < -135)
            return Direction.Left;
        else if (Angle >= -135 && Angle < -45)
            return Direction.Down;

        return Direction.Right;
    }

    public static bool Between(float value, float min, float max)
    {
        return (value >= min && value < max);
    }

    public static int GetRandomWeightedIndex(int[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        int total = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] >= 0) total += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            if (weights[i] <= 0f) continue;

            s += (float)weights[i] / total;
            if (s >= r) return i;
        }

        return -1;
    }

    public static float SmoothAngle(float CurrentAngle, float TargetAngle, float Easing)
    {
        return CurrentAngle += ((Mathf.Atan2(Mathf.Sin((TargetAngle - CurrentAngle) * Mathf.Deg2Rad), Mathf.Cos((TargetAngle - CurrentAngle) * Mathf.Deg2Rad)) * Mathf.Rad2Deg) / Easing) * (Time.deltaTime * 60);

    }

    public static Vector3 RayToPosition(float Radius, Vector3 StartPosition, float Direction, float Distance, LayerMask layerToCheck)
    {
        Direction *= Mathf.Deg2Rad;
        Vector3 PointToCheck = StartPosition + new Vector3(Distance * Mathf.Cos(Direction), Distance * Mathf.Sin(Direction));
        RaycastHit2D[] Results = Physics2D.CircleCastAll(StartPosition + new Vector3(Radius * Mathf.Cos(Direction), Radius * Mathf.Sin(Direction)), Radius, Vector3.Normalize(PointToCheck - StartPosition), Distance, layerToCheck);
        if (Results.Length > 0)
            return (Vector3)Results[0].centroid;

        return PointToCheck;
    }

    public static bool WithinRange(float Value, float Min, float Max)
    {
        return (Value >= Min && Value <= Max);
    }

    public static bool WithinRange(float Value, int Min, int Max)
    {
        return (Value >= Min && Value <= Max);
    }

}

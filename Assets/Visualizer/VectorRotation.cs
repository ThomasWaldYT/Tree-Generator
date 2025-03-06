using UnityEngine;

public static class VectorRotation
{
    /// <summary>
    /// Rotates rotatingVector around stationaryVector by "degrees" degrees.
    /// </summary>
    /// <param name="stationaryVector">The vector to base the rotation off of.</param>
    /// <param name="rotatingVector">The vector to transform.</param>
    /// <param name="degrees">The amount of degrees to rotate by.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector3 RotateVector(Vector3 stationaryVector, Vector3 rotatingVector, float degrees)
    {
        float rotatingMagnitude = rotatingVector.magnitude;
        stationaryVector.Normalize();
        rotatingVector.Normalize();

        if (AngleBetween(stationaryVector, rotatingVector) == 0) return rotatingVector;

        Vector3 cross1 = Vector3.Cross(stationaryVector, rotatingVector).normalized;
        Vector3 cross2 = Vector3.Cross(cross1, stationaryVector).normalized;

        float angleToPlane = AngleBetween(rotatingVector, cross2);
        float adjacentLength = rotatingVector.magnitude * Mathf.Cos(angleToPlane * Mathf.Deg2Rad);

        Vector3 oppositeVector = stationaryVector * Mathf.Sin(angleToPlane * Mathf.Deg2Rad);
        Vector3 shadowVector = (Mathf.Cos(degrees * Mathf.Deg2Rad) * cross2 - Mathf.Sin(degrees * Mathf.Deg2Rad) * cross1)
                               * adjacentLength;

        float direction = Mathf.Sign(Vector3.Dot(rotatingVector, stationaryVector));
        Vector3 finalVector = (shadowVector + (oppositeVector * direction)) * rotatingMagnitude;

        return finalVector;
    }

    /// <summary>
    /// Gets the angle between two vectors in degrees.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    /// <returns>The angle in degrees.</returns>
    private static float AngleBetween(Vector3 v1, Vector3 v2)
    {
        v1.Normalize();
        v2.Normalize();
        float dotProduct = Mathf.Clamp(Vector3.Dot(v1, v2), -1, 1);
        return Mathf.Rad2Deg * Mathf.Acos(dotProduct);
    }
}
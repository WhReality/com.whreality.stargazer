using UnityEngine;

namespace WhReality.Stargazer.Helpers
{
    public static class SphereMath
    {
        public static float ArcDistance(Vector3 positionA, Vector3 positionB, float radius)
        {
            if (Vector3.SqrMagnitude(positionA - positionB) == 0) return 0f;

            return Mathf.Acos(Vector3.Dot(positionA.normalized, positionB.normalized)) * Mathf.Rad2Deg * Mathf.PI *
                radius / 180f;
        }
    }
}
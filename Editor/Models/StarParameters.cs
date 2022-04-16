using UnityEngine;

namespace WhReality.Stargazer.Models
{
    public class StarParameters
    {
        public StarParameters(Vector3 direction, float coreRadius, float haloHaloFalloff, Color coreColor,
            Color haloColor)
        {
            Direction = direction;
            CoreColor = coreColor;
            HaloColor = haloColor;
            HaloFalloff = haloHaloFalloff;
            CoreRadius = coreRadius;
        }

        public Vector3 Direction { get; set; }
        public Color CoreColor { get; set; }
        public float HaloFalloff { get; set; }
        public float CoreRadius { get; set; }
        public Color HaloColor { get; set; }

        public void Deconstruct(out Vector3 direction, out float coreRadius, out float haloFalloff, out Color coreColor,
            out Color haloColor)
        {
            direction = Direction;
            coreRadius = CoreRadius;
            haloFalloff = HaloFalloff;
            coreColor = CoreColor;
            haloColor = HaloColor;
        }
    }
}
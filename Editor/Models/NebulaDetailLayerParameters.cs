using UnityEngine;

namespace WhReality.Stargazer.Models
{
    public class NebulaDetailLayerParameters
    {
        public NebulaDetailLayerParameters(Color color, Vector3 offset, float frequency, int displaceSteps, int octaves)
        {
            Color = color;
            Offset = offset;
            Frequency = frequency;
            DisplaceSteps = displaceSteps;
            Octaves = octaves;
        }
        
        public Color Color { get; set; }
        public Vector3 Offset { get; set; }
        public float Frequency { get; set; }
        public int DisplaceSteps { get; set; }
        public int Octaves { get; set; }
        
        public void Deconstruct(out Vector3 offset, out float frequency, out Color color, out int displaceSteps,
            out int octaves)
        {
            offset = Offset;
            frequency = Frequency;
            color = Color;
            displaceSteps = DisplaceSteps;
            octaves = Octaves;
        }
    }
}
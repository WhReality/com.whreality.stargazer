// ReSharper disable InconsistentNaming
namespace WhReality.Stargazer.Models
{
    public class NebulaParameters
    {
        public NebulaParameters(float falloff, float density, NebulaDetailLayerParameters[] detailLayers,
            NebulaDetailLayerParameters shape)
        {
            Falloff = falloff;
            Density = density;
            DetailLayers = detailLayers;
            Shape = shape;
        }

        public float Falloff { get; set; }
        public float Density { get; set; }
        public NebulaDetailLayerParameters[] DetailLayers { get; set; }
        public NebulaDetailLayerParameters Shape { get; set; }

        public void Deconstruct(out float density, out float falloff, out NebulaDetailLayerParameters[] detailLayers,
            out NebulaDetailLayerParameters shape)
        {
            density = Density;
            falloff = Falloff;
            detailLayers = DetailLayers;
            shape = Shape;
        }
    }
}
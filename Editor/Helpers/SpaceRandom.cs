using System.Collections.Generic;
using UnityEngine;
using WhReality.Stargazer.Models;

namespace WhReality.Tools.Stargazer.Helpers
{
    public static class SpaceRandom
    {
        private static int _seed = 1234;
        
        public struct NebulaDetailParams
        {
            public float FrequencyMin;
            public float FrequencyMax;
            public int Octaves;
            public int Count;
            public Gradient Gradient;
        }

        public struct NebulaShapeParams
        {
            public int Octaves;
            public float Frequency;
            public int DisplaceSteps;
            public Vector3 Offset;
        }
        
        public static StarParameters[] RandomStars(int count, float haloFalloffMin,
            float haloFalloffMax, Gradient gradient, float coreSize)
        {
            Random.InitState(_seed);

            var starParameters = new List<StarParameters>();
            for (var i = 0; i < count; i++)
            {
                var position = RandomVector(-1.0f, 1.0f);
                var coreRadius = coreSize;
                var haloFalloff = Random.Range(haloFalloffMin, haloFalloffMax);
                var coreColor = Color.white;
                var haloColor = gradient?.Evaluate(Random.value) ?? RandomColor();

                starParameters.Add(new StarParameters(position, coreRadius, haloFalloff, coreColor, haloColor));
            }

            return starParameters.ToArray();
        }

        public static StarParameters[] RandomSuns(int count, int haloFalloffMin, int haloFalloffMax, Gradient color)
        {
            Random.InitState(_seed);

            var starParameters = new List<StarParameters>();
            for (var i = 0; i < count; i++)
            {
                var position = RandomVector(-1.0f, 1.0f);
                var coreRadius = 0.002f;
                var haloFalloff = Random.Range(haloFalloffMin, haloFalloffMax);
                var coreColor = Color.white;
                var haloColor = color?.Evaluate(Random.value) ?? RandomColor();

                starParameters.Add(new StarParameters(position, coreRadius, haloFalloff, coreColor, haloColor));
            }

            return starParameters.ToArray();
        }

        public static NebulaParameters RandomNebulas(float density, float falloff, NebulaDetailParams detail, NebulaShapeParams shape)
        {
            Random.InitState(_seed);
            var detailLayers = new NebulaDetailLayerParameters[detail.Count];
            var stepSize = 1 / (float) detailLayers.Length;
            var step = stepSize;
            for (var i = 0; i < detailLayers.Length; i++)
            {
                var time = i > 0 ? (i + 1) * stepSize : 0;
                var color = detail.Gradient?.Evaluate(time) ??
                            RandomColor();
                var offset = RandomVector(-1f, 1f);
                var frequency = Random.Range(detail.FrequencyMin, detail.FrequencyMax);
                var displaceSteps = Random.Range(1, 2);
                detailLayers[i] = new NebulaDetailLayerParameters(color, offset, frequency, displaceSteps, detail.Octaves);

                step += stepSize;
            }

            var s = new NebulaDetailLayerParameters(Color.white, shape.Offset,shape.Frequency, shape.DisplaceSteps,  shape.Octaves);
            return new NebulaParameters(falloff, density, detailLayers, s);
        }
        
        public static Color[] RandomPointStars(float density, float brightness, int width, int height, Color color)
        {
            Random.InitState(_seed);

            var starCount = (int) Mathf.Round(width * density);
            var data = new Color[width * height];
            for (var i = 0; i < data.Length; i++) data[i] = Color.black;

            for (var i = 0; i < starCount; i++)
            {
                var rX = (int) Mathf.Floor(Random.value * width);
                var rY = (int) Mathf.Floor(Random.value * height);
                var c = Random.Range(0.0f,1.0f) * brightness;
                data[rX + width * rY] = Color.Lerp(data[i] = Color.black, color, c);
            }

            return data;
        }

        public static void SetSeed(int seed)
        {
            _seed = seed;
        }

        private static Color RandomColor()
        {
            return new Color(Random.value, Random.value, Random.value);
        }

        private static Vector3 RandomVector(float min, float max)
        {
            return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
        }

   
    }
}
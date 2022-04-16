using UnityEngine;
using WhReality.Stargazer.Lib;
using WhReality.Tools.Stargazer.Services;

namespace WhReality.Stargazer.Helpers
{
    public class SpaceNoise
    {
        private readonly INoise _simplex;
        private readonly INoise _worley;

        public SpaceNoise(int seed, float freq)
        {
            _simplex = new SimplexNoise(seed, freq);
            _worley = new WorleyNoise(seed, 0.2f, 1f);
        }

        public float CalcPixel3D(Vector3 pos)
        {
            return _simplex.Sample3D(pos.x, pos.y, pos.z);
        }

        public float NormalNoise(Vector3 point, float frequency, float octaves, float lacunarity,
            float persistence)
        {
            var sum = NormalNoise(point * frequency);
            var amplitude = 1f;
            var range = 1f;
            for (var o = 1; o < octaves; o++)
            {
                frequency *= lacunarity;
                amplitude *= persistence;
                range += amplitude;
                sum += NormalNoise(point * frequency) * amplitude;
            }

            var result = sum / range;

            if (result < 0f || result > 1.0f)
                throw new NoiseCalculationException($"NormalNoise needs to be in range 0..1. Result: {result}");

            return result;
        }

        public float Worley(Vector3 point)
        {
            var result = _worley.Sample3D(point.x, point.y, point.z);
            return result;
        }

        public float Ridge(Vector3 point)
        {
            var e0 = 1 * -Mathf.Abs(CalcPixel3D(point * 1));
            var e1 = 0.5f * -Mathf.Abs(CalcPixel3D(point * 2)) * e0;
            var e2 = 0.25f * -Mathf.Abs(CalcPixel3D(point * 4)) * (e0 + e1);
            var ridgeNoise = (e0 + e1 + e2) / (1 + 0.5f + 0.25f);
            var result = ridgeNoise + 1.0f;
            if (result < 0 || result > 1.0f)
                throw new NoiseCalculationException($"RidgeNoise value needs to be between 0...1. Result: {result}");

            return result;
        }

        public float Displace(Vector3 point, float frequency, int steps)
        {
            var scale = Mathf.Pow(frequency, steps);
            var result = 0.0f;
            for (var i = 1; i < steps; i++)
            {
                result = CalcPixel3D(point * scale + new Vector3(result, result, result));
                scale += 0.6f;
            }

            return result;
        }

        private float NormalNoise(Vector3 point)
        {
            var sample = CalcPixel3D(point);
            var result = sample * 0.5f + 0.5f;
            if (result < 0 || result > 1.0f)
                throw new NoiseCalculationException($"NormalNoise value needs to be between 0...1. Result: {result}");
            return result;
        }
    }
}
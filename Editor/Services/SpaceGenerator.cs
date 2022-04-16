using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WhReality.Stargazer.Helpers;
using WhReality.Stargazer.Infrastructure;
using WhReality.Stargazer.Models;
using Debug = UnityEngine.Debug;

namespace WhReality.Tools.Stargazer.Services
{
    public class SpaceParameters
    {
        public SpaceParameters(int faceSize, Color[] pointStars, NebulaParameters nebula, StarParameters[] stars, StarParameters[] suns)
        {
            FaceSize = faceSize;
            PointStars = pointStars;
            Nebula = nebula;
            Stars = stars;
            Suns = suns;
        }

        public int FaceSize { get; set; }
        public Color[] PointStars { get; set; }
        public NebulaParameters Nebula { get; }
        public StarParameters[] Stars { get; set; }
        public StarParameters[] Suns { get; set; }

        public void Deconstruct(out int faceSize, out Color[] pointStars, out NebulaParameters nebula, out StarParameters[] stars, out StarParameters[] suns)
        {
            faceSize = FaceSize;
            pointStars = PointStars;
            nebula = Nebula;
            stars = Stars;
            suns = Suns;
        }
    }
    
    public class SpaceGenerator
    {
        private static readonly int[] KCubeXRemap = {2, 2, 0, 0, 0, 0};
        private static readonly int[] KCubeYRemap = {1, 1, 2, 2, 1, 1};
        private static readonly int[] KCubeZRemap = {0, 0, 1, 1, 2, 2};
        private static readonly float[] KCubeXSign = {-1.0F, 1.0F, 1.0F, 1.0F, 1.0F, -1.0F};
        private static readonly float[] KCubeYSign = {-1.0F, -1.0F, 1.0F, -1.0F, -1.0F, -1.0F};
        private static readonly float[] KCubeZSign = {1.0F, -1.0F, 1.0F, -1.0F, 1.0F, -1.0F};

        private int _seed;

        public bool _isRunning = false;

        private SpaceNoise _spaceNoise;

        public SpaceGenerator(int seed)
        {
            _seed = seed;
        }

        /// <summary>
        /// Generates faces in background. 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="callbackAction"></param>
        /// <param name="updateProgressAction"></param>
        public void GenerateFacesInBackground(SpaceParameters parameters, Action<Dictionary<int, Color[]>> callbackAction, Action<float> updateProgressAction)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            var workerThread = new Thread(() => BackgroundTask(parameters, callbackAction, updateProgressAction));
            workerThread.Start();
        }

        /// <summary>
        /// Generate faces
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="updateProgress"></param>
        /// <returns></returns>
        private Dictionary<int, Color[]> GenerateFaces(SpaceParameters parameters, Action<float> updateProgress = null)
        {
            var (faceSize, pointStars, nebula, stars, suns) = parameters;
            var faces = new Dictionary<int, Color[]>(6);
            var invSize = 1.0f / faceSize;
            _spaceNoise = new SpaceNoise(_seed, 1f);
            
            var sw = new Stopwatch();
            sw.Start();
            
            var totalAmount = faceSize * faceSize * 6;
            var progressStep = 1 + faceSize;
            var progressPercentage = ((float)progressStep / (float)totalAmount);

            Parallel.For(0, 6, new ParallelOptions
            {
                MaxDegreeOfParallelism = 2
            }, face =>
            {
                var colors = new Color[faceSize * faceSize];
                var signScale = new Vector3(KCubeXSign[face], KCubeYSign[face], KCubeZSign[face]);
                Parallel.For(0, faceSize, new ParallelOptions
                {
                    MaxDegreeOfParallelism = 2
                }, x =>
                {
                    Parallel.For(0, faceSize, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 2
                    }, y =>
                    {
                        var idx = x + faceSize * y;
                        var uvDir = new Vector3(x * invSize * 2.0f - 1.0f, y * invSize * 2.0f - 1.0f, 1.0f);
                        uvDir = uvDir.normalized;
                        uvDir.Scale(signScale);
                        var dir = Vector3.zero;
                        dir[KCubeXRemap[face]] = uvDir[0];
                        dir[KCubeYRemap[face]] = uvDir[1];
                        dir[KCubeZRemap[face]] = uvDir[2];
                        var point = dir * 1;
                        colors[idx] = CalculatePixelColor(idx, point, pointStars, nebula, stars, suns);
                    });
                    
                    updateProgress?.Invoke(progressPercentage);
                });
                var actualFace = face switch
                {
                    2 => 3,
                    3 => 2,
                    _ => face
                };
                faces.Add(actualFace, colors);
            });

            Debug.Log($"Generated space. Elapsed: {sw.ElapsedMilliseconds}ms");
            return faces;
        }
        
        private void BackgroundTask(SpaceParameters spaceParametersDto, Action<Dictionary<int, Color[]>> callback, Action<float> updateProgress)
        {
            var dispatcher = UnityMainThreadDispatcher.Instance;
            var action = GenerateFaces(spaceParametersDto, updateProgress);
            dispatcher.Enqueue(()=> callback(action));
            _isRunning = false;
        }
        
        private Color CalculatePixelColor(int idx, Vector3 dir, IReadOnlyList<Color> pointStars,
            NebulaParameters nebula, IReadOnlyList<StarParameters> stars,
            IReadOnlyList<StarParameters> suns)
        {
            var color = Color.black;

            if (pointStars.Count > 0) color = pointStars[idx];
            if (nebula != null) color = CalculateNebula(nebula, dir, color);
            if (stars.Count > 0) color = CalculateStars(stars, dir, color);
            if (suns.Count > 0) color = CalculateStars(suns, dir, color);

            return color;
        }

        private Color CalculateNebula(NebulaParameters nebulaParameters, Vector3 point,
            Color c)
        {
            var (density, falloff, details, shape) = nebulaParameters;
            var nebulaShape = ShapeNoise(point + shape.Offset, shape.Frequency, shape.DisplaceSteps);
            var baseNoise = BaseNoise(point + shape.Offset, shape.Frequency, shape.Octaves);
            var nebulaNoise = Mathf.Lerp(0, baseNoise, nebulaShape);

            foreach (var detailLayer in details)
            {
                var (offset, frequency, color, steps, octaves) = detailLayer;
                frequency = Mathf.Lerp(frequency / 2, frequency, nebulaShape);
                var textureNoise = DisplaceNoise(point + offset, frequency, octaves, steps);
                textureNoise = Mathf.Lerp(0, textureNoise, nebulaNoise);
                textureNoise = Mathf.Pow(textureNoise * density, falloff);
                c = Color.Lerp(c, color, textureNoise);
            }

            return c;

            float BaseNoise(Vector3 p, float f, int o)
            {
                var n = _spaceNoise.NormalNoise(p,
                    f,
                    o,
                    1.8f,
                    0.8f);

                return n;
            }

            float ShapeNoise(Vector3 p, float f, int s)
            {
                var displace = _spaceNoise.Displace(p, f, s);
                p += new Vector3(displace, displace, displace);
                var worley = _spaceNoise.Worley(p);
                return worley;
            }

            float DisplaceNoise(Vector3 p, float f, int o, int steps)
            {
                var displace = _spaceNoise.Displace(p, 1, steps);
                p += new Vector3(displace, displace, displace);
                var baseNoise = _spaceNoise.NormalNoise(p,
                    f,
                    o,
                    1.8f,
                    0.8f);
                return baseNoise;
            }
        }

        private static Color CalculateStars(IReadOnlyList<StarParameters> starParametersArray, Vector3 dir, Color color,
            bool useArcDistance = true)
        {
            foreach (var starParameters in starParametersArray)
            {
                var (starDirection, coreRadius, haloFalloff, coreColor, haloColor) = starParameters;
                var distance = useArcDistance
                    ? SphereMath.ArcDistance(dir, starDirection, 1.0f)
                    : Vector3.Distance(dir, starDirection);
                if (distance <= coreRadius)
                {
                    color = coreColor;
                    continue;
                }

                var e = 1.0f - Mathf.Exp(-(distance - coreRadius) * haloFalloff);
                var rgb = Color.Lerp(coreColor, haloColor, e);
                color = Color.Lerp(rgb, color, e);
            }

            return color;
        }
    }

    internal class NoiseCalculationException : Exception
    {
        public NoiseCalculationException(string s) : base(s)
        {
        }
    }
}
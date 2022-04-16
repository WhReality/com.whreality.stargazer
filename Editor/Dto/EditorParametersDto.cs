using System;
using UnityEngine;
using WhReality.Stargazer.Models;

// ReSharper disable InconsistentNaming

namespace WhReality.Stargazer.Dto
{
    [Serializable]
    public class EditorParametersDto
    {
        public double Version;

        public int Seed;
        
        public bool PointStarsEnabled;
        public float PointStarsDensity;
        public float PointStarsBrightness;

        public bool NebulaEnabled;
        public float NebulaDetailFrequency;
        public float NebulaDetailFrequencyLow;
        public float NebulaDetailFrequencyHigh;
        public bool NebulaDetailRandomFrequencyEnabled;
        public int NebulaDetailLayerCount;
        public float NebulaDensity;
        public float NebulaFalloff;
        public bool NebulaDetailGradientEnabled;
        public Gradient NebulaDetailGradient;
        public float NebulaShapeFrequency;
        public Vector3 NebulaShapeOffset;
        public int NebulaShapeDisplaceSteps;

        public bool StarsEnabled;
        public bool StarsGradientEnabled;
        public int StarsCount;
        public int StarsHaloFalloff;
        public bool StarsRandomHaloFalloffEnabled;
        public float StarsHaloFalloffMin;
        public float StarsHaloFalloffMax;
        public Gradient StarsGradient;

        public bool SunsEnabled;
        public int SunsCount;
        public bool SunsGradientEnabled;
        public int SunsHaloFalloff;
        public int SunsCoreSize;
        public Gradient SunsColorGradient;

        public int NebulaDetailOctaves;
        public int NebulaShapeOctaves;
        public int FaceSize;
        public int SelectedFaceSize;
        public AssetType AssetType;
        public string Filename;
        public string Path;
        public bool IncludeSeed;
        public bool IncludeFaceSize;
        public bool SaveJson;
        public TextureLayout TextureLayout { get; set; }
    }
}
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WhReality.Stargazer.Dto;
using Object = UnityEngine.Object;

namespace WhReality.Tools.Stargazer.Services
{
    public static class AssetService
    {
        public static string SavePng(byte[] pngBytes, int faceSize, int seed, string folder, string name,
            bool includeFaceSize = false, bool includeSeed = false)
        {
            var fullFilename = ConstructOutputFilename("png",
                includeSeed ? $"_{seed}" : "",
                includeFaceSize ? $"_{faceSize}" : "",
                folder,
                name,
                true);

            File.WriteAllBytes(fullFilename, pngBytes);

            AssetDatabase.Refresh();

            var importer = (TextureImporter) AssetImporter.GetAtPath(fullFilename);
            if (importer == null) throw new AssetManagerException($"Failed to find asset {fullFilename}");

            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.TextureCube;
            importer.maxTextureSize = faceSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            Debug.Log($"Saved file {fullFilename}");
            return fullFilename;
        }

        public static string SaveCubemap(Object cubemap, int faceSize, int seed, string folder, string name,
            bool includeFaceSize, bool includeSeed)
        {
            var fullFilename = ConstructOutputFilename("asset",
                includeSeed ? $"_{seed}" : "",
                includeFaceSize ? $"_{faceSize}" : "",
                folder,
                name,
                true);

            AssetDatabase.CreateAsset(cubemap, fullFilename);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Saved file {fullFilename}");
            return fullFilename;
        }


        public static void SaveJson(string folder, string name, EditorParametersDto spaceParams)
        {
            var fullFilename = ConstructOutputFilename("json",
                "123",
                "1234",
                folder,
                name,
                true);
            var json = JsonUtility.ToJson(spaceParams);
            File.WriteAllText(fullFilename, json);
            Debug.Log($"Saved file {fullFilename}");
        }

        private static string CheckFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)){
                folder = "Assets/";
            }
            
            // Make sure base folder is 'Assets/'
            if (!folder.ToLower().StartsWith("assets/")) folder = "Assets/" + folder;

            // Add to the end
            if (!folder.EndsWith("/")) folder += "/";

            return folder;
        }


        public static string ConstructOutputFilename(
            string extension,
            string seed,
            string faceSize,
            string folder,
            string name,
            bool createFolder = false)
        {
            folder = CheckFolder(folder);
            if (createFolder) CreateFolderIfNeeded(folder);

            return $"{folder}{name}{seed}{faceSize}.{extension}";
        }

        private static void CreateFolderIfNeeded(string folder)
        {
            if (string.IsNullOrEmpty(folder) || Directory.Exists(folder)) return;

            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();

            Debug.Log($"Created folder {folder}");
        }
    }

    public class AssetManagerException : Exception
    {
        public AssetManagerException(string s) : base(s)
        {
        }
    }
}
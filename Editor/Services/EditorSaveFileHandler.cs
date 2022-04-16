using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using WhReality.Stargazer.Dto;

namespace WhReality.Tools.Stargazer.Services
{
    public static class EditorSaveService
    {
        private static string BasePath { get; set; } = $"{Application.persistentDataPath}/WhReality/Stargazer/EditorParameters/";
        
        public static bool SaveFileExists(string filename){
            if (!filename.StartsWith("/")){
                filename = "/" + filename;
            }
            
            var fullFilename = $"{BasePath}{filename}";
            return File.Exists(fullFilename);
        }
        
        public static void SaveParametersJson(string filename, EditorParametersDto dto)
        {
            var json = JsonUtility.ToJson(dto);
            var fullFilename = $"{BasePath}{filename}";
            var path = Path.GetDirectoryName(fullFilename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            File.WriteAllText($"{BasePath}{filename}", json, System.Text.Encoding.Unicode);
            Debug.Log($"Saved parameters to '{fullFilename}'");
        }

        public static EditorParametersDto LoadParametersDto(string filename)
        {
            var fullFilename = $"{BasePath}{filename}";
            
            var json = File.ReadAllText(fullFilename);
            json = DecodeEncodedNonAsciiCharacters(json);
            var dto = JsonUtility.FromJson<EditorParametersDto>(json);
            
            Debug.Log($"Loaded parameters from '{fullFilename}'");
            return dto;
        }
        
        static string DecodeEncodedNonAsciiCharacters( string value ) {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char) int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber )).ToString();
                } );
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;
using WhReality.Stargazer.Infrastructure;
using WhReality.Stargazer.Resources;


namespace WhReality.Stargazer
{
    public class JsonImportEditor: EditorWindowBase
    {
        private string _json = "Paste json here...";
        
        private bool _canImport = false;

        public Action<string> Import { get; set; }
        public Func<string, bool> ValidateJson { get; set; }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("JSON", EditorStyles.boldLabel);
            var style = new GUIStyle(GUI.skin.textField)
            {
                margin = new RectOffset(5, 5, 5, 5),
                fixedHeight = 500
            };
            Validate(EditorGUILayout.TextArea(_json, style));
            
            if (_canImport && GUILayout.Button("Import"))
            {
                Import?.Invoke(_json);
            }

            if (!_canImport)
            {
                EditorGUILayout.HelpBox(TextResources.InvalidJsonError,
                    MessageType.Warning, true);
            }
            
            
            GUILayout.EndVertical();
        }

        private void Validate(string json)
        {
            if (ValidateJson == null)
            {
                _canImport = true;
                _json = json;
                return;
            }

            _canImport = ValidateJson.Invoke(json);
            if (_canImport)
            {
                _json = json;
            }
        }
    }
}
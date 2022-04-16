using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WhReality.Stargazer.Infrastructure
{
    public static class WhGUI
    {
        private static GUIStyle _horizontalLine(int height = 1)
        {
            return new GUIStyle()
            {
                normal = { background = EditorGUIUtility.whiteTexture },
                margin = new RectOffset(0, 0, 10, 10),
                fixedHeight = height,
            };
        } 

        public static void HorizontalLine(Color color, int height = 1)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, _horizontalLine(height));
            GUI.color = c;
        }

        public static void Indented(float px, Action action, bool vertical = false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(px, false);
            if (vertical) EditorGUILayout.BeginVertical();
            
            action?.Invoke();
            
            if (vertical) EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        public static void Indented(Action action, bool vertical = false)
        {
            Indented(20, action, vertical);
        }
        
        public static bool IconButton(string icon, string tooltip, RectOffset margin = null, int width = 40, int height = 40)
        {
            if (margin == null)
            {
                margin = new RectOffset(5, 0, 0, 0);
            }
                
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                margin = margin,
                fixedWidth = 30,
                fixedHeight = 30
            };
                
            var iconId = AssetDatabase.FindAssets(icon, null).FirstOrDefault();
            if (iconId == null) return GUILayout.Button(new GUIContent("Icon not found", tooltip), buttonStyle);
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconId));
            return GUILayout.Button(new GUIContent(asset, tooltip), buttonStyle);

        }
    }
}
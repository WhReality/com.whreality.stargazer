using System;
using System.Collections.Generic;
using UnityEngine;
using WhReality.Stargazer.Models;

namespace WhReality.Tools.Stargazer.Services
{
    public class TextureService
    {
        /// <summary>
        /// Create cubemap unity object
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="faceSize"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Cubemap CreateCubemap(IReadOnlyDictionary<int, Color[]> colors, int faceSize, string name)
        {
            var cubemap = new Cubemap(faceSize, TextureFormat.RGBA32, true) {name = name};
            for (var i = 0; i < 6; i++)
            {
                var face = i switch
                {
                    3 => 2,
                    2 => 3,
                    _ => i
                };
                cubemap.SetPixels(colors[i], (CubemapFace) face, 0);
            }

            cubemap.Apply();
            return cubemap;
        }

        /// <summary>
        /// Create texture2D unity object
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="faceSize"></param>
        /// <param name="name"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        public static Texture2D CreateTexture(IReadOnlyDictionary<int, Color[]> colors, int faceSize, string name,
            TextureLayout layout)
        {
            return layout switch
            {
                TextureLayout.Default => DefaultLayout(),
                TextureLayout.Horizontal => HorizontalLayout(1,0),
                TextureLayout.Vertical => HorizontalLayout(0,1),
                _ => throw new ArgumentOutOfRangeException(nameof(layout), layout, null)
            };

            Texture2D HorizontalLayout(int x, int y)
            {
                var faceSizeX = x == 0 ? faceSize : faceSize * 6;
                var faceSizeY = y == 0 ? faceSize : faceSize * 6;
                
                var texture2D = new Texture2D(faceSizeX, faceSizeY) {name = name};
                for (var i = 0; i < 6; i++) texture2D.SetPixels(faceSize * i * x, faceSize * i * y, faceSize, faceSize, colors[i]);

                return texture2D;
            }

            Texture2D DefaultLayout()
            {
                var width = faceSize * 4;
                var height = faceSize * 3;
                var faceOffsets = new[]
                {
                    new Vector2(2, 1), // PositiveX
                    new Vector2(0, 1), // NegativeX
                    new Vector2(1, 2), // PositiveY
                    new Vector2(1, 0), // NegativeY
                    new Vector2(1, 1), // PositiveZ
                    new Vector2(3, 1)  // NegativeZ
                };

                var result = new Texture2D(width, height) {name = name};
                var baseColors = new Color[width * height];
                for (var i = 0; i < baseColors.Length; i++) baseColors[i] = Color.black;

                result.SetPixels(baseColors);
                for (var i = 0; i < 6; i++)
                {
                    var x = (int) faceOffsets[i].x * faceSize;
                    var y = (int) faceOffsets[i].y * faceSize;
                    result.SetPixels(x, y, faceSize, faceSize, colors[i]);
                }

                result.Apply();
                return result;
            }
        }

    }
}
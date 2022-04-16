using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhReality.Stargazer.Dto;
using WhReality.Stargazer.Infrastructure;
using WhReality.Stargazer.Models;
using WhReality.Stargazer.Resources;
using WhReality.Tools.Stargazer.Helpers;
using WhReality.Tools.Stargazer.Services;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace WhReality.Stargazer
{
    public class SpaceGeneratorEditor : EditorWindowBase
    {
        private const double Version = 1.0;
        private const float GUIIndention = 10;
        private const int PreviewFaceSize = 128;
        private const string AutosaveFile = "autosave.json";

        private string[] _faceSizeOptions =
        {
            "512", "1024", "2048", "4096"
        };

        private bool _firstDrawCall = true;
        private int _seed;

        private bool _autoUpdatePreview = true;
        
        private AssetType _assetType;
        private string _folder;
        private DateTime? _generatePreviewTime;
        private bool _includeFaceSize;
        private bool _includeJson;
        private bool _includeSeed;
        private bool _invalidJson;
        private string _json;
        private string _name;
        private TextureLayout _textureLayout;

        private bool _nebulaEnabled;
        private float _nebulaDensity;
        private float _nebulaFalloff;
        private float _nebulaDetailDetailFrequency;
        private float _nebulaDetailFrequencyHigh;
        private float _nebulaDetailFrequencyLow;
        private Gradient _nebulaDetailGradient;
        private bool _nebulaDetailRandomColorEnabled;
        private int _nebulaDetailOctaves;
        private int _nebulaLayersCount;
        private bool _nebulaDetailRandomFrequency;
        private float _nebulaShapeFrequency;
        private Vector3 _nebulaShapeOffset;
        private int _nebulaShapeOctaves;
        private int _nebulaShapeDisplaceSteps;

        private bool _pointStarsEnabled;
        private float _pointStarBrightness;
        private float _pointStarDensity;

        private Texture2D _preview;

        private Vector2 _scrollPos;

        private int _selectedFaceSize;
        private bool _showDetailSettings;
        private bool _showShapeSettings;
        private Gradient _starsColorGradient = new Gradient();
        private int _starsCount;

        private bool _starsEnabled;
        private bool _starsGradientEnabled;
        private int _starsHaloFalloff;
        private float _starsHaloFalloffMax;
        private float _starsHaloFalloffMin;
        private bool _starsRandomHaloFalloffEnabled;

        private int _sunsCoreSize;
        private int _sunsHaloFalloff;
        private int _sunsCount;

        private bool _sunsEnabled;
        private bool _sunsGradientEnabled;
        private Gradient _sunsColorGradient = new Gradient();
        private bool _sunsRandomColor;

        private bool _jsonDiffrentVersion;
        private Texture2D _logo;

        private Work _currentWork;
        
        private float _workProgress;
        private int _faceSize;
        private bool _repaint;
        private string _faceSizeLimitationWarning;

        private enum Work
        {
            None,
            Asset,
            Preview
        }

        // JSON
        public string Json
        {
            get => _json;
            set
            {
                if (!TryParseJson<EditorParametersDto>(value, out _)) _invalidJson = true;

                _json = value;
            }
        }


        // Point star properties
        public bool PointStarsEnabled
        {
            get => _pointStarsEnabled;
            set => OnPropertyChanged(ref _pointStarsEnabled, value);
        }

        public float PointStarDensity
        {
            get => _pointStarDensity;
            set => OnPropertyChanged(ref _pointStarDensity, value);
        }

        public float PointStarBrightness
        {
            get => _pointStarBrightness;
            set => OnPropertyChanged(ref _pointStarBrightness, value);
        }

        // Nebula properties
        public bool NebulaEnabled
        {
            get => _nebulaEnabled;
            set => OnPropertyChanged(ref _nebulaEnabled, value);
        }

        public float NebulaDetailFrequency
        {
            get => _nebulaDetailDetailFrequency;
            set => OnPropertyChanged(ref _nebulaDetailDetailFrequency, value);
        }

        public float NebulaFrequencyLow
        {
            get => _nebulaDetailFrequencyLow;
            set => OnPropertyChanged(ref _nebulaDetailFrequencyLow, value);
        }

        public float NebulaFrequencyHigh
        {
            get => _nebulaDetailFrequencyHigh;
            set => OnPropertyChanged(ref _nebulaDetailFrequencyHigh, value);
        }

        public bool NebulaRandomFrequency
        {
            get => _nebulaDetailRandomFrequency;
            set => OnPropertyChanged(ref _nebulaDetailRandomFrequency, value);
        }

        public int NebulaLayersCount
        {
            get => _nebulaLayersCount;
            set => OnPropertyChanged(ref _nebulaLayersCount, value);
        }

        public float NebulaDensity
        {
            get => _nebulaDensity;
            set => OnPropertyChanged(ref _nebulaDensity, value);
        }

        public float NebulaFalloff
        {
            get => _nebulaFalloff;
            set => OnPropertyChanged(ref _nebulaFalloff, value);
        }

        public bool NebulaGradientEnabled
        {
            get => _nebulaDetailRandomColorEnabled;
            set => OnPropertyChanged(ref _nebulaDetailRandomColorEnabled, value);
        }

        public Gradient NebulaGradient
        {
            get => _nebulaDetailGradient;
            set => OnPropertyChanged(ref _nebulaDetailGradient, value);
        }

        public float NebulaShapeFrequency
        {
            get => _nebulaShapeFrequency;
            set => OnPropertyChanged(ref _nebulaShapeFrequency, value);
        }

        public Vector3 NebulaShapeOffset
        {
            get => _nebulaShapeOffset;
            set => OnVectorChanged(ref _nebulaShapeOffset, value);
        }

        public int NebulaShapeOctaves
        {
            get => _nebulaShapeOctaves;
            set => OnPropertyChanged(ref _nebulaShapeOctaves, value);
        }

        public int NebulaDetailOctaves
        {
            get => _nebulaDetailOctaves;
            set => OnPropertyChanged(ref _nebulaDetailOctaves, value);
        }

        public int NebulaShapeDisplaceSteps
        {
            get => _nebulaShapeDisplaceSteps;
            set => OnPropertyChanged(ref _nebulaShapeDisplaceSteps, value);
        }

        // Star properties
        public bool StarsEnabled
        {
            get => _starsEnabled;
            set => OnPropertyChanged(ref _starsEnabled, value);
        }

        public bool StarsGradientEnabled
        {
            get => _starsGradientEnabled;
            set => OnPropertyChanged(ref _starsGradientEnabled, value);
        }

        public int StarsCount
        {
            get => _starsCount;
            set => OnPropertyChanged(ref _starsCount, value);
        }

        public int StarsHaloFalloff
        {
            get => _starsHaloFalloff;
            set => OnPropertyChanged(ref _starsHaloFalloff, value);
        }

        public bool StarsRandomHaloFalloffEnabled
        {
            get => _starsRandomHaloFalloffEnabled;
            set => OnPropertyChanged(ref _starsRandomHaloFalloffEnabled, value);
        }

        public float StarsHaloFalloffMin
        {
            get => _starsHaloFalloffMin;
            set => OnPropertyChanged(ref _starsHaloFalloffMin, value);
        }

        public float StarsHaloFalloffMax
        {
            get => _starsHaloFalloffMax;
            set => OnPropertyChanged(ref _starsHaloFalloffMax, value);
        }

        public Gradient StarsColorGradient
        {
            get => _starsColorGradient;
            set => OnPropertyChanged(ref _starsColorGradient, value);
        }
        
        /// Sun properties
        public bool SunsEnabled
        {
            get => _sunsEnabled;
            set => OnPropertyChanged(ref _sunsEnabled, value);
        }

        public int SunsCount
        {
            get => _sunsCount;
            set => OnPropertyChanged(ref _sunsCount, value);
        }

        public bool SunsGradientEnabled
        {
            get => _sunsGradientEnabled;
            set => OnPropertyChanged(ref _sunsGradientEnabled, value);
        }

        public int SunHaloFalloff
        {
            get => _sunsHaloFalloff;
            set => OnPropertyChanged(ref _sunsHaloFalloff, value);
        }

        public int SunCoreSize
        {
            get => _sunsCoreSize;
            set => OnPropertyChanged(ref _sunsCoreSize, value);
        }

        public Gradient SunColorGradient
        {
            get => _sunsColorGradient;
            set => OnPropertyChanged(ref _sunsColorGradient, value);
        }

        public bool SunsRandomColor
        {
            get => _sunsRandomColor;
            set => OnPropertyChanged(ref _sunsRandomColor, value);
        }

        [MenuItem("Tools/Stargazer")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow(typeof(SpaceGeneratorEditor));
            window.name = "Stargazer";
            window.titleContent = new GUIContent("Stargazer");
            window.minSize = new Vector2(548, 10);
            window.Show();
        }

        private void Awake()
        {
            _firstDrawCall = false;
            LoadAutosave();
            GenerateSpaceInBackgroundCommand(Work.Preview);
        }

        private void OnGUI()
        {
            // Logo
            DrawLogo();
            
            DrawToolbar();
            WhGUI.HorizontalLine(Color.grey, 2);
            
            // Preview
            DrawPreviewOptions();
            
            WhGUI.HorizontalLine(Color.grey, 4);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, true);
            var lineColor = new Color(1f, 1f, 1f, 0.1f);
            // Seed
            DrawSeedOptions();
            WhGUI.HorizontalLine(lineColor);

            // Point stars
            DrawPointStarOptions();
            WhGUI.HorizontalLine(lineColor);

            // Nebula
            DrawNebulaOptions();
            WhGUI.HorizontalLine(lineColor);

            // Stars
            DrawStarOptions();
            WhGUI.HorizontalLine(lineColor);

            // Sun
            DrawSunOptions();
            WhGUI.HorizontalLine(lineColor);
            
            // Save
            DrawSaveOptions();
            
            EditorGUILayout.EndScrollView();
            
            void DrawLogo()
            {
                if (_logo == null)
                {
                    var ids = AssetDatabase.FindAssets("logo_stargazer_300");
                    _logo = (Texture2D) AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids.First()));
                }

                const int width = 180;
                const int height = 40;
                const int margin = 20;

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(margin, 0, margin, 0),
                });

                EditorGUI.DrawPreviewTexture(new Rect(margin, margin, width, height), _logo);

                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    padding = new RectOffset(190, 0, 0, 0)
                });
                GUILayout.Label($"v{Version:F}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    padding = new RectOffset(190, 0, height, 0)
                });
                EditorGUILayout.EndVertical();
              
                EditorGUILayout.EndHorizontal();
            }
            
            void DrawToolbar()
            {
                EditorGUILayout.BeginHorizontal();
                WhGUI.Indented(() =>
                {
                    if (WhGUI.IconButton("fas fa-file-import", TextResources.ImportJsonTooltip))
                    {
                        ShowImportJsonDialogCommand();
                    }
                    
                    if (WhGUI.IconButton("fas fa-dice-six", TextResources.FeelingLuckyTooltip))
                    {
                        RandomizeAllCommand();
                    }
                });
     
                EditorGUILayout.EndHorizontal();
            }
            
            void DrawSeedOptions()
            {
                EditorGUILayout.LabelField("Seed", EditorStyles.largeLabel);
                EditorGUILayout.BeginHorizontal();
                _seed = EditorGUILayout.IntField(" ", _seed);
                if (GUILayout.Button(TextResources.RandomSeed))
                {
                    Random.InitState(Environment.TickCount);
                    _seed = Random.Range(10, 100000000);
                    GenerateSpaceInBackgroundCommand(Work.Preview);
                }
                EditorGUILayout.EndHorizontal();
            }

            void DrawPointStarOptions()
            {
                PointStarsEnabled = EditorGUILayout.ToggleLeft(TextResources.PointStars, PointStarsEnabled);
                if (!PointStarsEnabled) return;

                WhGUI.Indented(
                    () => PointStarDensity = EditorGUILayout.FloatField(TextResources.Density, PointStarDensity));
                WhGUI.Indented(
                    () => PointStarBrightness =
                        EditorGUILayout.FloatField(TextResources.Brightness, PointStarBrightness));
            }

            void DrawNebulaOptions()
            {
                NebulaEnabled = EditorGUILayout.ToggleLeft(TextResources.Nebula, NebulaEnabled);
                if (!NebulaEnabled) return;

                WhGUI.Indented(
                    () => NebulaDensity =
                        EditorGUILayout.Slider(TextResources.Density, NebulaDensity, Limits.NebulaDensityMin,
                            Limits.NebulaDensityMax));
                WhGUI.Indented(
                    () => NebulaFalloff =
                        EditorGUILayout.Slider(TextResources.Falloff, NebulaFalloff, Limits.NebulaFalloffMin,
                            Limits.NebulaFalloffMax));

                _showShapeSettings = EditorGUILayout.Foldout(_showShapeSettings, TextResources.ShapeSettings);
                if (_showShapeSettings)
                {
                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaShapeOctaves =
                            EditorGUILayout.IntSlider(TextResources.Octaves, NebulaShapeOctaves, Limits.OctavesMin,
                                Limits.OctavesMax));
                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaShapeFrequency =
                            EditorGUILayout.Slider(TextResources.Frequency, NebulaShapeFrequency,
                                Limits.NebulaShapeFrequencyMin, Limits.NebulaShapeFrequencyMax));
                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaShapeDisplaceSteps = EditorGUILayout.IntSlider(TextResources.DisplaceSteps,
                            NebulaShapeDisplaceSteps, 1, 6));
                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaShapeOffset =
                            EditorGUILayout.Vector3Field(TextResources.Offset, NebulaShapeOffset));
                }

                _showDetailSettings = EditorGUILayout.Foldout(_showDetailSettings, TextResources.DetailLayers);
                if (_showDetailSettings)
                {
                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaRandomFrequency =
                            EditorGUILayout.Toggle(TextResources.RandomFrequency, NebulaRandomFrequency));

                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaGradientEnabled =
                            !EditorGUILayout.Toggle(TextResources.RandomColors, !NebulaGradientEnabled));

                    WhGUI.Indented(GUIIndention * 2, () => NebulaLayersCount =
                        EditorGUILayout.IntSlider(TextResources.Count, NebulaLayersCount, Limits.NebulaCountMin,
                            Limits.NebulaCountMax));

                    WhGUI.Indented(GUIIndention * 2,
                        () => NebulaDetailOctaves =
                            EditorGUILayout.IntSlider(TextResources.Octaves, NebulaDetailOctaves, Limits.OctavesMin,
                                Limits.OctavesMax));

                    if (!NebulaRandomFrequency)
                        WhGUI.Indented(GUIIndention * 2,
                            () => NebulaDetailFrequency = EditorGUILayout.Slider(TextResources.Frequency,
                                NebulaDetailFrequency, Limits.NebulaFrequencyMin, Limits.NebulaFrequencyMax));
                    else
                        WhGUI.Indented(GUIIndention * 2,
                            () => EditorGUILayout.MinMaxSlider(
                                $"{TextResources.Frequency} ({NebulaFrequencyLow:F}/{NebulaFrequencyHigh:F})",
                                ref _nebulaDetailFrequencyLow, ref _nebulaDetailFrequencyHigh,
                                Limits.NebulaFrequencyMin,
                                Limits.NebulaFrequencyMax));

                    if (NebulaGradientEnabled)
                    {
                        WhGUI.Indented(GUIIndention * 2,
                            () => NebulaGradient =
                                EditorGUILayout.GradientField(TextResources.GradientLabel, NebulaGradient));
                    }
                }
            }

            void DrawStarOptions()
            {
                StarsEnabled = EditorGUILayout.ToggleLeft(TextResources.Stars, StarsEnabled);
                if (!StarsEnabled) return;

                WhGUI.Indented(() =>
                    StarsRandomHaloFalloffEnabled =
                        EditorGUILayout.Toggle(TextResources.RandomFalloff, StarsRandomHaloFalloffEnabled));

                WhGUI.Indented(() =>
                    StarsGradientEnabled = !EditorGUILayout.Toggle(TextResources.RandomColors, !StarsGradientEnabled));
                WhGUI.Indented(() =>
                    StarsCount = EditorGUILayout.IntSlider(TextResources.Count, StarsCount, 1, 120));
                if (!StarsRandomHaloFalloffEnabled)
                    WhGUI.Indented(() =>
                        StarsHaloFalloff = EditorGUILayout.IntSlider(TextResources.Falloff, StarsHaloFalloff, 1, 1000));

                if (StarsRandomHaloFalloffEnabled)
                    WhGUI.Indented(() =>
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.MinMaxSlider(
                            TextResources.StarsFalloffRange +
                            $"({(int) _starsHaloFalloffMin}-{(int) _starsHaloFalloffMax})",
                            ref _starsHaloFalloffMin,
                            ref _starsHaloFalloffMax, 1, 1000);
                        EditorGUILayout.EndHorizontal();
                    });

                if (StarsGradientEnabled)
                    WhGUI.Indented(() =>
                        StarsColorGradient =
                            EditorGUILayout.GradientField(TextResources.GradientLabel, StarsColorGradient));
            }

            void DrawSunOptions()
            {
                SunsEnabled = EditorGUILayout.ToggleLeft(TextResources.SunsLabel, SunsEnabled);
                if (!SunsEnabled) return;

                WhGUI.Indented(() =>
                    SunsRandomColor = EditorGUILayout.Toggle(TextResources.RandomColors, SunsRandomColor));
                WhGUI.Indented(() => SunsCount = EditorGUILayout.IntSlider(TextResources.SunsCount, SunsCount,
                    Limits.SunsCountMin,
                    Limits.SunsCountMax));
                WhGUI.Indented(() => SunCoreSize = EditorGUILayout.IntSlider(TextResources.SunsCoreSize, SunCoreSize,
                    Limits.SunsCoreSizeMin, Limits.SunsCoreSizeMax));
                WhGUI.Indented(() =>
                    SunHaloFalloff = EditorGUILayout.IntSlider(TextResources.SunsFalloff, SunHaloFalloff,
                        Limits.SunsHaloFalloffMin, Limits.SunsHaloFalloffMax));

                if (!_sunsRandomColor)
                    WhGUI.Indented(() =>
                        SunColorGradient =
                            EditorGUILayout.GradientField(TextResources.GradientLabel, SunColorGradient));
            }

            void DrawPreviewOptions()
            {
                EditorGUILayout.BeginVertical();
                
                // Auto update
                WhGUI.Indented(
                    () => _autoUpdatePreview =
                        EditorGUILayout.Toggle(TextResources.AutoUpdatePreview, _autoUpdatePreview));
                // Preview
                if (_preview != null)
                    WhGUI.Indented(() => GUILayout.Box(_preview, new GUIStyle()
                    {
                        padding = new RectOffset(0,20,5,5),
                        stretchWidth = true,
                    }));
                
                // Refresh preview button
                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset(20, 20, 5, 20)
                };
                
                if (_currentWork == Work.None && GUILayout.Button(TextResources.RefreshPreview, buttonStyle))
                {
                    GenerateSpaceInBackgroundCommand(Work.Preview);
                }
                
                if (_currentWork != Work.None)
                {
                    var text = _currentWork == Work.Asset ? "Generating asset..." : "Generating preview...";
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), _workProgress, text);
                }
                
                EditorGUILayout.EndHorizontal();
            }

            void DrawSaveOptions()
            {
                var exampleFilename = AssetService.ConstructOutputFilename("asset",
                    _includeSeed ? $"_{_seed}" : "",
                    _includeFaceSize ? $"_{ResolveFaceSize()}" : "",
                    _folder,
                    _name);

                // Save Asset label
                EditorGUILayout.LabelField(TextResources.SaveAssetLabel, EditorStyles.boldLabel);

                WhGUI.Indented(() =>
                {
                    _assetType = (AssetType) EditorGUILayout.EnumPopup(TextResources.AssetType, _assetType);
                    switch (_assetType)
                    {
                        case AssetType.Cubemap:
                            break;
                        case AssetType.PNG:
                        {
                            _textureLayout =
                                (TextureLayout) EditorGUILayout.EnumPopup(TextResources.TextureLayout, _textureLayout);
                            switch (_textureLayout)
                            {
                                case TextureLayout.Default:
                                    _faceSizeOptions = new[] {"512", "1024", "2048", "4096"};
                                    _faceSizeLimitationWarning = string.Empty;
                                    break;
                                case TextureLayout.Vertical:
                                case TextureLayout.Horizontal:
                                    _faceSizeOptions = new[] {"512", "1024", "2048"};
                                    if (_selectedFaceSize > 2)
                                    {
                                        _selectedFaceSize = 2;
                                    }
                                    _faceSizeLimitationWarning = TextResources.FaceSizeLimitationWarning;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }


                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    _selectedFaceSize =
                        EditorGUILayout.Popup(TextResources.FaceSize, _selectedFaceSize, _faceSizeOptions);
                    
                    if (!string.IsNullOrEmpty(_faceSizeLimitationWarning) && _assetType == AssetType.PNG)
                    {
                        EditorGUILayout.HelpBox(_faceSizeLimitationWarning, MessageType.Warning, true);
                    }
                    
                    if (_selectedFaceSize != -1 && int.Parse(_faceSizeOptions[_selectedFaceSize]) > 1024)
                    {
                        EditorGUILayout.HelpBox(TextResources.FaceSizeWarning, MessageType.Warning, true);
                    }
                    
                    _name = EditorGUILayout.TextField(TextResources.Filename, _name);
                    _folder = EditorGUILayout.TextField(TextResources.Path, _folder);
                    _includeSeed = EditorGUILayout.Toggle(TextResources.IncludeSeed, _includeSeed);
                    _includeFaceSize = EditorGUILayout.Toggle(TextResources.IncludeFaceSize, _includeFaceSize);
                    _includeJson = EditorGUILayout.Toggle(TextResources.SaveJson, _includeJson);
                    EditorGUILayout.LabelField($"Example: \"{exampleFilename}\"", EditorStyles.miniLabel);
                }, true);
                
                // Buttons
                EditorGUILayout.BeginVertical(new GUIStyle()
                {
                    margin = new RectOffset(20, 20, 20, 20)
                });
                
                if (GUILayout.Button("Generate asset")) GenerateSpaceInBackgroundCommand();
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Copy JSON to clipboard")) CopyJsonToClipboardCommand();
                EditorGUILayout.Space();
                
                if (GUILayout.Button(TextResources.LoadDefault)) LoadDefaultParameters();

                EditorGUILayout.EndVertical();
            }
        }

        private void Update()
        {
            if (_repaint)
            {
                _repaint = false;
                Repaint();
            }
            
            if (_generatePreviewTime != null && _generatePreviewTime <= DateTime.Now && _currentWork == Work.None)
            {
                _generatePreviewTime = null;
                GenerateSpaceInBackgroundCommand(Work.Preview, true);
            }
        }
        
        private void RandomizeAllCommand()
        {
            var parameters = DefaultParameters();
            parameters.Seed = Random.Range(0, 1000000000);
            parameters.FaceSize = _faceSize;
            parameters.SelectedFaceSize = _selectedFaceSize;
            parameters.TextureLayout = _textureLayout;
            parameters.AssetType = _assetType;
            parameters.IncludeSeed = _includeSeed;
            parameters.IncludeFaceSize = _includeFaceSize;
            parameters.SaveJson = _includeJson;
            parameters.Filename = _name;
            parameters.Path = _folder;

            if (_pointStarsEnabled)
            {
                parameters.PointStarsEnabled = true;
                parameters.PointStarsDensity = Random.Range(0.3f, 4.0f);
                parameters.PointStarsBrightness = Random.Range(0.1f, 1.0f);
            }

            parameters.NebulaEnabled = _nebulaEnabled;
            if ( parameters.NebulaEnabled)
            {
                parameters.NebulaEnabled = true;
                var density = Random.Range(1.0f, 2.0f);
                parameters.NebulaDensity = density;
                parameters.NebulaFalloff = density;
                parameters.NebulaShapeOctaves = Random.Range(1, 12);
                parameters.NebulaShapeFrequency = Random.Range(0.1f, 1);
                parameters.NebulaShapeDisplaceSteps = Random.Range(1, 3);
                parameters.NebulaDetailRandomFrequencyEnabled = true;
                parameters.NebulaDetailLayerCount = Random.Range(1, 6);
                parameters.NebulaDetailOctaves = Random.Range(1, 12);
            }

            parameters.StarsEnabled = _starsEnabled;
            if (parameters.StarsEnabled)
            {
                
            }

            parameters.SunsEnabled = _sunsEnabled;
            if ( parameters.SunsEnabled)
            {
                parameters.SunsCount = Random.Range(1, 3);
                parameters.SunsHaloFalloff = 400;
                parameters.SunsGradientEnabled = false;
            }
            
            AssignParameters(parameters);
            GenerateSpaceInBackgroundCommand(Work.Preview, true);
        }

        private void ShowImportJsonDialogCommand()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = (JsonImportEditor)GetWindow(typeof(JsonImportEditor));
            window.name = "Stargazer JSON Import";
            window.titleContent = new GUIContent("Stargazer");
            window.Show();
            window.ValidateJson += ValidateJson;
            window.Import += (json) =>
            {
                LoadJsonCommand(json);
                window.ValidateJson -= ValidateJson;
                window.Close();
            };
        }

        private void GenerateSpaceInBackgroundCommand(Work work = Work.Asset, bool doAutosave = false)
        {
            if (_currentWork != Work.None)
            {
                return;
            }

            _currentWork = work;
            var faceSize = ResolveFaceSize(_currentWork);

            var spaceParameters = CreateSpaceParameters(faceSize);
            var spaceGenerator = new SpaceGenerator(_seed);
            var layout = _currentWork != Work.Preview ? TextureLayout.Default : _textureLayout;
            
            spaceGenerator.GenerateFacesInBackground(spaceParameters, faceColors =>
            {
                var cubemap = TextureService.CreateCubemap(faceColors, spaceParameters.FaceSize, _name);
                var texture2d = TextureService.CreateTexture(faceColors, spaceParameters.FaceSize, _name, layout);

                if (doAutosave)
                {
                    Autosave();
                }

                if (_currentWork == Work.Preview)
                {
                    _preview = new Texture2D(texture2d.width, texture2d.height);
                    _preview.SetPixels(texture2d.GetPixels());
                    _preview.Apply();
                    CompleteWork();
                    return;
                }

                if (_includeJson)
                {
                    SaveJson();
                }

                switch (_assetType)
                {
                    case AssetType.Cubemap:
                        SaveAsset(cubemap);
                        break;
                    case AssetType.PNG:
                        SaveAsset(texture2d.EncodeToPNG());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CompleteWork();
            }, UpdateProgress);

            void CompleteWork()
            {
                _currentWork = Work.None;
                _workProgress = 0;
            }
        }
        
        private void LoadJsonCommand(string json)
        {
            if (TryParseJson<EditorParametersDto>(json, out var dto))
            {
                AssignParameters(dto);
                GenerateSpaceInBackgroundCommand(Work.Preview);
            }

            try
            {
                var parameters = JsonUtility.FromJson<EditorParametersDto>(json);
                if (Math.Abs(parameters.Version - Version) > 0.1) _jsonDiffrentVersion = true;
            }
            catch (Exception)
            {
                Debug.LogError("Failed to load JSON");
            }
        }
        
        private bool ValidateJson(string json)
        {
            return TryParseJson<EditorParametersDto>(json, out _);
        }

        private void LoadDefaultParameters()
        {
            AssignParameters(DefaultParameters());
        }
        
        private int ResolveFaceSize(Work work = Work.Asset)
        {
            if (work == Work.Preview)
            {
                return PreviewFaceSize;
            }

            _faceSize = int.Parse(_faceSizeOptions[_selectedFaceSize]);
            return _faceSize;
        }

        private void UpdateProgress(float prog)
        {
            _workProgress += prog;
            _repaint = true;
        }
        
        public static Texture2D RenderMaterial(ref Material material, Vector2Int resolution, string filename = "")
        {
            var renderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y);
            Graphics.Blit(null, renderTexture, material);

            var texture = new Texture2D(resolution.x, resolution.y, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(Vector2.zero, resolution), 0, 0);

#if UNITY_EDITOR
            // optional, if you want to save it:
            if (filename.Length != 0)
            {
                byte[] png = texture.EncodeToPNG();
                File.WriteAllBytes(filename, png);
                AssetDatabase.Refresh();
            }
#endif

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture;
        }

        private SpaceParameters CreateSpaceParameters(int facesize)
        {
            SpaceRandom.SetSeed(_seed);

            var pointStars = _pointStarsEnabled
                ? SpaceRandom.RandomPointStars(_pointStarDensity, _pointStarBrightness, facesize, facesize,
                    Color.white)
                : new Color[0];

            var nebulas = _nebulaEnabled
                ? SpaceRandom.RandomNebulas(_nebulaDensity, _nebulaFalloff
                    , new SpaceRandom.NebulaDetailParams
                    {
                        Count = _nebulaLayersCount,
                        Octaves = _nebulaDetailOctaves,
                        Gradient = _nebulaDetailRandomColorEnabled ? _nebulaDetailGradient : null,
                        FrequencyMax = _nebulaDetailRandomFrequency
                            ? _nebulaDetailFrequencyHigh
                            : _nebulaDetailDetailFrequency,
                        FrequencyMin = _nebulaDetailRandomFrequency
                            ? _nebulaDetailFrequencyLow
                            : _nebulaDetailDetailFrequency
                    }, new SpaceRandom.NebulaShapeParams
                    {
                        Frequency = _nebulaShapeFrequency,
                        Octaves = _nebulaShapeOctaves,
                        Offset = _nebulaShapeOffset,
                        DisplaceSteps = _nebulaShapeDisplaceSteps
                    })
                : null;

            var stars = _starsEnabled
                ? SpaceRandom.RandomStars(_starsCount,
                    _starsRandomHaloFalloffEnabled ? _starsHaloFalloffMin : _starsHaloFalloff,
                    _starsRandomHaloFalloffEnabled ? _starsHaloFalloffMax : _starsHaloFalloff,
                    _starsGradientEnabled ? _starsColorGradient : null, 0.002f)
                : new StarParameters[0];

            var suns = _sunsEnabled
                ? SpaceRandom.RandomStars(_sunsCount, (float) _sunsHaloFalloff / 100, (float) _sunsHaloFalloff / 100,
                    _sunsRandomColor ? null : _sunsColorGradient, (float) _sunsCoreSize / 100)
                : new StarParameters[0];


            var spaceParameters = new SpaceParameters(facesize, pointStars, nebulas, stars, suns);
            return spaceParameters;
        }

        private void CopyJsonToClipboardCommand()
        {
            var spaceParams = CreateEditorParametersDto();
            var json = JsonUtility.ToJson(spaceParams);
            GUIUtility.systemCopyBuffer = json;
        }
        
        private void LoadAutosave()
        {
            if (EditorSaveService.SaveFileExists(AutosaveFile))
            {
                try
                {
                    var autosave = EditorSaveService.LoadParametersDto(AutosaveFile);
                    AssignParameters(autosave);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load {AutosaveFile}. Exception {e.Message}");
                }
            }

            AssignParameters(DefaultParameters());
        }
        
        private void Autosave()
        {
            EditorSaveService.SaveParametersJson(AutosaveFile, CreateEditorParametersDto());
        }

        private void AssignParameters(EditorParametersDto dto)
        {
            if (Math.Abs(dto.Version - Version) > 0.1) _jsonDiffrentVersion = true;

            _jsonDiffrentVersion = true;

            _seed = dto.Seed;
            _pointStarsEnabled = dto.PointStarsEnabled;
            _pointStarDensity = dto.PointStarsDensity;
            _pointStarBrightness = dto.PointStarsBrightness;

            _nebulaEnabled = dto.NebulaEnabled;
            _nebulaDensity = dto.NebulaDensity;
            _nebulaFalloff = dto.NebulaFalloff;
            _nebulaShapeFrequency = dto.NebulaShapeFrequency;
            _nebulaShapeOffset = dto.NebulaShapeOffset;
            _nebulaShapeDisplaceSteps = dto.NebulaShapeDisplaceSteps;
            _nebulaShapeOctaves = dto.NebulaShapeOctaves;
            _nebulaDetailRandomFrequency = dto.NebulaDetailRandomFrequencyEnabled;
            _nebulaDetailRandomColorEnabled = dto.NebulaDetailGradientEnabled;
            _nebulaLayersCount = dto.NebulaDetailLayerCount;
            _nebulaDetailRandomFrequency = dto.NebulaDetailRandomFrequencyEnabled;
            _nebulaDetailDetailFrequency = dto.NebulaDetailFrequency;
            _nebulaDetailFrequencyHigh = dto.NebulaDetailFrequencyHigh;
            _nebulaDetailFrequencyLow = dto.NebulaDetailFrequencyLow;
            _nebulaDetailGradient = dto.NebulaDetailGradient;
            _nebulaDetailOctaves = dto.NebulaDetailOctaves;

            _starsEnabled = dto.StarsEnabled;
            _starsRandomHaloFalloffEnabled = dto.StarsRandomHaloFalloffEnabled;
            _starsGradientEnabled = dto.StarsGradientEnabled;
            _starsCount = dto.StarsCount;
            _starsHaloFalloff = dto.StarsHaloFalloff;
            _starsHaloFalloffMax = dto.StarsHaloFalloffMax;
            _starsHaloFalloffMin = dto.StarsHaloFalloffMin;

            _sunsEnabled = dto.SunsEnabled;
            _sunsCount = dto.SunsCount;
            _sunsHaloFalloff = dto.SunsHaloFalloff;
            _sunsCoreSize = dto.SunsCoreSize;
            _sunsGradientEnabled = dto.SunsGradientEnabled;
            _sunsColorGradient = dto.SunsColorGradient;

            _faceSize = dto.FaceSize;
            _selectedFaceSize = dto.SelectedFaceSize;
            _assetType = dto.AssetType;
            _name = dto.Filename;
            _folder = dto.Path;
            _includeSeed = dto.IncludeSeed;
            _includeFaceSize = dto.IncludeFaceSize;
            _includeJson = dto.SaveJson;
            _textureLayout = dto.TextureLayout;
        }

        private void HandleKeys(KeyCode code)
        {
            if (code == KeyCode.F5) GenerateSpaceInBackgroundCommand(Work.Preview);
        }

        private EditorParametersDto CreateEditorParametersDto()
        {
            var spaceParams = new EditorParametersDto()
            {
                Version = Version,
                Seed = _seed,

                PointStarsEnabled = _pointStarsEnabled,
                PointStarsDensity = _pointStarDensity,
                PointStarsBrightness = _pointStarBrightness,

                NebulaEnabled = _nebulaEnabled,
                NebulaDensity = _nebulaDensity,
                NebulaFalloff = _nebulaFalloff,
                NebulaShapeFrequency = _nebulaShapeFrequency,
                NebulaShapeOffset = _nebulaShapeOffset,
                NebulaShapeDisplaceSteps = _nebulaShapeDisplaceSteps,
                NebulaShapeOctaves = _nebulaShapeOctaves,
                NebulaDetailRandomFrequencyEnabled = _nebulaDetailRandomFrequency,
                NebulaDetailGradientEnabled = _nebulaDetailRandomColorEnabled,
                NebulaDetailLayerCount = _nebulaLayersCount,
                NebulaDetailGradient = _nebulaDetailGradient,
                NebulaDetailFrequencyHigh = _nebulaDetailFrequencyHigh,
                NebulaDetailFrequencyLow = _nebulaDetailFrequencyLow,
                NebulaDetailFrequency = _nebulaDetailDetailFrequency,
                NebulaDetailOctaves = _nebulaDetailOctaves,

                StarsEnabled = _starsEnabled,
                StarsGradientEnabled = _starsGradientEnabled,
                StarsCount = _starsCount,
                StarsHaloFalloff = _starsHaloFalloff,
                StarsRandomHaloFalloffEnabled = _starsRandomHaloFalloffEnabled,
                StarsHaloFalloffMin = _starsHaloFalloffMin,
                StarsHaloFalloffMax = _starsHaloFalloffMax,
                StarsGradient = _starsColorGradient,

                SunsEnabled = _sunsEnabled,
                SunsCount = _sunsCount,
                SunsGradientEnabled = _sunsGradientEnabled,
                SunsHaloFalloff = _sunsHaloFalloff,
                SunsCoreSize = _sunsCoreSize,
                SunsColorGradient = _sunsColorGradient,

                FaceSize = _faceSize,
                SelectedFaceSize = _selectedFaceSize,
                AssetType = _assetType,
                Filename = _name,
                Path = _folder,
                IncludeFaceSize = _includeFaceSize,
                SaveJson = _includeJson,
                IncludeSeed = _includeSeed,
                TextureLayout = _textureLayout
            };

            return spaceParams;
        }

        private void SaveJson()
        {
            var spaceParams = CreateEditorParametersDto();
            AssetService.SaveJson(_folder, _name, spaceParams);
        }

        private void SaveAsset(Object cubemap)
        {
            AssetService.SaveCubemap(cubemap, _faceSize, _seed, _folder, _name,
                _includeFaceSize,
                _includeSeed);
        }

        private void SaveAsset(byte[] pngBytes)
        {
            AssetService.SavePng(pngBytes, _faceSize, _seed, _folder, _name,
                _includeFaceSize,
                _includeSeed);
        }

        private bool TryParseJson<T>(string json, out T output)
        {
            output = default;
            try
            {
                output = JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected override void OnPropertyChanged()
        {
            if (_autoUpdatePreview) _generatePreviewTime = DateTime.Now.AddMilliseconds(300);
        }

        private static EditorParametersDto DefaultParameters() => new EditorParametersDto()
        {
            Version = Version,
            Seed = 89088503,

            PointStarsEnabled = true,
            PointStarsDensity = 3,
            PointStarsBrightness = 1,

            NebulaEnabled = true,
            NebulaDensity = 1,
            NebulaFalloff = 1f,
            NebulaShapeOctaves = 12,
            NebulaShapeFrequency = 0.4f,
            NebulaShapeDisplaceSteps = 2,
            NebulaShapeOffset = new Vector3(),
            NebulaDetailRandomFrequencyEnabled = false,
            NebulaDetailGradientEnabled = false,
            NebulaDetailLayerCount = 4,
            NebulaDetailOctaves = 4,
            NebulaDetailFrequency = 2.6f,
            NebulaDetailFrequencyLow = 0.4f,
            NebulaDetailFrequencyHigh = 3,
            NebulaDetailGradient = new Gradient(),

            StarsEnabled = true,
            StarsRandomHaloFalloffEnabled = true,
            StarsGradientEnabled = false,
            StarsCount = 20,
            StarsHaloFalloffMin = 40.0f,
            StarsHaloFalloffMax = 300,
            StarsHaloFalloff = 200,

            SunsEnabled = false,
            SunsCount = 1,
            SunsCoreSize = 5,
            SunsHaloFalloff = 200,
            SunsGradientEnabled = false,
            SunsColorGradient = new Gradient(),

            FaceSize = 1024,
            SelectedFaceSize = 1,
            AssetType = AssetType.PNG,
            Filename = "space",
            Path = "Textures/",
            IncludeSeed = false,
            IncludeFaceSize = true,
            SaveJson = true
        };

        private struct Limits
        {
            public const int NebulaCountMax = 10;
            public const int NebulaCountMin = 1;
            public const float NebulaFalloffMax = 20.0f;
            public const float NebulaFalloffMin = 0.1f;
            public const float NebulaDensityMax = 4.0f;
            public const float NebulaDensityMin = 0.1f;
            public const float NebulaFrequencyMax = 10;
            public const float NebulaFrequencyMin = 0.1f;
            public const int OctavesMax = 12;
            public const int OctavesMin = 1;
            public const int SunsCountMax = 10;
            public const int SunsCountMin = 1;
            public const int SunsCoreSizeMax = 20;
            public const int SunsCoreSizeMin = 1;
            public const int SunsHaloFalloffMin = 50;
            public const int SunsHaloFalloffMax = 600;
            public const float NebulaShapeFrequencyMax = 10.0f;
            public const float NebulaShapeFrequencyMin = 0.1f;
        }
    }
}
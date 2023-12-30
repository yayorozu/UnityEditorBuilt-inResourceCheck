using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTool
{
    public class EditorBuiltinResourcesCheckWindow : EditorWindow
    {
        [MenuItem("Tools/Built-in Resources")]
        private static void ShowWindow()
        {
            var window = GetWindow<EditorBuiltinResourcesCheckWindow>();
            window.titleContent = new GUIContent("Built-in Resources");
            window.Show();
        }

        [SerializeField]
        private Texture[] _textures;
        private List<GUIStyle> _editorGUIStyles;
        
        private Vector2 _scrollPosition;
        
        private readonly string[] _tabToggles = { "Texture", "GUIStyles"};
        
        private int _tabIndex;
        private Vector2 _sizeRange =new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight * 4);
        private float _sizeRate = 0f;
        private SearchField _search;
        private string _searchText;

        private static class Styles
        {
            public static readonly GUILayoutOption SliderWidth = GUILayout.Width(100f);
            public static readonly GUILayoutOption SearchWidth = GUILayout.Width(200f);
        }

        private void OnEnable()
        {
            var targetPaths = new string[]
            {
                "Library/unity editor resources",
                "Library/unity_builtin_extra"
            };
            foreach (var path in targetPaths)
            {
                AssetDatabase.LoadAllAssetsAtPath(path);
            }
            
            _textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D))
                .Where(x => targetPaths.Contains(AssetDatabase.GetAssetPath(x)))
                .Select(v => v.name)
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .OrderBy(v => v)
                .Select(x => EditorGUIUtility.Load(x) as Texture2D)
                .Where(x => x)
                .ToArray();
        }

        private void OnDisable()
        {
            _textures = null;
        }
        
        private void OnGUI()
        {
            _search ??= new SearchField();
            
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton),
                    GUI.ToolbarButtonSize.FitToContents);

                if (_tabIndex == 0)
                {
                    _sizeRate = GUILayout.HorizontalSlider(_sizeRate, 0f, 1f, Styles.SliderWidth);
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _searchText = _search.OnToolbarGUI(_searchText, Styles.SearchWidth);
                    if (check.changed)
                    {
                        Apply();
                    }
                }
            }

            if (_tabIndex == 0)
            {
                DrawTexture();
            }
            else if (_tabIndex == 1)
            {
                DrawGUIStyle();
            }
        }

        private void Apply()
        {
        }

        private void DrawTexture()
        {
            var windowWidth = position.width - 10f;
            var gridSize = Mathf.Lerp(_sizeRange.x, _sizeRange.y, _sizeRate);
            var rowCount = Mathf.FloorToInt(windowWidth / (gridSize + 10));
            var totalSpace = windowWidth - rowCount * gridSize; 
            var margin = totalSpace / (rowCount + 1);
            var loopCount = Mathf.CeilToInt(_textures.Length / (float)rowCount);
            float height = 0;
            float width = 0;
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                var index = 0;
                Rect rect = new Rect();
                foreach (var texture in _textures
                             .Where(v => string.IsNullOrEmpty(_searchText) || v.name.Contains(_searchText, StringComparison.Ordinal)))
                {
                    if (index++ % rowCount == 0)
                    {
                        rect = GUILayoutUtility.GetRect(windowWidth - 14, gridSize);
                    }
                    
                    // アスヒによって制御する
                    if (texture.width > texture.height)
                    {
                        width = Mathf.Min(
                            Mathf.Max(texture.width, EditorGUIUtility.singleLineHeight),
                            gridSize
                        );
                        height = texture.height * width / texture.width;                            
                    }
                    else
                    {
                        height = Mathf.Min(
                            Mathf.Max(texture.height, EditorGUIUtility.singleLineHeight),
                            gridSize
                        );
                        width = texture.width * height / texture.height;
                    }

                    rect.xMin = margin + (index % rowCount) * (gridSize + margin);
                    rect.height = height;
                    rect.width = width;

                    var content = new GUIContent("", texture.name);
                    if (GUI.Button(rect, content, EditorStyles.label))
                    {
                        GUIUtility.systemCopyBuffer = $"EditorGUIUtility.Load(\"{texture.name}\")";
                        Debug.Log(texture.name);
                    }

                    GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
                    
                }
            }
        }

        private void DrawGUIStyle()
        {
            if (_editorGUIStyles == null || _editorGUIStyles.Count == 0)
            {
                _editorGUIStyles = new List<GUIStyle>();
                var e = GUI.skin.GetEnumerator();
                while (e.MoveNext())
                {
                    try
                    {
                        _editorGUIStyles.Add(e.Current as GUIStyle);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            var width = GUILayout.Width(300f);
            var label = "GUIStyle";
            using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                foreach (var style in _editorGUIStyles
                             .Where(v => string.IsNullOrEmpty(_searchText) || v.name.Contains(_searchText, StringComparison.Ordinal)))
                {
                    using (new EditorGUILayout.HorizontalScope("box"))
                    {
                        if (GUILayout.Button(style.name, EditorStyles.label, width))
                        {
                            Debug.Log(style.name);
                        }

                        GUILayout.Button(GUIContent.none, style, GUILayout.ExpandWidth(true));
                        GUILayout.Space(5);
                        GUILayout.Button(label, style, GUILayout.ExpandWidth(true));
                    }
                }
            }
        }
    }
}

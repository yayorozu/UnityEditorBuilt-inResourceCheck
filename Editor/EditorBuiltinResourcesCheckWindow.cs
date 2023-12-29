using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        
        private Vector2 _scrollPosition;
        
        private readonly string[] _tabToggles = { "Texture", "GUIStyles"};
        private int _tabIndex;

        private Vector2 SizeRange =new Vector2(EditorGUIUtility.singleLineHeight * 2, EditorGUIUtility.singleLineHeight * 4);
        private float _sizeRate = 0f;

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
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _tabIndex = GUILayout.Toolbar(_tabIndex, _tabToggles, new GUIStyle(EditorStyles.toolbarButton),
                    GUI.ToolbarButtonSize.FitToContents);

                _sizeRate = GUILayout.HorizontalSlider(_sizeRate, 0f, 1f, GUILayout.Width(100));
            }

            if (_tabIndex == 0)
            {
                DrawTexture();
            }
            else if (_tabIndex == 1)
            {
                
            }
        }

        private void DrawTexture()
        {
            var windowWidth = position.width - 10f;
            var size = Mathf.Lerp(SizeRange.x, SizeRange.y, _sizeRate);
            var rowCount = Mathf.FloorToInt(windowWidth / size);
            var loopCount = Mathf.CeilToInt(_textures.Length / rowCount);
            float height = 0;
            float width = 0;
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                for (var i = 0; i < loopCount; i++)
                {
                    // GetRect
                    var rect = GUILayoutUtility.GetRect(windowWidth - 14, size);
                    for (int j = 0; j < rowCount; j++)
                    {
                        int index = i * rowCount + j;
                        if (index >= _textures.Length)
                            continue;
                        
                        var texture = _textures[index];

                        // アスヒによって制御する
                        if (texture.width > texture.height)
                        {
                            width = Mathf.Min(
                                Mathf.Max(texture.width, EditorGUIUtility.singleLineHeight),
                                size
                            );
                            height = texture.height * width / texture.width;                            
                        }
                        else
                        {
                            height = Mathf.Min(
                                Mathf.Max(texture.height, EditorGUIUtility.singleLineHeight),
                                size
                            );
                            width = texture.width * height / texture.height;
                        }

                        rect.xMin = j * size;
                        rect.height = height;
                        rect.width = width;

                        var content = new GUIContent("", texture.name);
                        if (GUI.Button(rect, content, EditorStyles.label))
                        {
                            GUIUtility.systemCopyBuffer = $"EditorGUIUtility.Load(\"{texture.name}\")";
                        }

                        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
                    }
                }
            }
        }
    }
}

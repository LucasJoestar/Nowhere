﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SuperSpriteEditor : EditorWindow
{
    #region Fields / Properties
    /// <summary>
    /// Get the path where to save color groups files.
    /// </summary>
    private static string savePath { get { return Application.dataPath + "/Resources/Editor/Color Groups/"; } }

    /// <summary>
    /// Indicates if editor manipulation should also take selected objects children sprites.
    /// </summary>
    [SerializeField] private bool doUseChildrenSprites = true;

    /// <summary>
    /// Value used to increment sprites layer.
    /// </summary>
    [SerializeField] private int layerIncrementValue = 0;

    /// <summary>
    /// The index of the selected color group.
    /// </summary>
    [SerializeField] private int selectedGroup = -1;

    /// <summary>
    /// Scrollbar of the window.
    /// </summary>
    [SerializeField] private Vector2 scrollbar = new Vector2();

    /// <summary>
    /// Indicates if the user has selected one group or more for fusion.
    /// </summary>
    [SerializeField] private bool isInFusionMode = false;

    /// <summary>
    /// Groups of loaded sprite renderers sorted by color.
    /// </summary>
    [SerializeField] private ColorGroup[] colorGroups = new ColorGroup[] { };
    #endregion

    #region Methods

    #region Original Methods

    #region Menus
    /// <summary>
    /// Method to call this window from the Unity toolbar.
    /// </summary>
    [MenuItem("Window/Sprites Editor")]
    public static void CallWindow() => GetWindow<SuperSpriteEditor>("Sprites Editor").Show();

    /// <summary>
    /// Draws this editor.
    /// </summary>
    public void DrawEditor()
    {
        DrawToolbar();

        DrawGeneralMode();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("________________________________________________________________________________________________________________________________________________________________________________________________________________________________________________", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        DrawColorMode();
    }

    /// <summary>
    /// Draws the general mode.
    /// </summary>
    public void DrawGeneralMode()
    {
        GUILayout.Space(15);

        EditorGUILayout.LabelField(new GUIContent("General", "General Sprites Edition Section"), EditorStyles.boldLabel);

        if (Selection.gameObjects.Length == 0)
        {
            EditorGUILayout.HelpBox("No sprite actually selected !", MessageType.Info);
            return;
        }

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);

        doUseChildrenSprites = EditorGUILayout.Toggle(new GUIContent("Use children sprites", "Should the manipulations automatically also select the selected GameObjects sprites children or not"), doUseChildrenSprites);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Increment Layer", "Increment the selected GameObjects layer by the above number"), GUILayout.Height(25), GUILayout.Width(200)))
        {
            SpriteRenderer[] _sprites = null;
            SpriteMask[] _masks = null;
            if (doUseChildrenSprites)
            {
                _sprites = Selection.gameObjects.ToList().SelectMany(s => s.GetComponentsInChildren<SpriteRenderer>()).ToArray();
                _masks = Selection.gameObjects.ToList().SelectMany(s => s.GetComponentsInChildren<SpriteMask>()).ToArray();
            }
            else
            {
                _sprites = Selection.gameObjects.ToList().Select(g => g.GetComponent<SpriteRenderer>()).Distinct().ToArray();
                _masks = Selection.gameObjects.ToList().Select(s => s.GetComponent<SpriteMask>()).Distinct().ToArray();
            }

            if (_sprites != null)
            {
                Undo.RecordObjects(_sprites, "Increment Sprites layer order");

                foreach (SpriteRenderer _sprite in _sprites)
                {
                    _sprite.sortingOrder += layerIncrementValue;
                }
            }

            if (_masks != null)
            {
                Undo.RecordObjects(_masks, "Increment Sprites layer order");

                foreach (SpriteMask _mask in _masks)
                {
                    _mask.backSortingOrder += layerIncrementValue;
                    _mask.frontSortingOrder += layerIncrementValue;
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);

        if ((Selection.gameObjects.Length > 0) && GUILayout.Button("Debug Sprite(s) Group(s)", GUILayout.Width(175)))
        {
            SpriteRenderer[] _sprites = null;
            if (doUseChildrenSprites) _sprites = Selection.gameObjects.ToList().SelectMany(s => s.GetComponentsInChildren<SpriteRenderer>()).ToArray();
            else _sprites = Selection.gameObjects.ToList().Select(g => g.GetComponent<SpriteRenderer>()).Distinct().ToArray();

            if ((_sprites != null) && (_sprites.Length > 0))
            {
                foreach (SpriteRenderer _sprite in _sprites)
                {
                    if (!_sprite) continue;
                    Debug.Log(_sprite.name + " => " + colorGroups.Where(g => g.Sprites.Contains(_sprite)).FirstOrDefault()?.Name);
                }
            }
        }

        GUILayout.FlexibleSpace();

        layerIncrementValue = EditorGUILayout.IntField(new GUIContent("Layer Increment value", "Value used to increment selected objects layer"), layerIncrementValue, GUILayout.Width(200));

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the color mode.
    /// </summary>
    public void DrawColorMode()
    {
        // Get non-assignated color groups and associate them with matching one if found
        for (int _i = 0; _i < colorGroups.Length; _i++)
        {
            colorGroups[_i].Sprites = colorGroups[_i].Sprites.Where(s => (s != null) && s.enabled).ToList();

            SpriteRenderer[] _differents = colorGroups[_i].Sprites.Where(s => s.color != colorGroups[_i].Color).ToArray();

            if (_differents.Length > 0)
            {
                if (_differents.Length == colorGroups[_i].Sprites.Count)
                {
                    colorGroups[_i].Color = _differents[0].color;
                }

                else
                {
                    foreach (SpriteRenderer _sprite in _differents)
                    {
                        colorGroups[_i].Sprites.Remove(_sprite);
                        LoadSprite(_sprite);
                    }
                }
            }
            if (colorGroups[_i].Sprites.Count == 0)
            {
                colorGroups = colorGroups.Where(g => g != colorGroups[_i]).ToArray();
                Repaint();
            }
        }

        EditorGUILayout.LabelField(new GUIContent("Level Colors", "All colors of the loaded sprites in the level"), EditorStyles.boldLabel);

        if (colorGroups.Length == 0)
        {
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(new GUIContent("No sprites loaded !", "Click on the \"Load Sprites\" button to get sprites of the scene, or add some to load them next"));

            EditorGUILayout.EndScrollView();
            return;
        }

        // Draw color groups editor
        scrollbar = EditorGUILayout.BeginScrollView(scrollbar);
        GUILayout.Space(5);

        // Draw all folders & color groups !!
        DrawColorGroups();

        GUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws the toolbar.
    /// </summary>
    public void DrawToolbar()
    {
        // Draw toolbar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button(new GUIContent("Range", "Range the groups by name"), EditorStyles.toolbarButton))
        {
            colorGroups = colorGroups.OrderBy(g => g.Color.grayscale).ToArray();
            colorGroups = colorGroups.OrderBy(g => g.Name).ToArray();
        }
        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("Load Sprites", "Loads all sprites of this level"), EditorStyles.toolbarButton)) LoadSprites();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Reset", "Clear all loaded sprites and get them from zero point"), EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Confirm color groups reset", "Are you sure you want to reset all your color groups ? This action cannot be undone.", "Yes, I'm sure !", "I've changed my mind..."))
            {
                colorGroups = new ColorGroup[] { };
                LoadSprites();
                Repaint();
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("Load", "Load saved groups if existing"), EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Confirm color group load", "Are you sure you want to load saved groups ? Your saved groups will be erased. This action cannot be undone.", "Yes, I'm sure !", "I've changed my mind..."))
            {
                LoadGroups();
            }
        }
        if (GUILayout.Button(new GUIContent("Save", "Save color groups"), EditorStyles.toolbarButton))
        {
            SaveGroups();
        }
        if (GUILayout.Button(new GUIContent("?", "Help box indicating how the tool works."), EditorStyles.toolbarButton))
        {
            EditorUtility.DisplayDialog("Sprite Editor",
                                        "The Sprite Editor window is divided into differents sections.\n\n" +
                      "• Toolbar :\n" +
                      "   - Load Sprites allow you to load newly added sprites to the scene ; this will not affect your actual groups ;\n" +
                      "   - Reset destroys all your current scene groups ;\n" +
                      "   - Load will replace your groups with the one saved on the project ;\n" +
                      "   - Save will save your color groups for the actual scene into a certai folder, by default at : \"Assets/Resources/Editor/Sprite Groups\", but you can change it in this script by modifying the savePath field ;\n\n" + 
                      "• General :\n" +
                      "   - To indicate if the next manipulations should affect selected object only or also its children ;\n" +
                      "   - Increase sprites order in layer ;\n\n" +
                      "• Colors :\n" +
                      "   - Rename your color groups ;\n" +
                      "   - Select groups with the toggle at their right : a green F button now appears left to other groups ; click on it to merge your selected groups into the color of the one you chose with the button.\n" +
                      "   - If you select a game object in the scene, a blue F button appears to merge selected object (with its children if the toggle is checked) into the color group you want.",
                                        "Okay.");
        }

        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region Color
    /// <summary>
    /// Loads all sprite renderers of the level.
    /// </summary>
    public void LoadSprites()
    {
        SpriteRenderer[] _sprites = FindObjectsOfType<SpriteRenderer>().Where(s => s.enabled).ToArray();

        foreach (SpriteRenderer _sprite in _sprites)
        {
            LoadSprite(_sprite);
        }
    }

    /// <summary>
    /// Loads a sprite renderer.
    /// </summary>
    /// <param name="_sprite">Sprite to load.</param>
    private void LoadSprite(SpriteRenderer _sprite)
    {
        ColorGroup _matching = colorGroups.Where(c => c.Color == _sprite.color).FirstOrDefault();
        if (_matching == null)
        {
            colorGroups = colorGroups.Append(new ColorGroup(_sprite.color, new SpriteRenderer[] { _sprite })).OrderBy(g => g.Name).ToArray();
        }
        else if (!_matching.Sprites.Contains(_sprite))
        {
            _matching.Sprites.Add(_sprite);
        }
    }

    /// <summary>
    /// Draw some color groups.
    /// </summary>
    /// <param name="_colorGroups">Groups to draw.</param>
    private void DrawColorGroups()
    {
        ColorGroup _colorGroup;

        for (int _i = 0; _i < colorGroups.Length; _i++)
        {
            _colorGroup = colorGroups[_i];

            if (selectedGroup == _i) GUI.backgroundColor = new Color(.7f, .7f, .7f);
            else GUI.backgroundColor = Color.white;

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal(selectedGroup == _i ? "HelpBox" : EditorStyles.inspectorDefaultMargins);

            // Draw fusion mode buttons
            if (!_colorGroup.isSelected && (isInFusionMode || (Selection.gameObjects.Length > 0)))
            {
                Color _original = GUI.color;
                GUI.color = isInFusionMode ? new Color(0, .75f, 0, 1) : new Color(.5f, .5f, .8f, 1);

                if (GUILayout.Button("F", GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    if (isInFusionMode)
                    {
                        ColorGroup[] _groups = colorGroups.Where(g => g.isSelected).ToArray();

                        foreach (ColorGroup _group in _groups)
                        {
                            Undo.RecordObjects(_group.Sprites.ToArray(), "fusion sprite groups color");

                            foreach (SpriteRenderer _sprite in _group.Sprites)
                            {
                                _sprite.color = _colorGroup.Color;
                            }
                            _colorGroup.Sprites.AddRange(_group.Sprites);
                        }

                        colorGroups = colorGroups.Except(_groups).ToArray();
                        isInFusionMode = false;

                        Repaint();
                    }
                    else
                    {
                        SpriteRenderer[] _sprites = Selection.gameObjects.ToList().SelectMany(s => doUseChildrenSprites ? s.GetComponentsInChildren<SpriteRenderer>() : s.GetComponents<SpriteRenderer>()).ToArray();

                        Undo.RecordObjects(_sprites, "fusion sprite groups color");

                        foreach (SpriteRenderer _sprite in _sprites)
                        {
                            _sprite.color = _colorGroup.Color;
                        }

                        _colorGroup.Sprites.AddRange(_sprites);
                    }
                }

                GUI.color = _original;
            }
            else GUILayout.Space(28);

            // Draw color group
            EditorGUI.BeginChangeCheck();
            _colorGroup.Name = EditorGUILayout.TextField(_colorGroup.Name, GUILayout.MinWidth(100), GUILayout.MaxWidth(500));
            if (EditorGUI.EndChangeCheck()) selectedGroup = _i;

            if (GUILayout.Button($"({_colorGroup.Sprites.Count})", EditorStyles.label, GUILayout.Width(50)))
            {
                foreach (SpriteRenderer _sprite in _colorGroup.Sprites)
                {
                    Debug.Log($"CG \"{_colorGroup.Name}\" => {_sprite.name}");
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            _colorGroup.Color = EditorGUILayout.ColorField(_colorGroup.Color);
            if (EditorGUI.EndChangeCheck())
            {
                selectedGroup = _i;

                Undo.RecordObjects(_colorGroup.Sprites.ToArray(), "change sprites group color");
                _colorGroup.Sprites.ForEach(s => s.color = _colorGroup.Color);
            }

            bool _selected = EditorGUILayout.Toggle(_colorGroup.isSelected, GUILayout.Width(15));
            if (_selected != _colorGroup.isSelected)
            {
                _colorGroup.isSelected = _selected;
                if (_selected)
                {
                    if (!isInFusionMode) isInFusionMode = true;
                }
                else if (!colorGroups.Any(g => g.isSelected)) isInFusionMode = false;
            }

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Loads saved groups.
    /// </summary>
    private void LoadGroups()
    {
        Directory.CreateDirectory(savePath);

        string _path = savePath + EditorSceneManager.GetActiveScene().name + ".txt";

        // If the file doesn't exist, return
        if (!File.Exists(_path)) return;

        string[] _groups = File.ReadAllLines(_path).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        string _groupName = string.Empty;
        string[] _groupColor = new string[] { };

        colorGroups = new ColorGroup[_groups.Length];

        for (int _i = 0; _i < _groups.Length; _i++)
        {
            _groupName = _groups[_i].Split('|')[0];
            _groupColor = _groups[_i].Split('|')[1].Split('#');

            colorGroups[_i] = new ColorGroup(new Color(float.Parse(_groupColor[0]), float.Parse(_groupColor[1]), float.Parse(_groupColor[2]), float.Parse(_groupColor[3])), _groupName);
        }

        LoadSprites();
    }

    /// <summary>
    /// Save groups.
    /// </summary>
    private void SaveGroups()
    {
        string _file = string.Empty;
        foreach (ColorGroup _group in colorGroups)
        {
            _file += $"{_group.Name}|{_group.Color.r}#{_group.Color.g}#{_group.Color.b}#{_group.Color.a}\n";
        }
        Directory.CreateDirectory(savePath);
        File.WriteAllText(savePath + EditorSceneManager.GetActiveScene().name + ".txt", _file);
    }
    #endregion

    #endregion

    #region Unity Methods
    // This function is called when the object is loaded
    private void OnEnable()
    {
        // Load sprites on enable
        LoadSprites();
    }

    // Implement your own editor GUI here
    private void OnGUI()
    {
        DrawEditor();
    }
    #endregion

    #endregion
}

[Serializable]
public class ColorGroup
{
    #region Fields / Properties
    /// <summary>
    /// Name of this group.
    /// </summary>
    public string Name = "? New Color Group ?";

    /// <summary>
    /// Color associated with the class.
    /// </summary>
    public Color Color;

    /// <summary>
    /// Sprites with the color.
    /// </summary>
    public List<SpriteRenderer> Sprites = new List<SpriteRenderer>();

    /// <summary>
    /// Is this group selected in the window or not.
    /// </summary>
    public bool isSelected = false;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new Color Group.
    /// </summary>
    public ColorGroup()
    {
        Color = Color.white;
        Sprites = new List<SpriteRenderer>();
    }

    /// <summary>
    /// Creates a new Color Group.
    /// </summary>
    /// <param name="_color">Color of the group.</param>
    public ColorGroup(Color _color)
    {
        Color = _color;
        Sprites = new List<SpriteRenderer>();
    }

    /// <summary>
    /// Creates a new Color Group.
    /// </summary>
    /// <param name="_color">Color of the group.</param>
    /// <param name="_name">Name of the group.</param>
    public ColorGroup(Color _color, string _name)
    {
        Color = _color;
        Name = _name;
        Sprites = new List<SpriteRenderer>();
    }

    /// <summary>
    /// Creates a new Color Group.
    /// </summary>
    /// <param name="_color">Color of the group.</param>
    /// <param name="_sprites">Sprites in this group.</param>
    public ColorGroup(Color _color, IEnumerable<SpriteRenderer> _sprites)
    {
        Color = _color;
        Sprites = _sprites.ToList();
    }
    #endregion
}

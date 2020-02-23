using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(UpdateSystem))]
public class UpdateSystemEditor : Editor
{
    #region Fields
    /**********************************
     *********     FIELDS     *********
     *********************************/

    /// <summary>
    /// Reorderable list for the <see cref="updateModes"/> serialized property.
    /// </summary>
    private ReorderableList     updateModesReorderableList =    null;

    /// <summary>
    /// Serialized property for <see cref="UpdateSystem.UpdateModes"/> of type <see cref="UpdateMode"/>[].
    /// </summary>
    private SerializedProperty  updateModes =                   null;
    #endregion

    #region Memory
    /**********************************
     *********     MEMORY     *********
     *********************************/

    /// <summary>
    /// Used to create foldouts for each reorderable list element.
    /// </summary>
    private bool[]              foldouts =                      new bool[] { };
    #endregion

    #region Methods

    #region Callback Methods
    /************************************
     *****     CALLBACK METHODS     *****
     ***********************************/

    /// <summary>
    /// Draw a reorderable list element.
    /// Used with <see cref="ReorderableList.drawElementCallback"/> callback.
    /// </summary>
    /// <param name="_rect">Rect where to draw element.</param>
    /// <param name="_index">Index of the element within the list.</param>
    /// <param name="_isActive">Is the element active.</param>
    /// <param name="_isFocused">Is the element focused.</param>
    private void DrawElement(Rect _rect, int _index, bool _isActive, bool _isFocused)
    {
        SerializedProperty _element = updateModes.GetArrayElementAtIndex(_index);
        GUIContent _content = new GUIContent($"[{_index + 1}] - {ObjectNames.NicifyVariableName(((UpdateModeTimeline)_element.FindPropertyRelative("timeline").enumValueIndex).ToString())}");

        Rect _drawRect = new Rect(_rect.x + 10, _rect.y + 2, Mathf.Min(EditorStyles.label.CalcSize(_content).x, _rect.width), 20);
        foldouts[_index] = EditorGUI.Foldout(_drawRect, foldouts[_index], _content, true);

        if (foldouts[_index])
        {
            _drawRect = new Rect()
            {
                x = _drawRect.x + 25,
                y = _drawRect.y + 20,
                width = _rect.width - 50,
                height = EditorGUIUtility.singleLineHeight
            };

            EditorGUI.PropertyField(_drawRect, _element.FindPropertyRelative("isFrameInterval"));
            _drawRect.y += _drawRect.height + 5;
            EditorGUI.PropertyField(_drawRect, _element.FindPropertyRelative("updateInterval"));
        }

        if (_index < (updateModesReorderableList.count - 1)) EditorGUI.DrawRect(new Rect(_rect.x, _rect.y + (_rect.height - 3), _rect.width, 1), Color.gray);
    }

    /// <summary>
    /// Draw reorderable list header.
    /// Used with <see cref="ReorderableList.drawHeaderCallback"/> callback.
    /// </summary>
    /// <param name="_rect">Rect where to draw header.</param>
    private void DrawHeader(Rect _rect)
    {
        EditorGUI.LabelField(_rect, new GUIContent($"Update Modes [{updateModesReorderableList.count}] :"));
    }


    /// <summary>
    /// Get reorderable list element height at given index.
    /// Used with <see cref="ReorderableList.elementHeightCallback"/> callback.
    /// </summary>
    /// <param name="_index">Index of the element.</param>
    /// <returns>Returns element height, in pixels.</returns>
    private float GetElementHeight(int _index) => foldouts[_index] ? 70 : 25;
    #endregion

    #region Unity Methods
    /***********************************
     ******     UNITY METHODS     ******
     **********************************/

    // 	This function is called when the object is loaded
    private void OnEnable()
    {
        // Get serialized property from serialized object
        updateModes = serializedObject.FindProperty("updateModes");

        // Check there exist an update mode for each update mode timeline
        List<int> _timelines = new List<int>();
        foreach (UpdateModeTimeline _timeline in Enum.GetValues(typeof(UpdateModeTimeline)))
        {
            _timelines.Add((int)_timeline);
        }

        for (int _i = 0; _i < updateModes.arraySize; _i++)
        {
            SerializedProperty _updateMode = updateModes.GetArrayElementAtIndex(_i);
            int _timeline = _updateMode.FindPropertyRelative("timeline").enumValueIndex;

            if (_timelines.Contains(_timeline))
            {
                _timelines.Remove(_timeline);
            }
            else
            {
                updateModes.DeleteArrayElementAtIndex(_i);
                _i--;
            }
        }
        for (int _i = 0; _i < _timelines.Count; _i++)
        {
            updateModes.InsertArrayElementAtIndex(0);
            updateModes.GetArrayElementAtIndex(0).FindPropertyRelative("timeline").enumValueIndex = _timelines[_i];
        }

        // Apply modifications to serialized object
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        // Create a reorderable list associated with the update mode array.
        updateModesReorderableList = new ReorderableList(serializedObject, updateModes, true, true, false, false);
        updateModesReorderableList.drawHeaderCallback += DrawHeader;
        updateModesReorderableList.drawElementCallback += DrawElement;
        updateModesReorderableList.elementHeightCallback += GetElementHeight;

        foldouts = new bool[updateModesReorderableList.count];
    }

    // Implement this function to make a custom inspector
    public override void OnInspectorGUI()
    {
        // Update to get latest object values
        serializedObject.Update();

        GUILayout.Space(5);
        updateModesReorderableList.DoLayoutList();

        // Apply modifications
        serializedObject.ApplyModifiedProperties();
    }
    #endregion

    #endregion
}

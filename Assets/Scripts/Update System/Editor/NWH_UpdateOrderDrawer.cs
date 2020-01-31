using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(NWH_UpdateOrder))]
public class NWH_UpdateOrderDrawer : PropertyDrawer
{
    Dictionary<string, ReorderableList> _test = new Dictionary<string, ReorderableList>();

    #region Methods

    #region Original Methods
    private ReorderableList LoadList(SerializedProperty _property)
    {
        return new ReorderableList(_property.serializedObject, _property.FindPropertyRelative("updateModes"), true, true, true, true);
    }
    #endregion

    #region Unity Methods
    public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
    {
        return 500;
    }

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        string _key = _property.serializedObject.targetObject.GetInstanceID() + "/" + _property.name;

        if (!_test.ContainsKey(_key))
        {
            ReorderableList reorderableList = new ReorderableList(_property.serializedObject, _property.FindPropertyRelative("updateModes"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, string.Format("{0}: {1}", _label.text, _property.FindPropertyRelative("updateModes").arraySize), EditorStyles.boldLabel);
                },

                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    SerializedProperty element = _property.FindPropertyRelative("updateModes").GetArrayElementAtIndex(index);
                    rect.y += 1.0f;
                    rect.x += 10.0f;
                    rect.width -= 10.0f;

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, 0.0f), element, true);
                },

                elementHeightCallback = (int index) =>
                {
                    return EditorGUI.GetPropertyHeight(_property.FindPropertyRelative("updateModes").GetArrayElementAtIndex(index)) + 4.0f;
                }
            };

            _test[_key] = reorderableList;
        }

        _test[_key].DoList(_position);
    }
    #endregion

    #endregion
}

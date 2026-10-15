using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
[ExcludeFromCodeCoverage]
// Mark a field so the Inspector can choose a managed-reference subclass.
public class SubclassPicker : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SubclassPicker))]
[ExcludeFromCoverage]
[ExcludeFromCodeCoverage]
public class SubclassPickerDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUI.GetPropertyHeight(property);
  }

  IEnumerable<Type> GetClasses(Type baseType)
  {
    // List concrete subclasses of the base type in the same assembly.
    return Assembly
      .GetAssembly(baseType)
      .GetTypes()
      .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // The menu can choose None or instantiate a subclass in the Inspector.
    var t = fieldInfo.FieldType;
    if (t.IsArray && t.FullName.EndsWith("[]"))
    {
      t = Type.GetType(t.FullName.Substring(0, t.FullName.Length - 2));
    }
    var typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";

    var dropdownRect = position;
    dropdownRect.x += EditorGUIUtility.labelWidth + 2;
    dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
    dropdownRect.height = EditorGUIUtility.singleLineHeight;
    if (EditorGUI.DropdownButton(dropdownRect, new(typeName), FocusType.Keyboard))
    {
      var menu = new GenericMenu();

      // null
      menu.AddItem(
        new GUIContent("None"),
        property.managedReferenceValue == null,
        () =>
        {
          property.managedReferenceValue = null;
          property.serializedObject.ApplyModifiedProperties();
        }
      );

      // inherited types
      foreach (var type in GetClasses(t))
      {
        menu.AddItem(
          new GUIContent(type.Name),
          typeName == type.Name,
          () =>
          {
            property.managedReferenceValue = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            property.serializedObject.ApplyModifiedProperties();
          }
        );
      }
      menu.ShowAsContext();
    }
    EditorGUI.PropertyField(position, property, label, true);
  }
}
#endif

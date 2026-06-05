#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ErmineGames.Features.Editor
{
    [CustomEditor(typeof(FeaturesSettings))]
    public class FeaturesSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty featuresProp;
        private List<Type> availableFeatureTypes;

        private void OnEnable()
        {
            featuresProp = serializedObject.FindProperty("features");
            CacheFeatureTypes();
        }

        private void CacheFeatureTypes()
        {
            availableFeatureTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(Feature).IsAssignableFrom(p) && !p.IsAbstract && p != typeof(Feature))
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Features List", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (featuresProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No features added. Click 'Add Feature' to add one.", MessageType.Info);
            }

            for (int i = 0; i < featuresProp.arraySize; i++)
            {
                DrawFeatureRecord(i);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Feature", GUILayout.Height(30)))
            {
                featuresProp.arraySize++;

                var newRecord = featuresProp.GetArrayElementAtIndex(featuresProp.arraySize - 1);
                newRecord.FindPropertyRelative("IsEnabled").boolValue = true;
                newRecord.FindPropertyRelative("FeatureTypeName").stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFeatureRecord(int index)
        {
            var recordProp = featuresProp.GetArrayElementAtIndex(index);
            var isEnabledProp = recordProp.FindPropertyRelative("IsEnabled");
            var featureTypeNameProp = recordProp.FindPropertyRelative("FeatureTypeName");
            var settingsProp = recordProp.FindPropertyRelative("Settings");

            var featureType = string.IsNullOrEmpty(featureTypeNameProp.stringValue)
                ? null
                : Type.GetType(featureTypeNameProp.stringValue);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(5));
            var isExpanded = featuresProp.GetArrayElementAtIndex(index).isExpanded;
            isExpanded = EditorGUILayout.Foldout(isExpanded, string.Empty, true, EditorStyles.foldoutHeader);
            featuresProp.GetArrayElementAtIndex(index).isExpanded = isExpanded;
            EditorGUILayout.EndHorizontal();

            isEnabledProp.boolValue = EditorGUILayout.Toggle(isEnabledProp.boolValue, GUILayout.Width(30));
            DrawTypeDropdown(featureTypeNameProp, index);

            if (GUILayout.Button("x", GUILayout.Width(20)))
            {
                featuresProp.DeleteArrayElementAtIndex(index);
                return;
            }

            EditorGUILayout.EndHorizontal();


            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                if (featureType != null && HasSettings(featureType))
                {
                    EditorGUILayout.Space();
                    DrawSettingsField(featureType, settingsProp);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawTypeDropdown(SerializedProperty featureTypeNameProp, int recordIndex)
        {
            var currentTypeName = featureTypeNameProp.stringValue;
            var currentType = string.IsNullOrEmpty(currentTypeName) ? null : Type.GetType(currentTypeName);
            var currentLabel = currentType != null
                ? ObjectNames.NicifyVariableName(currentType.Name)
                : "Select feature";
            
            if (EditorGUILayout.DropdownButton(new GUIContent(currentLabel), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), currentType == null, () =>
                {
                    featureTypeNameProp.stringValue = string.Empty;

                    // Очищаем Settings при смене типа
                    var recordProp = featuresProp.GetArrayElementAtIndex(recordIndex);
                    var settingsProp = recordProp.FindPropertyRelative("Settings");
                    settingsProp.managedReferenceValue = null;

                    serializedObject.ApplyModifiedProperties();
                });

                menu.AddSeparator("");

                foreach (var featureType in availableFeatureTypes.OrderBy(t => t.Name))
                {
                    var typeName = featureType.AssemblyQualifiedName;
                    var displayName = ObjectNames.NicifyVariableName(featureType.Name);
                    menu.AddItem(new GUIContent(displayName), currentType == featureType, () =>
                    {
                        featureTypeNameProp.stringValue = typeName;

                        // Создаём новый Settings при смене типа (если есть)
                        CreateSettingsForFeature(featureType, recordIndex);

                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
        }

        private void CreateSettingsForFeature(Type featureType, int recordIndex)
        {
            // Получаем тип Settings из Generic параметра Feature
            if (featureType.BaseType?.GenericTypeArguments.Length > 0)
            {
                var settingsType = featureType.BaseType.GenericTypeArguments[0];

                // Создаём экземпляр Settings
                var settingsInstance = Activator.CreateInstance(settingsType);

                var recordProp = featuresProp.GetArrayElementAtIndex(recordIndex);
                var settingsProp = recordProp.FindPropertyRelative("Settings");
                settingsProp.managedReferenceValue = settingsInstance;
            }
        }

        private void DrawSettingsField(Type featureType, SerializedProperty settingsProp)
        {
            if (HasSettings(featureType))
            {
                EditorGUILayout.BeginHorizontal();
                var settingsType = featureType.BaseType.GenericTypeArguments[0];
                EditorGUILayout.LabelField($"{settingsType.Name}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawPropertyWithoutFoldout(settingsProp);
            }
        }

        private void DrawPropertyWithoutFoldout(SerializedProperty property)
        {
            SerializedProperty endProperty = property.GetEndProperty();
            property.NextVisible(true);

            while (!SerializedProperty.EqualContents(property, endProperty))
            {
                EditorGUILayout.PropertyField(property, true);
                property.NextVisible(false);
            }
        }

        private bool HasSettings(Type featureType)
        {
            return featureType.BaseType?.GenericTypeArguments.Length > 0;
        }
    }
}
#endif

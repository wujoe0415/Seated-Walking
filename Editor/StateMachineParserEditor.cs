using LocomotionStateMachine;
using UnityEditor;
using UnityEngine;
using static LocomotionStateMachine.LocomotionParser;

namespace LocomotionStateMachine
{
    [CustomEditor(typeof(LocomotionParser))]
    public class StateMachineParserEditor : Editor
    {
        private SerializedProperty containerProp;
        private LocomotionParser parser;
        private bool isContainerChanged = false;

        private void OnEnable()
        {
            // Cache the target and property references
            parser = (LocomotionParser)target;
            containerProp = serializedObject.FindProperty("Container");

            // Monitor changes in the StateMachineContainer reference
            EditorApplication.update += CheckContainerChanges;
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckContainerChanges;
        }

        // Method to check if the container has been changed or updated
        private void CheckContainerChanges()
        {
            if (containerProp.objectReferenceValue != null)
            {
                StateMachineContainer newContainer = containerProp.objectReferenceValue as StateMachineContainer;

                // Check if the container has changed
                if (newContainer != parser.Container)
                {
                    parser.Container = newContainer;
                    isContainerChanged = true;
                    Repaint(); // Repaint inspector to show updates
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw the default container field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(containerProp, new GUIContent("State Machine Container"));

            // Check if container was modified
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                isContainerChanged = true;
                Debug.Log("Container Change");
            }

            // If container changed, update LocomotionStateName list
            if (isContainerChanged && parser.Container != null)
            {
                isContainerChanged = false;
                DrawLocomotionStateNameFields();
            }
            else if (parser.Container != null)
            {
                DrawLocomotionStateNameFields();
            }
        }

        // Method to display LocomotionStateName fields and MonoBehaviour slots
        private void DrawLocomotionStateNameFields()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Locomotion State", EditorStyles.boldLabel);
            //GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Function", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Display each LocomotionStateName with a MonoBehaviour field
            for(int i = 0; i < parser.StateMaps.Count; i++) { 
                EditorGUILayout.BeginHorizontal();

                // Show the LocomotionStateName
                EditorGUILayout.LabelField(parser.StateMaps[i].key, GUILayout.Width(150));

                // Add a MonoBehaviour slot for assigning related MonoBehaviour
                MonoBehaviour currentMonoBehaviour = parser.StateMaps[i].value;
                parser.StateMaps[i].value = (LocomotionState)EditorGUILayout.ObjectField(
                    currentMonoBehaviour,
                    typeof(LocomotionState),
                    true
                );

                EditorGUILayout.EndHorizontal();
            }

            // Apply any changes made to the fields
            if (GUI.changed)
            {
                EditorUtility.SetDirty(parser);
                EditorUtility.SetDirty(parser.Container);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
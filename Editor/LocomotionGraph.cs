using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

// Credit to https://www.youtube.com/redirect?event=video_description&redir_token=QUFFLUhqa3ZLZXV4MFQwUWtKRUxNdzRnM2xKUUZVRmExd3xBQ3Jtc0tsTmxhWTJlNVh6ZFdJeGYzc08xZlA1ZVVhLWJZLTJ5QVE4THBHaU1Xdm1NNlFNdDgtZldYMDJDZGY2dFFhRGhsd0tXLXdhVDJrOTFVYmxabGNuYTluWFJQZmJaSVlZNU51cm9KSnZ4Q2p3MExrbFFuUQ&q=https%3A%2F%2Fgithub.com%2Fm3rt32%2FNodeBasedDialogueSystem&v=7KHGH0fPL84

namespace LocomotionStateMachine
{
    public class LocomotionGraph : EditorWindow
    {
        private string _fileName = "DefaultSeatedWalking";
        private LocomotionGraphView _graphView;

        [MenuItem("Graph/LocomotionStateMachine")]
        public static void CreateGraphViewWindow()
        {
            LocomotionGraph window = (LocomotionGraph)EditorWindow.GetWindow(typeof(LocomotionGraph), true, "Locomotion StateMachine");
            // titleContent
        }
        private void ConstructGraphView()
        {
            _graphView = new LocomotionGraphView(this)
            {
                name = "Locomotion StateMachine",
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
        private void GenerateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            TextField fileNameTextField = new TextField("File Name:");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => SaveData()) { text = "Save Data" });
            toolbar.Add(new Button(() => LoadData()) { text = "Load Data" });

            rootVisualElement.Add(toolbar);
        }
        private void SaveData()
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }
            
            GraphSaveUtility.GetInstance(_graphView).SaveGraph(_fileName);
        }
        private void LoadData()
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
                return;
            }
            GraphSaveUtility.GetInstance(_graphView).LoadGraph(_fileName);
        }

        private void GenerateMiniMap()
        {
            MiniMap miniMap = new MiniMap { anchored = false};
            miniMap.SetPosition(new Rect(10, 30, 150, 100));
            miniMap.maxHeight = 100;
            miniMap.maxWidth = 150;
            miniMap.elementTypeColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            miniMap.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            miniMap.style.color = Color.white;
            _graphView.Add(miniMap); 
        }
        private void GenerateBlackBoard()
        {
            Blackboard blackboard = new Blackboard(_graphView);
            blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
            blackboard.addItemRequested = _blackboard =>
            {
                _graphView.AddPropertyToBlackBoard(ExposedProperty.CreateInstance(), false);
            };
            blackboard.editTextRequested = (_blackboard, element, newValue) =>
            {
                string oldPropertyName = ((BlackboardField)element).text;
                if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                        "OK");
                    return;
                }

                int targetIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
                _graphView.ExposedProperties[targetIndex].PropertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10, 30, 200, 300));
            _graphView.Add(blackboard);
            _graphView.Blackboard = blackboard;
        }
        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMiniMap();
            //GenerateBlackBoard();
        }
        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}

// Designed by Kinemation, 2023

using System;
using System.Collections.Generic;
using System.Reflection;
using Kinemation.FPSFramework.Runtime.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kinemation.FPSFramework.Editor.Core
{
    [CustomEditor(typeof(CoreAnimComponent), true)]
    public class CoreAnimComponentEditor : UnityEditor.Editor
    {
        // Collection of all classes derived from Anim Layer
        private List<Type> layerTypes;
        private int selectedLayer = -1;
        
        // Interactable Anim Layers
        private ReorderableList layersReorderable;
        private CoreAnimComponent owner;
        // Inspector of the currently selected anim layer
        private UnityEditor.Editor layerEditor;
        
        private void OnEnable()
        {
            owner = (CoreAnimComponent) target;
        
            layersReorderable = new ReorderableList(serializedObject, serializedObject.FindProperty("animLayers"), 
                true, true, true, true);

            layersReorderable.drawHeaderCallback += DrawHeader;
            layersReorderable.drawElementCallback += DrawElement;
            layersReorderable.onSelectCallback += OnSelectElement;
            layersReorderable.onRemoveCallback += OnRemoveCallback;
            layersReorderable.onAddCallback += OnAddCallback;

            // Used to hide layers which reside in the Core component
            HideRegisteredLayers();
            EditorUtility.SetDirty(target);
            Repaint();
        }
        
        // Draws a header of the reordererable list
        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Layers");
        }

        // Draws an element of the reordererable list
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = layersReorderable.serializedProperty.GetArrayElementAtIndex(index);
        
            Type type = element.objectReferenceValue.GetType();
            rect.y += 2;
            GUI.Label(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), type.Name);
        }
        
        // Called when layer is selected
        private void OnSelectElement(ReorderableList list)
        {
            //Debug.Log("Selected element: " + list.index);
            selectedLayer = list.index;
            
            var displayedComponent = owner.GetLayer(selectedLayer);
            if (displayedComponent == null)
            {
                return;
            }

            layerEditor = CreateEditor(displayedComponent);
        }
        
        private void OnAddCallback(ReorderableList list)
        {
            layerTypes = GetSubClasses<AnimLayer>();
        
            GUIContent[] menuOptions = new GUIContent[layerTypes.Count];

            for (int i = 0; i < layerTypes.Count; i++)
            {
                menuOptions[i] = new GUIContent(layerTypes[i].Name);
            }
            
            EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.zero), menuOptions, 
                -1, OnMenuOptionSelected, null);
        }
    
        private void OnRemoveCallback(ReorderableList list)
        {
            //Debug.Log("Removed element from list");
            layerEditor = null;
            owner.RemoveLayer(list.index);
        }
        
        private void OnMenuOptionSelected(object userData, string[] options, int selected)
        {
            // This method is called when a menu option is selected
            //Debug.Log("Selected menu option: " + options[selected]);

            var selectedType = layerTypes[selected];

            if (!owner.IsLayerUnique(selectedType))
            {
                return;
            }

            // Add item class based on the selected index
            var newLayer = owner.transform.gameObject.AddComponent(selectedType);
        
            // Hide newly created item in the inspector
            newLayer.hideFlags = HideFlags.HideInInspector;
            owner.AddLayer((AnimLayer) newLayer);
            
            EditorUtility.SetDirty(target);
            Repaint();
        }
        
        private void RenderAnimButtons()
        {
            if (GUILayout.Button(new GUIContent("Setup rig", "Will find and assign bones")))
            {
                owner.SetupBones();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Preview animation"))
            {
                owner.EnableEditorPreview();
            }

            if (GUILayout.Button("Reset pose"))
            {
                owner.DisableEditorPreview();
            }

            GUILayout.EndHorizontal();
        }
        
        // Renders the inspector of currently selected Layer
        private void RenderItem()
        {
            if (layerEditor == null)
            {
                return;
            }
            
            EditorGUILayout.LabelField("Animation Layer", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);

            // Display the Inspector for a component that is a member of the component being edited
            layerEditor.OnInspectorGUI();
        
            EditorGUILayout.EndVertical();
        
            // Reset the background color
            GUI.backgroundColor = oldColor;
        }

        // Hides layers which reside in CoreAnimComponent
        private void HideRegisteredLayers()
        {
            var foundLayers = owner.gameObject.GetComponentsInChildren<AnimLayer>();
                
            foreach (var layer in foundLayers)
            {
                if (owner.HasA(layer))
                {
                    layer.hideFlags = HideFlags.HideInInspector;
                }
            }
            EditorUtility.SetDirty(target);
            Repaint();
        }

        private static List<Type> GetSubClasses<T>()
        {
            List<Type> subClasses = new List<Type>();
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(T)))
                {
                    subClasses.Add(type);
                }
            }
            return subClasses;
        }

        private void RenderLayerHelpers()
        {
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            // Get a reference to the "MiniButton" style from the skin
            var miniButtonStyle = skin.FindStyle("MiniButton");
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIContent register = new GUIContent(" Register layers", "Will find and hide layers. " 
            + "Unregistered layers will be added to the Core Component");
            
            if (GUILayout.Button(register, miniButtonStyle, GUILayout.ExpandWidth(false)))
            {
                var foundLayers = owner.gameObject.GetComponentsInChildren<AnimLayer>();
                
                foreach (var layer in foundLayers)
                {
                    if (owner.IsLayerUnique(layer.GetType()))
                    {
                        // Add new ones
                        layer.hideFlags = HideFlags.HideInInspector;
                        owner.AddLayer(layer);
                    }
                    else
                    {
                        if (!owner.HasA(layer))
                        {
                            // Destroy other "clone" components
                            DestroyImmediate(layer);
                        }
                        else
                        {
                            // Hide already exisiting
                            layer.hideFlags = HideFlags.HideInInspector; 
                        }
                    }
                }
                EditorUtility.SetDirty(target);
                Repaint();
            }
            
            GUIContent collapse = new GUIContent(" Collapse", "Will deselect the layer");
            if (GUILayout.Button(collapse, miniButtonStyle, GUILayout.ExpandWidth(false)))
            {
                layerEditor = null;
                layersReorderable.ClearSelection();
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            layersReorderable.DoLayoutList();
            
            RenderItem();
            RenderLayerHelpers();
            RenderAnimButtons();

            serializedObject.ApplyModifiedProperties();

        }
    }
}
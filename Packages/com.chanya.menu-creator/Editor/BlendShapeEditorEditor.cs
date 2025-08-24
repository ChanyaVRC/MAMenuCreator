using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BuildSoft.Unity.Tools
{
    [CustomEditor(typeof(BlendShapeEditor))]
    public class BlendShapeEditorEditor : Editor
    {
        internal static BlendShapeEditorEditor _activeEditor;

        private BlendShapeEditor _editor;
        private SerializedObject _serializedObject;
        private string _searchQuery;

        private void OnEnable()
        {
            _editor = (BlendShapeEditor)target;
            _serializedObject = new SerializedObject(_editor);
            _activeEditor = this;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            // Draw Search Bar
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Search BlendShapes:", GUILayout.Width(120));
                _searchQuery = EditorGUILayout.TextField(_searchQuery, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    _searchQuery = string.Empty;
                }
            }

            // List BlendShapes in children objects
            foreach (var renderer in _editor.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                DrawRendererBlendShapes(renderer, _searchQuery);
            }
            _serializedObject.ApplyModifiedProperties();
        }

        private void DrawRendererBlendShapes(SkinnedMeshRenderer renderer, string searchQuery)
        {
            if (renderer.sharedMesh == null) return;
            if (renderer.sharedMesh.blendShapeCount == 0) return;
            if (renderer.gameObject.CompareTag("EditorOnly")) return;
            
            IEnumerable<int> blendShapeIndices = EnumerateBlendShapeIndices(renderer, searchQuery);
            if (!blendShapeIndices.Any()) return;

            // Undo implementation
            Undo.RecordObject(renderer, "Modify BlendShape Weight");

            // Make to foldout section for each SkinnedMeshRenderer
            string foldoutKey = $"ToggleGenEditor_Foldout_{renderer.GetInstanceID()}";
            bool foldout = SessionState.GetBool(foldoutKey, false);
            using (new EditorGUILayout.HorizontalScope())
            {
                foldout = EditorGUILayout.Foldout(foldout, $"{renderer.name} ({blendShapeIndices.Count()})", true, EditorStyles.foldoutHeader);
                EditorGUILayout.ObjectField(renderer, typeof(SkinnedMeshRenderer), true);
            }
            SessionState.SetBool(foldoutKey, foldout);

            if (foldout)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var index in blendShapeIndices)
                    {
                        DrawBlendShape(renderer, index);
                    }
                }
            }
        }

        private void DrawBlendShape(SkinnedMeshRenderer renderer, int blendShapeIndex)
        {
            var blendShapeName = renderer.sharedMesh.GetBlendShapeName(blendShapeIndex);
            var blendShapeWeight = renderer.GetBlendShapeWeight(blendShapeIndex);
            using (var horizontalScope = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(blendShapeName, GUILayout.MinWidth(120));
                var newBlendShapeWeight = EditorGUILayout.Slider(blendShapeWeight, 0f, 100f);
                if (newBlendShapeWeight != blendShapeWeight)
                {
                    Undo.RecordObject(renderer, "Modify BlendShape Weight");
                    renderer.SetBlendShapeWeight(blendShapeIndex, newBlendShapeWeight);
                }

                // Button to create a shape changer menu
                if (GUILayout.Button("MA", GUILayout.Width(30)))
                {
                    CreateMenuForToggle(_editor, renderer, blendShapeIndex);
                }

                // Right-click context menu
                var e = Event.current;
                if (e.type == EventType.ContextClick)
                {
                    if (horizontalScope.rect.Contains(e.mousePosition))
                    {
                        ContextMenu(_editor, renderer, blendShapeIndex);
                        e.Use();
                    }
                }
            }
        }

        private static void ContextMenu(BlendShapeEditor editor, SkinnedMeshRenderer renderer, int blendShapeIndex)
        {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Create menu for Toggle (→)"), false, () => CreateMenuForToggle(editor, renderer, blendShapeIndex));
            menu.AddItem(new GUIContent("Create menu for Toggle (←)"), false, () => CreateMenuForToggle(editor, renderer, blendShapeIndex, false));
            menu.AddItem(new GUIContent("Create menu for Radial Puppet (→)"), false, () => CreateMenuForRadialPuppet(editor, renderer, blendShapeIndex));
            menu.AddItem(new GUIContent("Create menu for Radial Puppet (←)"), false, () => CreateMenuForRadialPuppet(editor, renderer, blendShapeIndex, false));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Create AnimationClip (Weight: 0)"), false, () => CreateAndPingAnimationClipAsset(renderer, blendShapeIndex, 0f));
            menu.AddItem(new GUIContent("Create AnimationClip (Weight: 100)"), false, () => CreateAndPingAnimationClipAsset(renderer, blendShapeIndex, 100f));
            menu.AddItem(new GUIContent("Create AnimationClip (Weight: Current Value)"), false,
                () => CreateAndPingAnimationClipAsset(renderer, blendShapeIndex, renderer.GetBlendShapeWeight(blendShapeIndex)));
            menu.ShowAsContext();
        }

        private static void CreateMenuForRadialPuppet(BlendShapeEditor editor, SkinnedMeshRenderer renderer, int blendShapeIndex, bool is0To100 = true)
        {
            GameObject menuObject = editor.gameObject.GetMenuGameObject();
            menuObject.CreateShapeChangerMenu(renderer, blendShapeIndex, MAMenuCreator.MenuControlType.RadialPuppet, is0To100);
        }

        private static void CreateMenuForToggle(BlendShapeEditor editor, SkinnedMeshRenderer renderer, int blendShapeIndex, bool is0To100 = true)
        {
            GameObject menuObject = editor.gameObject.GetMenuGameObject();
            menuObject.CreateShapeChangerMenu(renderer, blendShapeIndex, MAMenuCreator.MenuControlType.Toggle, is0To100);
        }

        private static void CreateAndPingAnimationClipAsset(SkinnedMeshRenderer renderer, int blendShapeIndex, float weight)
        {
            var clip = AssetUtility.GetOrCreateAnimationClipAsset(renderer, blendShapeIndex, weight);
            EditorGUIUtility.PingObject(clip);
        }

        private static IEnumerable<int> EnumerateBlendShapeIndices(SkinnedMeshRenderer renderer, string searchQuery)
        {
            int blendShapeCount = renderer.sharedMesh.blendShapeCount;
            if (string.IsNullOrEmpty(searchQuery))
            {
                // Use array for slightly faster allocation and enumeration
                return Enumerable.Range(0, blendShapeCount);
            }

            // Avoid repeated property access and use pooled list
            List<int> matchingIndices = new(blendShapeCount);
            var mesh = renderer.sharedMesh;
            for (int i = 0; i < blendShapeCount; i++)
            {
                var blendShapeName = mesh.GetBlendShapeName(i);
                if (blendShapeName.IndexOf(searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matchingIndices.Add(i);
                }
            }
            return matchingIndices;
        }
    }
}
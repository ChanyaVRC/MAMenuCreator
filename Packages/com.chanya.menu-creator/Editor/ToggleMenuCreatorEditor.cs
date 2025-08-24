using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.runtime;
using nadena.dev.ndmf.util;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BuildSoft.Unity.Tools
{
    [CustomEditor(typeof(ToggleMenuCreator))]
    public class ToggleMenuCreatorEditor : Editor
    {
        private GameObject _avatar;
        private ToggleMenuCreator _target;
        private SerializedObject _avatarSerializedObject;
        private SerializedObject _serializedObject;

        private void OnEnable()
        {
            _target = (ToggleMenuCreator)target;
            _serializedObject = new SerializedObject(target);
            _avatar = RuntimeUtil.FindAvatarInParents(_target.transform)?.gameObject;
            _avatarSerializedObject = _avatar == null ? null : new SerializedObject(_avatar);
        }

        public override void OnInspectorGUI()
        {
            // List all Renderers in children
            foreach (var item in _target.GetComponentsInChildren<Renderer>(true).Select(v => v.gameObject).Distinct().Where(v => !v.CompareTag("EditorOnly")))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!item.activeInHierarchy))
                    {
                        EditorGUILayout.LabelField(EditorUtility.TempContent(item.name, item.AvatarRootPath()), GUILayout.Width(EditorGUIUtility.labelWidth - EditorUtility.indent));
                    }
                    EditorGUILayout.ObjectField(item, typeof(GameObject), true);
                    if (GUILayout.Button("MA", GUILayout.Width(30)))
                    {
                        var menu = _avatar.GetMenuGameObject();
                        CreateObjectToggleMenu(item, menu);
                    }
                }
            }
        }

        private static void CreateObjectToggleMenu(GameObject menu, GameObject item)
        {
            var menuItem = menu.CreateMAMenu(item.name, item.activeSelf, MAMenuCreator.MenuControlType.Toggle);
            var objectToggle = menuItem.gameObject.AddComponent<ModularAvatarObjectToggle>();
            objectToggle.Inverted = item.activeSelf;
            objectToggle.Objects = new() {
                new() {
                    Active = !item.activeSelf,
                    Object = new() { referencePath = item.AvatarRootPath() }
                }
            };
        }
    }
}
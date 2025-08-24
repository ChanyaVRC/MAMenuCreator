using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace BuildSoft.Unity.Tools
{
    internal static class MAMenuCreator
    {
        internal enum MenuControlType
        {
            Button = VRCExpressionsMenu.Control.ControlType.Button,
            Toggle = VRCExpressionsMenu.Control.ControlType.Toggle,
            SubMenu = VRCExpressionsMenu.Control.ControlType.SubMenu,
            TwoAxisPuppet = VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet,
            FourAxisPuppet = VRCExpressionsMenu.Control.ControlType.FourAxisPuppet,
            RadialPuppet = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
        }

        public static ModularAvatarMenuItem CreateShapeChangerMenu(this GameObject menu, SkinnedMeshRenderer renderer, int blendShapeIndex, MenuControlType controlType, bool is0To100 = true)
        {
            switch (controlType)
            {
                case MenuControlType.Button:
                case MenuControlType.Toggle:
                    return menu.CreateToggleOrButtonMenu(renderer, blendShapeIndex, controlType, is0To100);

                case MenuControlType.RadialPuppet:
                    return menu.CreateRadialPuppetMenu(renderer, blendShapeIndex, is0To100);

                case MenuControlType.SubMenu:
                case MenuControlType.TwoAxisPuppet:
                case MenuControlType.FourAxisPuppet:
                    Debug.LogError($"Control type {controlType} is not supported for blend shapes. Use Toggle instead.");
                    throw new System.NotImplementedException();

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(controlType), controlType, null);
            }
        }

        public static GameObject GetMenuGameObject(this GameObject avatarRoot)
        {
            Transform menuTransform = avatarRoot.transform.Find("Menu");
            if (menuTransform != null)
            {
                return menuTransform.gameObject;
            }

            GameObject menu = new("Menu");
            menu.transform.SetParent(avatarRoot.transform, false);
            menu.AddComponent<ModularAvatarMenuInstaller>();
            menu.AddComponent<ModularAvatarMenuGroup>();
            Undo.RegisterCreatedObjectUndo(menu, "Create MA Menu");
            return menu;
        }

        private static ModularAvatarMenuItem CreateMAMenu(this GameObject menu, ChangedShape changedShape, bool isDefault)
        {
            return CreateMAMenu(menu, changedShape.ShapeName, new[] { changedShape }, isDefault);
        }

        private static ModularAvatarMenuItem CreateMAMenu(this GameObject menu, string name, ChangedShape changedShape, bool isDefault)
        {
            return CreateMAMenu(menu, name, new[] { changedShape }, isDefault);
        }

        private static ModularAvatarMenuItem CreateMAMenu(this GameObject menu, string name, IEnumerable<ChangedShape> changedShape, bool isDefault)
        {
            var menuItem = CreateMAMenu(menu, name, isDefault, MenuControlType.Toggle);

            var shapeChanger = menuItem.gameObject.AddComponent<ModularAvatarShapeChanger>();
            shapeChanger.Shapes.AddRange(changedShape);
            shapeChanger.Inverted = isDefault;

            return menuItem;
        }

        public static ModularAvatarMenuItem CreateMAMenu(this GameObject menu, string name, bool isDefault, MenuControlType controlType)
        {
            var newItem = new GameObject(name);
            newItem.transform.SetParent(menu.transform, false);
            Undo.RegisterCreatedObjectUndo(newItem, "Create MA Menu");

            var menuItem = newItem.AddComponent<ModularAvatarMenuItem>();
            menuItem.name = name;
            menuItem.automaticValue = true;
            menuItem.Control = new VRCExpressionsMenu.Control
            {
                type = (VRCExpressionsMenu.Control.ControlType)controlType
            };
            menuItem.isDefault = isDefault;

            return menuItem;
        }

        private static ModularAvatarMenuItem CreateToggleOrButtonMenu(this GameObject menu, SkinnedMeshRenderer renderer, int blendShapeIndex, MenuControlType controlType, bool is0To100)
        {
            Debug.Assert(controlType == MenuControlType.Button || controlType == MenuControlType.Toggle, "controlType must be Button or Toggle");

            float blendShapeWeight = renderer.GetBlendShapeWeight(blendShapeIndex);
            ChangedShape changedShape = new()
            {
                ChangeType = ShapeChangeType.Set,
                Object = new() { referencePath = renderer.AvatarRootPath() },
                ShapeName = renderer.sharedMesh.GetBlendShapeName(blendShapeIndex),
                Value = blendShapeWeight == 0f ? 100f : 0f,
            };

            ModularAvatarMenuItem modularAvatarMenuItem = CreateMAMenu(menu, changedShape, (blendShapeWeight != 0f) ^ (!is0To100));
            modularAvatarMenuItem.Control.type = (VRCExpressionsMenu.Control.ControlType)controlType;
            return modularAvatarMenuItem;
        }

        private static ModularAvatarMenuItem CreateRadialPuppetMenu(this GameObject menu, SkinnedMeshRenderer renderer, int blendShapeIndex, bool is0To100)
        {
            string blendShapeName = renderer.sharedMesh.GetBlendShapeName(blendShapeIndex);
            float blendShapeWeight = renderer.GetBlendShapeWeight(blendShapeIndex);
            var menuItem = menu.CreateMAMenu(blendShapeName, false, MenuControlType.RadialPuppet);

            // Create BlendTree asset
            string suffix = is0To100 ? "" : "_100to0";
            string path = AssetUtility.GetGeneratedFolderPath(AssetUtility.GetUniqueFileName(renderer, blendShapeName, suffix + ".asset"));

            BlendTreeElements elements = AssetUtility.GetOrCreateBlendTreeElementsAsset(renderer, is0To100, blendShapeName, path);

            var maMergeBlendTree = menuItem.gameObject.AddComponent<ModularAvatarMergeBlendTree>();
            maMergeBlendTree.Motion = elements.BlendTree;
            maMergeBlendTree.PathMode = MergeAnimatorPathMode.Absolute;

            // TODO: Check the parent MAParameters component
            var maParameters = menuItem.gameObject.GetOrAddComponent<ModularAvatarParameters>();
            List<ParameterConfig> parameters = maParameters.parameters;
            if (!parameters.Any(v => v.nameOrPrefix == elements.BlendTree.blendParameter))
            {
                float defaultWeight = GetValueForFloatParameter(blendShapeWeight, is0To100);
                parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = elements.BlendTree.blendParameter,
                    syncType = ParameterSyncType.Float,
                    saved = true,
                    defaultValue = defaultWeight,
                    hasExplicitDefaultValue = true,
                });
            }
            menuItem.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter() { name = elements.BlendTree.blendParameter } };
            return menuItem;
        }

        private static float GetValueForFloatParameter(float blendShapeWeight, bool is0To100)
        {
            if (is0To100)
            {
                return blendShapeWeight / 100f;
            }
            return 1f - (blendShapeWeight / 100f);
        }
    }
}
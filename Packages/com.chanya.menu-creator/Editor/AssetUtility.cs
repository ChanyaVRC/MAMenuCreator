using nadena.dev.ndmf.runtime;
using nadena.dev.ndmf.util;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BuildSoft.Unity.Tools
{
    internal static class AssetUtility
    {
        public const string GeneratedFileFolder = "Generated";

        public static void CreateAndImportAssets(BlendTreeElements elements, string pathName)
        {
            AssetDatabase.CreateAsset(elements, pathName);
            AssetDatabase.AddObjectToAsset(elements.BlendTree, pathName);
            AssetDatabase.AddObjectToAsset(elements.AnimationClip_0, pathName);
            AssetDatabase.AddObjectToAsset(elements.AnimationClip_100, pathName);
            AssetDatabase.ImportAsset(pathName);
        }

        public static string GetUniqueFileName(SkinnedMeshRenderer renderer, string blendShapeName, string extension)
            => $"{RuntimeUtil.FindAvatarInParents(renderer.transform)?.name}##{renderer.AvatarRootPath().Replace('/', '#')}##{blendShapeName}{extension}";

        public static string GetFileName(SkinnedMeshRenderer renderer, string blendShapeName, string extension)
            => $"{renderer.AvatarRootPath().Replace('/', '#')}##{blendShapeName}{extension}";

        public static string GetGeneratedFolderPath(string assetName)
        {
            var path = "Assets/MenuCreator";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "MenuCreator");
            }

            // if does not exist, create the folder
            string folderPath = path + "/" + GeneratedFileFolder;
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(path, GeneratedFileFolder);
            }
            return string.Concat(folderPath, "/", assetName);
        }

        public static AnimationClip GetOrCreateAnimationClipAsset(SkinnedMeshRenderer renderer, int blendShapeIndex, float weight, string path = null)
        {
            string blendShapeName = renderer.sharedMesh.GetBlendShapeName(blendShapeIndex);
            path ??= GetGeneratedFolderPath(GetFileName(renderer, blendShapeName, $"_{Mathf.RoundToInt(weight)}.anim"));
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) is AnimationClip existing)
            {
                return existing;
            }

            return CreateAnimationClipAsset(renderer, blendShapeName, weight, path);
        }

        private static AnimationClip CreateAnimationClipAsset(SkinnedMeshRenderer renderer, string blendShapeName, float weight, string path)
        {
            var clip = CreateAnimationClip(renderer, blendShapeName, weight);
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.ImportAsset(path);
            return clip;
        }

        // TODO: Move to runtime utility class
        public static AnimationClip CreateAnimationClip(SkinnedMeshRenderer renderer, string blendShapeName, float weight)
        {
            var clip = new AnimationClip
            {
                name = $"{blendShapeName}_AnimationClip_{weight}"
            };
            var curve = new AnimationCurve();
            curve.AddKey(0, weight);

            var binding = EditorCurveBinding.FloatCurve(renderer.AvatarRootPath(), typeof(SkinnedMeshRenderer), "blendShape." + blendShapeName);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            return clip;
        }

        public static BlendTreeElements GetOrCreateBlendTreeElementsAsset(SkinnedMeshRenderer renderer, bool is0To100, string blendShapeName, string path = null)
        {
            path ??= GetGeneratedFolderPath(GetUniqueFileName(renderer, blendShapeName, (is0To100 ? "" : "_100to0") + ".asset"));
            if (AssetDatabase.LoadAssetAtPath<BlendTreeElements>(path) is BlendTreeElements existing)
            {
                return existing;
            }
            return CreateBlendTreeElementsAsset(renderer, is0To100, blendShapeName, path);
        }

        public static BlendTreeElements CreateBlendTreeElementsAsset(SkinnedMeshRenderer renderer, bool is0To100, string blendShapeName, string path)
        {
            BlendTreeElements elements = CreateBlendTreeElements(renderer, is0To100, blendShapeName);
            CreateAndImportAssets(elements, path);
            return elements;
        }

        // TODO: Move to runtime utility class
        private static BlendTreeElements CreateBlendTreeElements(SkinnedMeshRenderer renderer, bool is0To100, string blendShapeName)
        {
            BlendTreeElements elements = new()
            {
                AnimationClip_0 = CreateAnimationClip(renderer, blendShapeName, 0f),
                AnimationClip_100 = CreateAnimationClip(renderer, blendShapeName, 100f),
            };

            var childMotions = new ChildMotion[]
            {
                new() { motion = elements.AnimationClip_0 },
                new() { motion = elements.AnimationClip_100 }
            };

            if (!is0To100)
            {
                (childMotions[0], childMotions[1]) = (childMotions[1], childMotions[0]);
            }

            elements.BlendTree = new BlendTree
            {
                name = $"{blendShapeName}_RadialPuppet",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "/BSFT/BlendShapeEditor/" + blendShapeName,
                children = childMotions,
            };
            return elements;
        }
    }
}
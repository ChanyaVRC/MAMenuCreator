using UnityEditor;
using UnityEngine;

namespace BuildSoft.Unity.Tools
{
    internal static class EditorUtility
    {
        internal static float indent => EditorGUI.indentLevel * 15f;

        private static readonly GUIContent s_Text = new();

        private static readonly GUIContent s_Image = new();

        private static readonly GUIContent s_TextImage = new();

        internal static GUIContent TempContent(string t)
        {
            s_Text.image = null;
            s_Text.text = t;
            s_Text.tooltip = null;
            return s_Text;
        }

        internal static GUIContent TempContent(string text, string tip)
        {
            s_Text.image = null;
            s_Text.text = text;
            s_Text.tooltip = tip;
            return s_Text;
        }

        internal static GUIContent TempContent(Texture i)
        {
            s_Image.image = i;
            s_Image.text = null;
            s_Image.tooltip = null;
            return s_Image;
        }

        internal static GUIContent TempContent(string t, Texture i)
        {
            s_TextImage.image = i;
            s_TextImage.text = t;
            s_TextImage.tooltip = null;
            return s_TextImage;
        }

        internal static GUIContent[] TempContent(string[] texts)
        {
            GUIContent[] array = new GUIContent[texts.Length];
            for (int i = 0; i < texts.Length; i++)
            {
                array[i] = new GUIContent(texts[i]);
            }

            return array;
        }

        internal static GUIContent[] TempContent(string[] texts, string[] tooltips)
        {
            GUIContent[] array = new GUIContent[texts.Length];
            for (int i = 0; i < texts.Length; i++)
            {
                array[i] = new GUIContent(texts[i], tooltips[i]);
            }

            return array;
        }

    }
}
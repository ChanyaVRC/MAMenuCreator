using UnityEngine;

namespace BuildSoft.Unity.Tools
{
    internal static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
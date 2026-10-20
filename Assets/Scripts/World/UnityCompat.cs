using UnityEngine;

/// <summary>
/// 兼容 Unity 2022 与 2023+ 的查找接口，避免新版本的过时警告。
/// </summary>
public static class UnityCompat
{
    public static T FindOne<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }

    public static T[] FindAll<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<T>();
#endif
    }
}

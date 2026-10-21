using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// JSON 存档：优先 Application.persistentDataPath，并镜像一份 PlayerPrefs。
/// </summary>
public static class SaveSystem
{
    const string FileName = "escape_exam_save.json";
    const string PrefsKey = "EscapeExam.SaveJson";

    public static string FilePath
    {
        get { return Path.Combine(Application.persistentDataPath, FileName); }
    }

    public static bool HasSave()
    {
        if (File.Exists(FilePath))
            return true;
        return !string.IsNullOrEmpty(PlayerPrefs.GetString(PrefsKey, ""));
    }

    public static bool Save()
    {
        GameProgress progress = GameProgress.Ensure();
        SaveData data = progress.ToSaveData();
        string json = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(FilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("写入存档文件失败，改用 PlayerPrefs：" + e.Message);
        }
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
        return true;
    }

    public static bool LoadAndApply(bool loadScene = true)
    {
        SaveData data = ReadSave();
        if (data == null)
            return false;

        GameProgress progress = GameProgress.Ensure();
        progress.ApplyFromSave(data);

        if (loadScene && !string.IsNullOrEmpty(data.sceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(data.sceneName))
                SceneManager.LoadScene(data.sceneName);
            else
                Debug.LogWarning("存档场景未在 Build Settings：" + data.sceneName);
        }
        return true;
    }

    public static SaveData ReadSave()
    {
        string json = null;
        if (File.Exists(FilePath))
        {
            try
            {
                json = File.ReadAllText(FilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("读取存档文件失败：" + e.Message);
            }
        }
        if (string.IsNullOrEmpty(json))
            json = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("解析存档失败：" + e.Message);
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(FilePath))
        {
            try { File.Delete(FilePath); }
            catch { }
        }
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }
}

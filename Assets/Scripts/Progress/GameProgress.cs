using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 跨场景进度：阶段、资金、手机、留条、地图解锁等。DontDestroyOnLoad。
/// </summary>
public class GameProgress : MonoBehaviour
{
    public const string StageExam = "Exam";
    public const string StageSchool = "School";
    public const string StageHub = "Hub";
    public const string StageHome = "Home";
    public const string StageTransit = "Transit";
    public const string StageTravel = "Travel";

    public static GameProgress Instance { get; private set; }

    public CampusExamState examChapter = new CampusExamState();
    public CampusCorridorState corridorChapter = new CampusCorridorState();
    public event System.Action SaveApplied;
    public string stage = StageExam;
    public int money;
    public bool hasPhone;
    public bool leftNote;
    public bool mapUnlocked;
    public bool escapedSchool;
    public string lastSceneName = "";
    public Vector3 checkpoint = Vector3.zero;
    public bool hasCheckpoint;

    /// <summary>手机商店标价（旅游资金）。</summary>
    public const int PhonePrice = 50;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastSceneName = scene.name;
    }

    public static GameProgress Ensure()
    {
        if (Instance != null)
            return Instance;
        GameObject go = new GameObject("GameProgress");
        return go.AddComponent<GameProgress>();
    }

    public void MarkEscapedSchool()
    {
        escapedSchool = true;
        mapUnlocked = true;
        stage = StageHub;
    }

    public void MarkLeftNote()
    {
        leftNote = true;
    }

    public void GrantPhone()
    {
        hasPhone = true;
    }

    public void AddMoney(int amount)
    {
        if (amount == 0)
            return;
        money = Mathf.Max(0, money + amount);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0)
            return true;
        if (money < amount)
            return false;
        money -= amount;
        return true;
    }

    public void SetCheckpoint(Vector3 worldPos)
    {
        checkpoint = worldPos;
        hasCheckpoint = true;
    }

    public bool CanBuyTickets()
    {
        return leftNote && hasPhone;
    }

    public void ApplyFromSave(SaveData data)
    {
        if (data == null)
            return;
        examChapter = data.examChapter ?? new CampusExamState();
        corridorChapter = data.corridorChapter ?? new CampusCorridorState();
        stage = data.stage ?? StageExam;
        money = data.money;
        hasPhone = data.hasPhone;
        leftNote = data.leftNote;
        mapUnlocked = data.mapUnlocked;
        escapedSchool = data.escapedSchool;
        lastSceneName = data.sceneName ?? "";
        checkpoint = new Vector3(data.checkpointX, data.checkpointY, data.checkpointZ);
        hasCheckpoint = data.hasCheckpoint;
        if (data.hasTicket)
            TicketInventory.Grant(data.ticketName);
        else
            TicketInventory.Clear();
        SaveApplied?.Invoke();
    }

    public SaveData ToSaveData()
    {
        string scene = SceneManager.GetActiveScene().name;
        return new SaveData
        {
            examChapter = examChapter,
            corridorChapter = corridorChapter,
            stage = stage,
            money = money,
            hasPhone = hasPhone,
            leftNote = leftNote,
            mapUnlocked = mapUnlocked,
            escapedSchool = escapedSchool,
            sceneName = scene,
            checkpointX = checkpoint.x,
            checkpointY = checkpoint.y,
            checkpointZ = checkpoint.z,
            hasCheckpoint = hasCheckpoint,
            hasTicket = TicketInventory.HasTicket,
            ticketName = TicketInventory.TicketName
        };
    }
}

[System.Serializable]
public class SaveData
{
    public CampusExamState examChapter;
    public CampusCorridorState corridorChapter;
    public string stage;
    public int money;
    public bool hasPhone;
    public bool leftNote;
    public bool mapUnlocked;
    public bool escapedSchool;
    public string sceneName;
    public float checkpointX;
    public float checkpointY;
    public float checkpointZ;
    public bool hasCheckpoint;
    public bool hasTicket;
    public string ticketName;
}

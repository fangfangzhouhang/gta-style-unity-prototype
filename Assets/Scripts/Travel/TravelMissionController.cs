using UnityEngine;

/// <summary>
/// 跟踪至少 3 个旅行目标；全部完成后显示中文通关反馈。
/// </summary>
public class TravelMissionController : MonoBehaviour
{
    public TravelGameHUD hud;
    public TravelMissionPoint[] points;

    public int CompletedCount { get; private set; }
    public int TotalCount { get; private set; }
    public bool AllComplete { get; private set; }

    string lastCompletedTitle = "";
    float feedbackUntil;

    void Start()
    {
        if (hud == null)
            hud = UnityCompat.FindOne<TravelGameHUD>();
        if (points == null || points.Length == 0)
            points = UnityCompat.FindAll<TravelMissionPoint>();

        TotalCount = points != null ? points.Length : 0;
        Recount();
        RefreshHud();
    }

    public void NotifyMissionComplete(TravelMissionPoint point)
    {
        if (point == null || AllComplete)
            return;

        lastCompletedTitle = point.title;
        feedbackUntil = Time.time + 2.4f;
        Recount();

        if (hud != null)
        {
            hud.showFeedbackBanner = true;
            hud.feedbackTitle = "完成：" + lastCompletedTitle;
            hud.feedbackSubtitle = CompletedCount + " / " + TotalCount + " 处地标已打卡";
        }

        RefreshHud();

        if (CompletedCount >= TotalCount && TotalCount > 0)
            CompleteAll();
    }

    void Update()
    {
        if (feedbackUntil > 0f && Time.time >= feedbackUntil && !AllComplete)
        {
            feedbackUntil = 0f;
            if (hud != null)
                hud.showFeedbackBanner = false;
        }
    }

    void Recount()
    {
        CompletedCount = 0;
        if (points == null)
            return;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].IsComplete)
                CompletedCount++;
        }
    }

    void CompleteAll()
    {
        AllComplete = true;
        if (hud != null)
        {
            hud.showFeedbackBanner = true;
            hud.feedbackTitle = "旅行任务完成";
            hud.feedbackSubtitle = "v0.3 切片通关 · 三处地标已打卡";
            hud.objective = "旅行任务完成";
            hud.detail = "西湖 · 长城 · 黄山";
            hud.bottomPrompt = "可退出 Play；后续里程碑再扩展更多城市";
        }
    }

    void RefreshHud()
    {
        if (hud == null || AllComplete)
            return;

        hud.objective = "打卡中国地标（" + CompletedCount + "/" + TotalCount + "）";
        hud.detail = NextHint();
        hud.bottomPrompt = "WASD 移动    走进黄色光圈即可完成";
    }

    string NextHint()
    {
        if (points == null)
            return "寻找黄色光圈";
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && !points[i].IsComplete)
                return "下一站：" + points[i].title;
        }
        return "全部完成";
    }
}

using UnityEngine;

/// <summary>
/// 原型票务状态。场景切换间用静态字段保留「是否已购票 / 车次名」。
/// </summary>
public static class TicketInventory
{
    public static bool HasTicket { get; private set; }
    public static string TicketName { get; private set; } = "";

    public static void Grant(string trainName)
    {
        HasTicket = true;
        TicketName = string.IsNullOrEmpty(trainName) ? "车票" : trainName;
    }

    public static void Clear()
    {
        HasTicket = false;
        TicketName = "";
    }

    /// <summary>检票成功后清除，避免旅行区误带状态。</summary>
    public static void Consume()
    {
        HasTicket = false;
        // 保留 TicketName 仅作展示；下次购票会覆盖
    }
}

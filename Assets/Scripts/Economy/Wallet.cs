using UnityEngine;

/// <summary>
/// 旅游资金读写封装，底层落到 GameProgress.money。
/// </summary>
public static class Wallet
{
    public static int Balance
    {
        get
        {
            GameProgress gp = GameProgress.Ensure();
            return gp != null ? gp.money : 0;
        }
    }

    public static void Add(int amount)
    {
        GameProgress.Ensure().AddMoney(amount);
    }

    public static bool TrySpend(int amount)
    {
        return GameProgress.Ensure().TrySpend(amount);
    }
}

/// <summary>
/// 是否持有手机。无手机时窗口只提示需要手机才能查看票价。
/// </summary>
public static class PhoneInventory
{
    public static bool HasPhone
    {
        get { return GameProgress.Ensure().hasPhone; }
    }

    public static void Grant()
    {
        GameProgress.Ensure().GrantPhone();
    }
}

using UnityEngine;

/// <summary>
/// 商店 / 路边摊买手机交互（靠近按 E）。
/// </summary>
public class BuyPhoneInteract : MonoBehaviour
{
    public string promptNear = "按 E 购买手机（50 旅游资金）";
    public float interactRadius = 2.6f;
    public int price = GameProgress.PhonePrice;

    Transform player;
    public bool PlayerInRange { get; private set; }
    public string LastTip { get; private set; }

    void Start()
    {
        ResolvePlayer();
    }

    void ResolvePlayer()
    {
        ThirdPersonController motor = UnityCompat.FindOne<ThirdPersonController>();
        if (motor != null)
            player = motor.transform;
    }

    void Update()
    {
        if(CampusJournal.Opened)return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsOpen)
            return;

        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
                return;
        }

        float dist = Vector3.Distance(
            new Vector3(player.position.x, 0f, player.position.z),
            new Vector3(transform.position.x, 0f, transform.position.z));
        PlayerInRange = dist <= interactRadius;
        if (!PlayerInRange)
            return;

        if (PhoneInventory.HasPhone)
        {
            LastTip = "已有手机";
            return;
        }

        LastTip = promptNear;
        if (Input.GetKeyDown(KeyCode.E))
            TryBuy();
    }

    void TryBuy()
    {
        if (PhoneInventory.HasPhone)
            return;
        if (!Wallet.TrySpend(price))
        {
            LastTip = "资金不足（需要 " + price + "）";
            HubTownFlowController hub = UnityCompat.FindOne<HubTownFlowController>();
            if (hub != null)
                hub.ShowTip("资金不足", "买手机需要 " + price + " 旅游资金，先做任务赚钱");
            return;
        }
        PhoneInventory.Grant();
        LastTip = "已购买手机";
        HubTownFlowController hubOk = UnityCompat.FindOne<HubTownFlowController>();
        if (hubOk != null)
            hubOk.ShowTip("购买成功", "已获得手机，可在枢纽查看票价");
        SaveSystem.Save();
    }
}

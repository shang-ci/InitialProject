using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 需要物品的事件类
/// </summary>
public class Event_Item : EventBase
{
    [Header("事件属性")]
    [SerializeField] protected List<string> RequiredItems; // 需要的物品列表
    [SerializeField] private TMP_Dropdown itemDropdown;// 下拉框
    [SerializeField] private TMP_InputField countInput;// 数量输入框
    [SerializeField] private Button consumeButton;// 消费按钮——测试用的
    [SerializeField] private TMP_Text errorText; // 用于显示提示信息
    [SerializeField] private bool isConsumable;// 物品是否够消费

    private Inventory inventory;
    private int selectedIndex = -1;
    private int consumeAmount = 0;
    private ItemInfo? selectedItemInfo;// 当前选中的物品信息

    protected override void Awake()
    {
        base.Awake();

        inventory = Character.Instance.inventory;
    }

    protected override void Start()
    {
        base.Start();

        PopulateDropdown();
        itemDropdown.onValueChanged.AddListener(OnDropdownChanged);
        countInput.onValueChanged.AddListener(OnCountChanged);
    }

    private void OnDestroy()
    {
        countInput.onValueChanged.RemoveListener(OnCountChanged);
        itemDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    public override void Initialize(EventData eventData)
    {
        base.Initialize(eventData);

        RequiredItems = eventData.RequiredItems;
    }

    protected override void ExecutionEvent(EventData eventData)
    {
        isEventActive = true;

        //属性和文本都过关
        if (RollTheDice_CharacterStat(eventData, successProbability) && selectedItemInfo.HasValue && Tip(selectedItemInfo.Value))
        {
            //消耗
            OnConsume();

            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true;
            GameManager.Instance.RegisterChoice(eventData.SuccessEvent); // 注册成功事件
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        //属性过关但物品不满足要求
        else if (RollTheDice_CharacterStat(eventData, successProbability))
        {
            // 成功但没有满足物品要求
            Result_Story.text = eventData.FailedResults + "骰子成功，但没有满足所有物品要求。";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册成功事件
            isSuccess_Event = false;
        }
        //物品满足但属性不满足
        else if (selectedItemInfo.HasValue && Tip(selectedItemInfo.Value))
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "装备满足，但骰子不满足要求。";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            isSuccess_Event = false;
        }
        //都不满足
        else
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "骰子和装备都不满足";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            isSuccess_Event = false;
        }

        // 展开Three面板
        ExpandThree();

        //消耗掉所有卡槽中的卡牌
        foreach (var cardSlot in CardSlots)
        {
            var card = cardSlot.GetComponentInChildren<Card>();
            if (card != null)
            {
                Inventory_ZXH.Instance.Backpack.RemoveCard(card.cardData);
            }

        }
    }

    #region 装备下拉框
    /// <summary>
    /// 填充下拉列表
    /// </summary>
    private void PopulateDropdown()
    {
        var slice = inventory.AllItemInfos; // 获取背包所有 ItemInfo
        var options = new List<string>();
        foreach (var itemInfo in slice)
        {
            options.Add($"{itemInfo.Item.name} x{itemInfo.Amount}");
        }
        itemDropdown.ClearOptions();
        itemDropdown.AddOptions(options);
        selectedIndex = itemDropdown.options.Count > 0 ? 0 : -1;
    }

    private void OnDropdownChanged(int index)
    {
        selectedIndex = index;
        UpdateConsumableState();
    }

    private void OnCountChanged(string text)
    {
        consumeAmount = int.TryParse(text, out var v) ? v : 0;
        UpdateConsumableState();
    }

    /// <summary>
    /// 更新是否可消耗状态并更新当前物品
    /// </summary>
    private void UpdateConsumableState()
    {
        errorText.gameObject.SetActive(false);
        isConsumable = false;
        selectedItemInfo = null;

        if (selectedIndex < 0) return;

        var slice = inventory.AllItemInfos;
        if (selectedIndex >= slice.Count)
        {
            ShowError("下拉索引越界");
            return;
        }

        var info = slice[selectedIndex];
        selectedItemInfo = info;

        //提示是否满足事件
        bool flowControl = Tip(info);
        if (!flowControl)
        {
            return;
        }

        if (consumeAmount <= 0)
        {
            ShowError("请输入数量 (>0)");
            return;
        }

        if (consumeAmount > info.Amount)
        {
            ShowError($"库存不足，仅有 {info.Amount}");
            return;
        }

        // 当前选择合法且数量足够可消费
        isConsumable = true;
    }

    /// <summary>
    /// 提示是否满足事件
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private bool Tip(ItemInfo info)
    {
        //提示是否满足事件需求
        var def = info.Item.ItemDefinition;
        var defName = def.name;
        if (RequiredItems != null && RequiredItems.Count > 0)
        {
            // 只允许消耗列入 RequiredItems 的物品
            if (!RequiredItems.Contains(defName))
            {
                ShowError($"请选择正确物品（需：{RequiredItems[0]}）");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 点击消耗按钮
    /// </summary>
    private void OnConsume()
    {
        UpdateConsumableState();
        if (!isConsumable || selectedItemInfo == null)
        {
            ShowError("当前无法消耗");
            return;
        }

        var info = selectedItemInfo.Value;
        var def = info.Item.ItemDefinition;
        inventory.RemoveItem(def, consumeAmount);
        Debug.Log($"消耗 {info.Item.name} x{consumeAmount}");
        PopulateDropdown();

        // 隐藏提示
        errorText.gameObject.SetActive(false);
    }


    /// <summary>
    /// 报错提醒
    /// </summary>
    /// <param name="msg"></param>
    private void ShowError(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
            errorText.gameObject.SetActive(true);
            // 三秒后自动隐藏
            StopAllCoroutines();
            StartCoroutine(ClearErrorAfterSeconds(2f));
        }
    }

    private IEnumerator ClearErrorAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Event_Role : EventBase
{
    [Header("事件属性")]
    [SerializeField] protected Role RequiredRole; // 需要的角色
    [SerializeField] private TMP_Dropdown roleDropdown;// 角色下拉框
    [SerializeField] private Button confirmButton;// 确认按钮——可有可无
    [SerializeField] private TMP_Text feedbackText; // 错误或提示信息

    private Role? selectedRole;// 当前选中的角色
    private bool isRoleMatch = false;// 是否匹配角色

    protected override void Start()
    {
        base.Start();

        PopulateDropdown();
        roleDropdown.onValueChanged.AddListener(OnDropdownChanged);
        feedbackText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        roleDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    protected override void ExecutionEvent(EventData eventData)
    {
        if (!isBock) return; // 如果没有锁定事件,表示玩家还没有确认选择就算到期也不会执行事件逻辑

        isEventActive = true;

        //属性和文本都过关
        if (RollTheDice_CharacterStat(eventData, successProbability) && isRoleMatch)
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true;

            if (SuccessEvent != null)
            {
                GameManager.Instance.RegisterChoice(eventData.SuccessEvent); // 注册成功事件
            }
            
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        //属性过关但角色不满足要求
        else if (RollTheDice_CharacterStat(eventData, successProbability))
        {
            Result_Story.text = eventData.FailedResults + "骰子成功，但没有满足角色要求。";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            if (FailedEvent != null)
            {
               GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册成功事件 
            }
            
            isSuccess_Event = false;
        }
        //角色满足但属性不满足
        else if (isRoleMatch)
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "角色满足，但骰子不满足要求。";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            if (FailedEvent != null)
            {
                GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            }
            
            isSuccess_Event = false;
        }
        //都不满足
        else
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults + "骰子和角色都不满足";
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";

            if (FailedEvent != null)
            {
                GameManager.Instance.RegisterChoice(eventData.FailedEvent); // 注册失败事件
            }
            
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

    #region 角色下拉框
    /// <summary>
    /// 更新显示
    /// </summary>
    private void PopulateDropdown()
    {
        // 将 enum 每一项转成 string 列表用于下拉显示
        var names = System.Enum.GetNames(typeof(Role)).ToList();
        roleDropdown.ClearOptions();
        roleDropdown.AddOptions(names);
        roleDropdown.value = 0;
        OnDropdownChanged(0);
    }

    /// <summary>
    /// 下拉框选择角色
    /// </summary>
    /// <param name="index"></param>
    private void OnDropdownChanged(int index)
    {
        // 枚举值取决于 index
        selectedRole = (Role)index;
        ValidateSelection();
    }

    /// <summary>
    /// 判断是否匹配成功
    /// </summary>
    private void ValidateSelection()
    {
        feedbackText.gameObject.SetActive(false);
        isRoleMatch = false;

        if (!selectedRole.HasValue) return;

        if (selectedRole.Value != RequiredRole)
        {
            feedbackText.text = $"请选择正确角色（需：{RequiredRole}）";
            feedbackText.color = Color.red;
            feedbackText.gameObject.SetActive(true);
            return;
        }

        // 匹配成功
        isRoleMatch = true;
    }

    /// <summary>
    /// 按钮测试
    /// </summary>
    private void OnConfirm()
    {
        ValidateSelection();
        if (!isRoleMatch)
        {
            feedbackText.text = "当前选择角色不正确。";
            return;
        }
    }

    protected override void SetRight()
    {
        base.SetRight();
    }

    protected override void LockingEvent()
    {
        base.LockingEvent();

        if(roleDropdown != null)
        {
            roleDropdown.interactable = false; // 锁定事件时禁用下拉框
        }
    }

    #endregion
}

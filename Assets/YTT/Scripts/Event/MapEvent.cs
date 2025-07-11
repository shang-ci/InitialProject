/* 
7.3
定义一个类，包含事件应该有的字段
*/
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMapEvent", menuName = "Events/Map Event")]
public class MapEvent : ScriptableObject
{
    [Header("基础信息")]
    public string eventID;// 事件的唯一标识符
    public string title;// 事件标题
    [TextArea(2, 5)]
    public string description;// 事件描述
    public Sprite icon;// 事件图标

    public enum TriggerType { Always, StatBased, DayBased }
    public TriggerType triggerType;
    // 触发类型：总是触发、基于属性触发、基于天数触发

    public int triggerDay;// 触发事件的具体天数（仅在DayBased时有效）
    public int availableFromDay;// 事件可用的起始天数
    public int availableUntilDay;// 事件可用的结束天数

    public string statToCheck;// 触发事件所需检查的属性（仅在StatBased时有效）
    public int requiredStatValue;// 触发事件所需的属性值（仅在StatBased时有效）

    [Header("行为信息")]
    public float durationHours;// 事件持续的小时数
    public string conversationStartNode;// 事件开始时的对话节点

    [Header("胜利条件")]
    public Condition winCondition;// 胜利条件

    [Header("胜利结果")]
    public Outcome winOutcome;// 胜利后的结果

    [Header("失败结果")]
    public Outcome loseOutcome;// 失败后的结果

    // 判断当前事件是否可用（在地图上显示）
    public bool IsAvailable(int currentDay, int currentStat)
    {
        if (currentDay < availableFromDay || currentDay > availableUntilDay)
            return false;

        switch (triggerType)
        {
            case TriggerType.Always:
                return true;
            case TriggerType.DayBased:
                return currentDay == triggerDay;
            case TriggerType.StatBased:
                return currentStat >= requiredStatValue;
            default:
                return false;
        }
    }
}

[System.Serializable]
public class Condition
{
    public string statToCheck;// 需要检查的属性名称
    public int minValueRequired;// 触发事件所需的最小属性值
    public int maxValueRequired;// 触发事件所需的最大属性值
    public string statName;// 需要检查的属性名称（用于胜利条件）
}

[System.Serializable]
public class Outcome
{
    public List<ItemReward> itemRewards = new();//胜利获得的物品
    public List<StatEffect> statEffects = new();//胜利或失败后对属性的影响
    public List<CardData> cardRewards = new();//胜利获得的卡牌
    public List<CardData> cardRemovals = new();//失败移除的卡牌
}

[System.Serializable]
public class ItemReward
{
    public string itemName;// 物品名称
    public int quantity;// 物品数量
}

[System.Serializable]
public class StatEffect
{
    public string statName;// 属性名称
    public int valueChange;// 属性值变化量
}


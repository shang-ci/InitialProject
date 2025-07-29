using System.Collections.Generic;
using UnityEngine;
/*
完美集成：完全融入了您现有的 DataManager 和 EventData 结构，改动最小。
数据驱动：您只需要修改CSV文件，就能任意组合、增删事件的触发条件，无需修改任何C#代码。
高扩展性：当未来需要一种新的触发条件时（比如“角色位置条件”），您只需：在 EventConditionFactory 中增加一个 case。创建一个新的 LocationCondition 子类。
在CSV中就可以直接使用 Location|主城|10 这样的新格式了。
*/

/*
每个条件由 类型|参数1|参数2 这样的结构组成。
如果一个事件有多个条件，则用 分号 (;) 将它们隔开。
*/

/// <summary>
/// 所有事件触发条件的抽象基类
/// </summary>
[System.Serializable]
public abstract class EventTriggerConditionBase
{
    /// <summary>
    /// 检查此条件是否已满足
    /// </summary>
    /// <returns>如果满足则返回true，否则返回false</returns>
    public abstract bool IsMet();
}

// 属性条件
public class AttributeCondition : EventTriggerConditionBase
{
    public string AttributeName;
    public float RequiredValue;

    public override bool IsMet()
    {
        //// 假设您使用Dialogue System，我们从Lua环境中获取变量
        //return PixelCrushers.DialogueSystem.DialogueLua.GetVariable(AttributeName).AsFloat >= RequiredValue;
        return Character.Instance.GetAttribute(AttributeName) >= RequiredValue;
    }
}

// 物品条件
public class ItemCondition : EventTriggerConditionBase
{
    public string RequiredItemID;
    public int RequiredCount = 1;

    public override bool IsMet()
    {
        // 假设您的 DataManager 可以检查物品数量
        return Character.Instance.IsEquippedByDefinitionName(RequiredItemID);
    }
}

// 时间条件
public class TimeCondition : EventTriggerConditionBase
{
    public int RequiredDay;

    public override bool IsMet()
    {
        // 假设您的 DataManager 可以获取当前天数
        return GameManager.Instance.currentDay == RequiredDay;
    }
}

// 前置事件条件
public class EventCompletedCondition : EventTriggerConditionBase
{
    public string RequiredEventID;

    public override bool IsMet()
    {
        // 直接从 CharacterEventManager 查询事件是否已完成
        //return CharacterEventManager.Instance.IsEventCompleted(RequiredEventID);
        return true;
    }
}

// 随机条件
public class RandomCondition : EventTriggerConditionBase
{
    [Range(0f, 1f)]
    public float Chance = 0.5f;

    public override bool IsMet()
    {
        return Random.value <= Chance;
    }
}

public class DirectCondition : EventTriggerConditionBase
{
    public override bool IsMet()
    {
        return true;
    }
}

public class AllDayCondition : EventTriggerConditionBase
{
    public override bool IsMet()
    {
        return true;
    }
}
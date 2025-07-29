using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

/// <summary>
/// 存储单个事件所有信息的数据类
/// </summary>
[System.Serializable]
public class EventData
{
    public string EventID { get; private set; }
    public EventType EventType { get; private set; }
    public SelectType SelectType { get; private set; }
    public string EventName { get; private set; }
    public string EventPrefabName { get; private set; } // 存储Prefab在Resources下的路径
    public string Story { get; private set; }// 事件的故事文本
    public string Tips { get; private set; }// 事件的提示文本
    public List<string> RequiredAttributes { get; private set; } // 需要的属性，已解析为列表
    public List<string> RequiredItems { get; private set; } // 需要的物品
    public int RequiredCoin { get; private set; } //需要的金币
    public Role RequiredRole { get; private set; } //需要的角色 
    public int DurationDays { get; private set; }// 事件持续的天数
    public int SuccessThreshold { get; private set; } // 成功的条件/阈值，投出的成功骰子需要大于这个数
    public string SuccessfulResults { get; private set; } // 成功的结果文本
    public string FailedResults { get; private set; } // 失败的结果文本

    public List<string> RewardItemIDs { get; private set; } // 奖励物品ID，已解析为列表
    public string SuccessEvent { get; private set; } // 成功的后续事件ID
    public string FailedEvent { get; private set; } // 失败的后续事件ID

    [Tooltip("该事件是否可以重复触发？如果为false，则成功完成一次后将不再触发")]
    public bool IsRepeatable;
    public List<EventTriggerConditionBase> Conditions { get; set; } // 事件触发条件列表
    public EventTriggerType triggerType; // 触发类型

    // 构造函数：负责将从CSV读取的原始字符串数据，解析并填充到类的属性中
    public EventData(string[] rawData)
    {
        try
        {
            EventID = rawData[0];

            // 使用 Enum.Parse 将字符串转换为枚举，true表示忽略大小写
            EventType = (EventType)Enum.Parse(typeof(EventType), rawData[1], true);
            SelectType = (SelectType)Enum.Parse(typeof(SelectType), rawData[2], true);

            EventName = rawData[3];
            EventPrefabName = rawData[4]; // 这里直接存储字符串路径
            Story = rawData[5];
            Tips = rawData[6];

            // 解析逗号分隔的字符串为列表
            // Trim()可以去除每个元素前后的空格，以防 "a, b" 这种情况
            RequiredAttributes = rawData[7].Split('、').Select(s => s.Trim()).ToList();
            RequiredItems = rawData[8].Split('、').Select(s  => s.Trim()).ToList();
            RequiredCoin = int.Parse(rawData[9]);
            RequiredRole = (Role)Enum.Parse(typeof(Role), rawData[10], true);

            DurationDays = int.Parse(rawData[11]);
            SuccessThreshold = int.Parse(rawData[12]);

            SuccessfulResults = rawData[13];
            FailedResults = rawData[14];

            RewardItemIDs = rawData[15].Split('、').Select(s => s.Trim()).ToList();
            SuccessEvent = rawData[16]; // 成功后续事件ID
            FailedEvent = rawData[17]; // 失败后续事件ID

            // 事件触发条件的初始化
            Conditions = new List<EventTriggerConditionBase>();

            if (rawData.Length > 18 && !string.IsNullOrEmpty(rawData[18]))
            {
                string allConditionsString = rawData[18];

                // 1. 用分号分割出每个单独的条件字符串
                // "Item|GoldenKey|1;EventCompleted|EVT_001" -> ["Item|GoldenKey|1", "EventCompleted|EVT_001"]
                string[] singleConditionStrings = allConditionsString.Split(';');

                // 单个条件字符串
                foreach (var conditionString in singleConditionStrings)
                {
                    // 2. 用竖线分割出条件的类型和参数
                    // "Item|GoldenKey|1" -> ["Item", "GoldenKey", "1"]
                    string[] parts = conditionString.Split('|');

                    // 3. 使用工厂来创建具体的条件实例
                    EventTriggerConditionBase condition = EventConditionFactory.CreateEventTriggerCondition(parts);

                    if (condition != null)
                    {
                        // 4. 将创建好的条件添加到列表中
                        Conditions.Add(condition);
                    }
                }
            }
            
            triggerType = (EventTriggerType)Enum.Parse(typeof(EventTriggerType), rawData[19], true);

            IsRepeatable = false;
            if (rawData.Length > 20)
            {
                // 使用 string.Equals 并忽略大小写
                IsRepeatable = string.Equals(rawData[20].Trim(), "TRUE", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception e)
        {
            // 如果某一行数据格式错误，这能帮助我们快速定位问题
            Debug.LogError($"Error parsing event data for row with ID {rawData[0]}. Check your CSV format. Error: {e.Message}");
        }
    }
}
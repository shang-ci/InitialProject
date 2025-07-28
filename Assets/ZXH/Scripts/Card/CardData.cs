// CardData.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 使用CreateAssetMenu特性，我们可以在Unity编辑器的 "CreateEventTriggerCondition" 菜单中直接创建这种资源文件。
[CreateAssetMenu(fileName = "New CardData", menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("卡牌基础信息")]
    public string id;
    public string cardName;
    public CardType cardType;
    public Attributes attributes; // 卡牌的属性
    public string description; // 卡牌的描述

    [Header("卡牌视觉表现")]
    public Sprite artwork; // 卡牌的图画
                           // public GameObject cardPrefab; // 如果卡牌有复杂的3D模型或特效，可以在这里引用

    /// <summary>
    /// 读取属性
    /// </summary>
    /// <returns></returns>
    public Attributes GetAttributes()
    {
        return attributes;
    }

    /// <summary>
    /// 设置属性（整体替换）
    /// </summary>
    /// <param name="newAttributes"></param>
    public void SetAttributes(Attributes newAttributes)
    {
        attributes = newAttributes;
    }

    /// <summary>
    /// 单独设置某个属性
    /// </summary>
    /// <param name="attrName"></param>
    /// <param name="value"></param>
    public void SetAttributeValue(string attrName, int value)
    {
        switch (attrName)
        {
            case "physique": attributes.physique = value; break;
            case "social": attributes.social = value; break;
            case "survival": attributes.survival = value; break;
            case "intelligence": attributes.intelligence = value; break;
            case "charm": attributes.charm = value; break;
            case "combat": attributes.combat = value; break;
            case "support": attributes.support = value; break;
        }
    }

    /// <summary>
    /// 单独获取某个属性
    /// </summary>
    /// <param name="attrName"></param>
    /// <returns></returns>
    public int GetAttributeValue(string attrName)
    {
        return attrName switch
        {
            "physique" => attributes.physique,
            "social" => attributes.social,
            "survival" => attributes.survival,
            "intelligence" => attributes.intelligence,
            "charm" => attributes.charm,
            "combat" => attributes.combat,
            "support" => attributes.support,
            _ => 0
        };
    }

    /// <summary>
    /// 属性叠加（如加成）
    /// </summary>
    /// <param name="add"></param>
    public void AddAttributes(Attributes add)
    {
        attributes.physique += add.physique;
        attributes.social += add.social;
        attributes.survival += add.survival;
        attributes.intelligence += add.intelligence;
        attributes.charm += add.charm;
        attributes.combat += add.combat;
        attributes.support += add.support;
    }

    /// <summary>
    /// 属性移除
    /// </summary>
    /// <param name="remove"></param>
    public void RemoveAttributes(Attributes remove)
    {
        attributes.physique -= remove.physique;
        attributes.social -= remove.social;
        attributes.survival -= remove.survival;
        attributes.intelligence -= remove.intelligence;
        attributes.charm -= remove.charm;
        attributes.combat -= remove.combat;
        attributes.support -= remove.support;
    }
}

/// <summary>
/// 卡牌数据队列，用于存储和管理卡牌数据的队列
/// </summary>
public class CardDataQueue
{
    private readonly Queue<CardData> queue = new Queue<CardData>();

    /// <summary>
    /// 将卡牌数据添加到队列中
    /// </summary>
    /// <param name="data"></param>
    public void Enqueue(CardData data)
    {
        queue.Enqueue(data);
    }


    /// <summary>
    /// 从队列中移除并返回最前面的卡牌数据
    /// </summary>
    /// <returns></returns>
    public CardData Dequeue()
    {
        if (queue.Count == 0) return null;
        return queue.Dequeue();
    }

    public int Count => queue.Count;
}

/// <summary>
/// 属性结构体，用于存储卡牌的各种属性值
/// </summary>
[System.Serializable]
public struct Attributes
{
    public int physique;   // 体魄
    public int social;     // 社交
    public int survival;   // 生存
    public int intelligence; // 智慧
    public int charm;      // 魅力
    public int combat;     // 战斗
    public int support;    // 支持
}
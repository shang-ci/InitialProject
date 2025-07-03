// CardManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌管理器
/// </summary>
public class CardManager : MonoBehaviour
{
    [Header("卡牌数据库")]
    public List<CardData> cardDatabase; // 存储所有可能的卡牌数据（ScriptableObjects）

    [Header("生成设置")]
    public GameObject cardPrefab;       // 卡牌的预制体
    public GameObject cardSlotPrefab;  // 卡牌槽的预制体
    public Transform handParent;         // 手牌区域的父物体Transform

    [Header("测试")]
    [Tooltip("在开始时自动生成几张牌")]
    public int startingHandSize = 5;

    void Start()
    {
        // 游戏开始时发牌
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCard();
        }
    }

    // 从数据库中随机抽取一张卡牌并创建
    public void DrawCard()
    {
        if (cardDatabase == null || cardDatabase.Count == 0)
        {
            Debug.LogError("卡牌数据库为空，无法抽卡！");
            return;
        }

        if (cardPrefab == null || handParent == null)
        {
            Debug.LogError("请在CardManager中设置Card Prefab和Hand Panel！");
            return;
        }

        // 1. 随机选择一张卡牌数据
        CardData randomCardData = cardDatabase[Random.Range(0, cardDatabase.Count)];

        // 2. 实例化卡牌槽预制体
        GameObject newCardSlotObject = Instantiate(cardSlotPrefab, handParent);
        CardSlot newCardSlot = newCardSlotObject.GetComponent<CardSlot>();


        // 3. 获取卡牌脚本并设置数据
        Card newCard = newCardSlotObject.GetComponent<Card>();
        if (newCard != null)
        {
            newCard.cardData = randomCardData;
            // 手动调用SetupCard来更新卡牌的视觉表现
            newCard.SetupCard();
        }
        else
        {
            Debug.LogError("卡牌预制体上没有找到Card脚本！");
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("卡牌数据库")]
    public List<CardData> cardDatabase;

    [Header("生成设置")]
    public GameObject cardPrefab;// 卡牌预制件，用于生成卡牌实例
    public GameObject cardSlotPrefab;// 卡牌槽预制件，用于放置卡牌的容器
    public Transform handParent;// 卡牌槽的父物体，通常是手牌区域

    private CardDataQueue cardQueue = new CardDataQueue();// 卡牌数据队列，用于存储和管理卡牌数据

    void Start()
    {
        // 初始化队列
        foreach (var data in cardDatabase)
        {
            cardQueue.Enqueue(data);
        }
        // 发牌
        for (int i = 0; i < 4; i++)
        {
            DrawCard();
        }
    }

    /// <summary>
    /// 从卡牌队列中抽取一张卡牌并生成对应的卡牌实例
    /// </summary>
    public void DrawCard()
    {
        if (cardQueue.Count == 0)
        {
            Debug.LogWarning("没有可用的卡牌数据！");
            return;
        }

        CardData data = cardQueue.Dequeue();
        if (data == null) return;

        // 1. 生成卡牌槽
        GameObject slotObj = Instantiate(cardSlotPrefab, handParent);
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.acceptedCardType = data.cardType;
        slot.SetSlotColor(data.cardType);

        // 2. 生成卡牌
        GameObject cardObj = Instantiate(cardPrefab);
        Card card = cardObj.GetComponent<Card>();
        card.cardData = data;
        card.SetupCard();

        // 3. 放入卡牌槽
        slot.SetChild(card);
    }
}

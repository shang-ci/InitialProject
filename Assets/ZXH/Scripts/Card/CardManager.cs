using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("数据")]
    public List<CardData> cardDatabase;

    [Header("引用")]
    public GameObject cardPrefab;
    public GameObject cardSlotPrefab;
    public Transform handParent;

    public CardDataQueue cardQueue = new CardDataQueue();//所有手牌
    public List<CardData> handCards = new List<CardData>();//当前手牌——invent库


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 初始化手牌
    /// </summary>
    private void InitHandCard()
    {
        for (int i = 0; i < 5; i++)
        {
            AddCardData();
        }
    }

    /// <summary>
    /// 加入手牌堆，只有这一个加入方法
    /// </summary>
    public void AddCardData()
    {
        if (cardQueue.Count == 0)
        {
            //Debug.LogWarning("û�п��õĿ������ݣ�");
            return;
        }

        CardData data = cardQueue.Dequeue();
        if (data == null) return;

        // 1. ���ɿ��Ʋ�
        GameObject slotObj = Instantiate(cardSlotPrefab, handParent);
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.acceptedCardType = data.cardType;
        slot.SetSlotColor(data.cardType);

        // 2. ���ɿ���
        GameObject cardObj = Instantiate(cardPrefab);
        Card card = cardObj.GetComponent<Card>();
        card.cardData = data;
        card.SetupCard();

        // 3. ���뿨�Ʋ�
        slot.SetChild(card);
    }


    public void AddCard(CardData cardData)
    {
        // 你可以直接把卡牌加入手牌或牌库，这里以加入手牌为例
        GameObject slotObj = Instantiate(cardSlotPrefab, handParent);
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slot.acceptedCardType = cardData.cardType;
        slot.SetSlotColor(cardData.cardType);

        GameObject cardObj = Instantiate(cardPrefab);
        Card card = cardObj.GetComponent<Card>();
        card.cardData = cardData;
        card.SetupCard();

        slot.SetChild(card);
    }

    public void RemoveCard(CardData cardData)
    {
        // 遍历手牌，找到第一个匹配的卡牌并移除
        foreach (Transform slotTransform in handParent)
        {
            CardSlot slot = slotTransform.GetComponentInChildren<CardSlot>();
            if (slot != null && slot.HasCard())
            {
                Card card = slot.GetCard();
                if (card != null && card.cardData == cardData)
                {
                    Destroy(card.gameObject);
                    break;
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("�������ݿ�")]
    public List<CardData> cardDatabase;

    [Header("��������")]
    public GameObject cardPrefab;// ����Ԥ�Ƽ����������ɿ���ʵ��
    public GameObject cardSlotPrefab;// ���Ʋ�Ԥ�Ƽ������ڷ��ÿ��Ƶ�����
    public Transform handParent;// ���Ʋ۵ĸ����壬ͨ������������

    private CardDataQueue cardQueue = new CardDataQueue();//所有手牌

    void Start()
    {
        // ��ʼ������
        foreach (var data in cardDatabase)
        {
            cardQueue.Enqueue(data);
        }
        // ����
        for (int i = 0; i < 4; i++)
        {
            DrawCard();
        }
    }

    /// <summary>
    /// �ӿ��ƶ����г�ȡһ�ſ��Ʋ����ɶ�Ӧ�Ŀ���ʵ��
    /// </summary>
    public void DrawCard()
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
            CardSlot slot = slotTransform.GetComponent<CardSlot>();
            if (slot != null && slot.HasCard())
            {
                Card card = slot.GetCard();
                if (card != null && card.cardData == cardData)
                {
                    Destroy(card.gameObject);
                    // 你可以选择是否销毁slot
                    // Destroy(slot.gameObject);
                    break;
                }
            }
        }
    }
}

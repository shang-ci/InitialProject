using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡牌槽属性")]
    public CardType acceptedCardType;
    public int level;
    public Card child; // 当前卡槽内的卡牌


    [Header("类型颜色")]
    public Color roleColor = new Color(0.6f, 0.8f, 1f);   // 角色：淡蓝
    public Color intelColor = new Color(1f, 0.9f, 0.5f);  // 情报：淡黄
    public Color bookColor = new Color(0.8f, 1f, 0.8f);   // 书籍：淡绿
    public Color coinColor = new Color(1f, 0.85f, 0.6f);  // 金币：淡橙


    private void Awake()
    {
        SetSlotColor(acceptedCardType); // 初始化卡槽颜色
    }

    /// <summary>
    /// 根据类型设置卡槽颜色
    /// </summary>
    public void SetSlotColor(CardType type)
    {
        Image img = GetComponent<Image>();
        if (img == null) return;
        switch (type)
        {
            case CardType.Role:
                img.color = roleColor;
                break;
            case CardType.Intel:
                img.color = intelColor;
                break;
            case CardType.Book:
                img.color = bookColor;
                break;
            case CardType.Coin:
                img.color = coinColor;
                break;
        }
    }


    /// <summary>
    /// 设置当前卡槽内的卡牌
    /// </summary>
    /// <param name="card">传经来的卡牌</param>
    public void SetChild(Card card)
    {
        //card.SetNewParent(transform); // 更新卡牌的父物体为当前卡槽
        //card.SetupCard(); // 更新卡牌的显示
        //card.SetCardPos(); // 设置卡牌位置为卡槽中心

        //child = card;

        if (card == null) return;
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
        child = card;
        card.SetNewParent(this.transform);
        card.SetupCard(); // 更新卡牌的显示
    }


    // 处理卡牌放置事件
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        Card draggedCard = droppedObject.GetComponent<Card>();
        if (draggedCard == null) return;

        if (draggedCard.cardData.cardType == acceptedCardType)
        {
            if (child != null && child != draggedCard)
            {
                // 交换：把原有卡牌放回拖拽卡牌的原父物体
                Transform oldParent = draggedCard.OriginalParent;
                child.transform.SetParent(oldParent);
                child.transform.localPosition = Vector3.zero;
                child.SetNewParent(oldParent);

                // 更新原父物体的child（如果是CardSlot）
                CardSlot oldSlot = oldParent.GetComponent<CardSlot>();
                if (oldSlot != null)
                {
                    oldSlot.child = child;
                }
            }

            // 放置新卡牌
            SetChild(draggedCard);
        }
        else
        {
            Debug.LogWarning($"类型不匹配! 卡槽需要 {acceptedCardType}, 但拖来的是 {draggedCard.cardData.cardType}.");
        }
    }
}

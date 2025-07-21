using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 卡牌槽，负责管理卡牌的放置、交换和类型限制
/// </summary>
public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡牌槽属性")]
    public CardType acceptedCardType; // 该槽接受的卡牌类型
    public int level;                  // 槽的等级（可扩展用）
    public Card child;                 // 当前槽内的卡牌

    [Header("类型颜色")]
    public Color roleColor = new Color(0.6f, 0.8f, 1f);   // 角色：淡蓝
    public Color intelColor = new Color(1f, 0.9f, 0.5f);  // 情报：淡黄
    public Color bookColor = new Color(0.8f, 1f, 0.8f);   // 书籍：淡绿
    public Color coinColor = new Color(1f, 0.85f, 0.6f);  // 金币：淡橙

    [Header("卡牌槽辅助属性")]
    public bool isDroppable = true; //控制卡槽是否可放置
    private Image slotImage; // 控制是否接受射线

    private void Awake()
    {
        // 初始化卡槽颜色
        SetSlotColor(acceptedCardType);

        slotImage = GetComponent<Image>();
    }

    /// <summary>
    /// 根据卡牌类型设置卡槽的背景颜色
    /// </summary>
    /// <param name="type">卡牌类型</param>
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
    /// 设置当前卡槽内的卡牌，并建立父子关系
    /// </summary>
    /// <param name="card">要放入的卡牌</param>
    public void SetChild(Card card)
    {
        if (card == null) return;
        // 设置卡牌的父物体为当前卡槽
        card.transform.SetParent(this.transform);
        card.transform.localPosition = Vector3.zero;
        // 更新child引用
        child = card;
        // 通知卡牌其新父物体
        card.SetNewParent(this.transform);
        // 刷新卡牌显示
        card.SetupCard();
    }

    /// <summary>
    /// 处理拖拽放置事件
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public void OnDrop(PointerEventData eventData)
    {
        // 如果当前卡槽被设置为不可放置，则直接返回——锁定状态
        if (!isDroppable)
        {
            return;
        }

        // 检查拖拽的物体是否为卡牌
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        // 获取拖拽的卡牌组件
        Card draggedCard = droppedObject.GetComponent<Card>();
        if (draggedCard == null) return;

        // 类型匹配才允许放置
        if (draggedCard.cardData.cardType == acceptedCardType)
        {
            // 如果当前槽已有卡牌且不是自己，进行交换
            if (child != null && child != draggedCard)
            {
                Transform oldParent = draggedCard.OriginalParent;
                // 原有卡牌放回拖拽卡牌的原父物体
                //child.transform.SetParent(oldParent);
                //child.transform.localPosition = Vector3.zero;
                //child.SetNewParent(oldParent);
                //SetChild(child); // 将当前槽的卡牌放回拖拽卡牌的原父物体

                // 如果原父物体是卡槽，更新其child引用
                CardSlot oldSlot = oldParent.GetComponent<CardSlot>();
                if (oldSlot != null)
                {
                    oldSlot.SetChild(child);
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

    /// <summary>
    /// 判断卡槽内是否有卡牌
    /// </summary>
    public bool HasCard()
    {
        return child != null;
    }

    /// <summary>
    /// 获取当前卡槽内的卡牌
    /// </summary>
    public Card GetCard()
    {
        return child;
    }

    /// <summary>
    /// 设置卡槽及其子卡的可交互状态
    /// </summary>
    /// <param name="interactable">是否可交互</param>
    public void SetInteractable(bool interactable)
    {
        // 1. 控制是否可以放置
        isDroppable = interactable;

        // 2. 控制UI元素是否接收鼠标事件——射线
        if (slotImage != null)
        {
            slotImage.raycastTarget = interactable;
        }

        // 3. 同时控制其内部卡牌是否可以被拖拽
        if (child != null)
        {
            child.isDraggable = interactable;
        }
    }

}

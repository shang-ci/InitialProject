// CardSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡牌槽属性")]
    public CardType acceptedCardType;
    public int level;
    public Card child; // 当前卡槽内的卡牌（如果有的话）


    /// <summary>
    /// 设置当前卡槽内的卡牌
    /// </summary>
    /// <param name="card">传经来的卡牌</param>
    public void SetChild(Card card)
    {
        card.SetNewParent(transform); // 更新卡牌的父物体为当前卡槽
        card.SetupCard(); // 更新卡牌的显示
        card.SetCardPos(); // 设置卡牌位置为卡槽中心

        child = card;
    }


    // 处理卡牌放置事件
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop event triggered on " + gameObject.name);

        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        Card draggedCard = droppedObject.GetComponent<Card>();
        if (draggedCard == null) return;

        // 核心判断：类型是否匹配
        if (draggedCard.cardData.cardType == acceptedCardType)
        {
            // 判断卡槽是否已经被占用
            if (transform.childCount > 0)
            {
                // --- 卡牌交换逻辑 ---
                Debug.Log("卡槽已被占用，执行交换操作。");

                // 获取卡槽中原有的卡牌
                Card cardInSlot = transform.GetChild(0).GetComponent<Card>();
                if (cardInSlot != null && cardInSlot != draggedCard) // 确保不是和自己交换
                {
                    // 获取被拖拽卡牌的原始位置
                    Transform originalCardParent = draggedCard.OriginalParent;

                    // 1. 将卡槽中的旧卡牌移动到被拖拽卡牌的原始位置
                    cardInSlot.transform.SetParent(originalCardParent);
                    cardInSlot.transform.localPosition = Vector3.zero;
                    cardInSlot.SetNewParent(originalCardParent); // 更新旧卡牌的“家”
                    SetChild(cardInSlot); // 清除旧卡牌的子卡牌引用

                    // 2. 将被拖拽的新卡牌放置到当前卡槽
                    draggedCard.transform.SetParent(this.transform);
                    draggedCard.transform.localPosition = Vector3.zero;
                    draggedCard.SetNewParent(this.transform); // 更新新卡牌的“家”
                    SetChild(draggedCard); // 更新当前卡槽的子卡牌
                }
            }
            else
            {
                // --- 简单放置逻辑 (卡槽为空) ---
                Debug.Log($"成功放置卡牌: {draggedCard.cardData.cardName} 到卡槽: {gameObject.name}");

                draggedCard.transform.SetParent(this.transform);
                draggedCard.transform.localPosition = Vector3.zero;
                draggedCard.SetNewParent(this.transform);
                SetChild(draggedCard); // 更新当前卡槽的子卡牌
            }
        }
        else
        {
            Debug.LogWarning($"类型不匹配! 卡槽需要 {acceptedCardType}, 但拖来的是 {draggedCard.cardData.cardType}.");
            // 类型不匹配时，Card.cs中的OnEndDrag会自动处理返回原位
        }
    }
}

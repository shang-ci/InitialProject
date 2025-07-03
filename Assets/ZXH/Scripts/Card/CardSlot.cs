// CardSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [Header("卡牌槽属性")]
    public CardType acceptedCardType;
    public int level;
    public Card child; // 当前卡槽内的卡牌（如果有的话）

    // 当有物体被拖拽到此卡槽上并释放时调用
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop event triggered on " + gameObject.name);

        // 1. 获取被拖拽的物体
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        // 2. 获取该物体上的 Card 脚本
        Card card = droppedObject.GetComponent<Card>();
        if (card == null) return; // 如果拖来的不是卡牌，则不处理

        // 3. 核心逻辑：判断卡牌类型是否匹配
        if (card.cardData.cardType == acceptedCardType)
        {
            // 类型匹配，允许放置
            Debug.Log($"成功放置卡牌: {card.cardData.cardName} 到卡槽: {gameObject.name}");

            // 如果这个卡槽已经有卡牌了，可以实现交换逻辑（可选）
            // 这里我们先做简单的：直接让新卡牌成为子物体
            if (transform.childCount > 0)
            {
                // 可以将旧卡牌弹回原处，或者和其他卡牌交换位置
                // 为简化，此处暂不处理
                Debug.Log("卡槽已被占用！");
                return; // 或者执行交换逻辑
            }

            // 4. 吸附卡牌
            // 将卡牌的父物体设置为当前卡槽
            card.transform.SetParent(this.transform);
            // 将卡牌的位置重置到卡槽中心
            card.transform.localPosition = Vector3.zero;

            // 通知卡牌它已经被成功放置
            card.OnPlacedSuccessfully(this.transform);
        }
        else
        {
            // 类型不匹配，卡牌会自动返回原位（在Card.cs中实现）
            Debug.LogWarning($"类型不匹配! 卡槽需要 {acceptedCardType}, 但拖来的是 {card.cardData.cardType}.");
        }
    }
}
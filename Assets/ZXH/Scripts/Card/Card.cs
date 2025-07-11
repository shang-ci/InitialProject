// Card.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 确保卡牌对象上有Image和CanvasGroup组件
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("卡牌数据")]
    public CardData cardData; // 引用我们创建的ScriptableObject
    public CardType cardType; // 卡牌类型（可以从CardData中获取）
    public TextMeshProUGUI id; // 卡牌ID（可以从CardData中获取）
    public TextMeshProUGUI cardName; // 卡牌名称（可以从CardData中获取）
    public TextMeshProUGUI type; // 卡牌类型（可以从CardData中获取）

    private Transform originalParent; // 记录拖拽前的父物体
    public Transform OriginalParent { get { return originalParent; } }  

    private CanvasGroup canvasGroup;
    private Image cardImage;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        cardImage = GetComponent<Image>();
        SetupCard();
    }

    // 根据CardData初始化卡牌的显示
    public void SetupCard()
    {
        if (cardData != null)
        {
            if (cardImage != null && cardData.artwork != null)
            {
                cardImage.sprite = cardData.artwork;
            }

            gameObject.name = "Card_" + cardData.cardName;

            id.text = "id:" + cardData.id;
            cardName.text = "Name:" + cardData.cardName;
            type.text = "Type:" + cardData.cardType.ToString();
        }

        PrintAllAttributes(); // 打印所有属性信息
    }

    // 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 记录原始父物体，用于拖拽失败时返回
        originalParent = transform.parent;

        // 2. 将卡牌的父物体设置为Canvas的根节点，使其渲染在最上层
        transform.SetParent(transform.root);
        transform.SetAsLastSibling(); // 确保在最上层渲染

        // 3. 设置CanvasGroup，让卡牌在拖拽时可以穿透射线检测
        // 这样下方的卡牌槽才能接收到OnDrop事件
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f; // 半透明效果，给用户拖拽的视觉反馈
    }

    // 拖拽过程中
    public void OnDrag(PointerEventData eventData)
    {
        // 更新卡牌的位置跟随鼠标/手指
        transform.position = eventData.position;
    }

    // 结束拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        // 恢复卡牌的射线检测和透明度
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // 检查卡牌是否被放置到了新的父物体（即一个有效的卡牌槽）下
        // 如果父物体没有改变，说明没有找到合适的卡牌槽
        if (transform.parent == transform.root || transform.parent == originalParent)
        {
            // 没有放置到有效卡槽，返回原位
            SetCardPos();
        }
    }

    // 右键点击时弹出卡牌详情
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardDetailPanel.Instance.Show(cardData, Input.mousePosition);
        }
    }


    // 记录当前父物体——设置当被卡槽成功吸附后，由卡槽调用
    public void SetNewParent(Transform newParent)
    {
        originalParent = newParent;
    }

    /// <summary>
    /// 设置卡牌的位置为原始父物体的位置
    /// </summary>
    public void SetCardPos()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void PrintAllAttributes()
    {
        if (cardData == null)
        {
            Debug.LogWarning("CardData 为空，无法打印属性。");
            return;
        }

        string[] attrNames = new string[]
        {
        "physique", "social", "survival", "intelligence", "charm", "combat", "support"
        };

        string info = $"卡牌 [{cardData.cardName}] 属性：";
        foreach (var attrName in attrNames)
        {
            int value = cardData.GetAttributeValue(attrName);
            info += $"\n{attrName}: {value}";
        }
        Debug.Log(info);
    }
}
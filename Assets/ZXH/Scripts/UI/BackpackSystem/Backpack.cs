using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包系统
/// </summary>
public class Backpack : MonoBehaviour
{
    [Header("头部按钮 (Buttons)")]
    public Button roleButton;
    public Button intelButton;
    public Button bookButton;
    public Button coinButton;

    [Header("数据内容面板 (Panels)")]
    public Transform rolePanel;   // 存放角色卡牌的面板
    public Transform intelPanel;  // 存放情报卡牌的面板
    public Transform bookPanel;   // 存放书籍卡牌的面板
    public Transform coinPanel;   // 存放金币卡牌的面板

    [Header("预制体 (Prefabs)")]
    public GameObject CardSlotPrafbe; // 卡槽预制体
    public GameObject CardPrafbe;     // 卡牌预制体

    // 使用字典按类型分类存储所有卡牌数据
    private Dictionary<CardType, List<CardData>> cardCollection = new Dictionary<CardType, List<CardData>>();
    public List<CardData> cardDatabase;

    private Transform currentActivePanel; // 记录当前显示的面板

    void Start()
    {
        // 1. 初始化数据存储结构，为每种类型创建一个空列表
        cardCollection.Add(CardType.Role, new List<CardData>());
        cardCollection.Add(CardType.Intel, new List<CardData>());
        cardCollection.Add(CardType.Book, new List<CardData>());
        cardCollection.Add(CardType.Coin, new List<CardData>());

        // 2. 为每个按钮添加点击事件的监听器
        roleButton.onClick.AddListener(() => SwitchToCategory(CardType.Role));
        intelButton.onClick.AddListener(() => SwitchToCategory(CardType.Intel));
        bookButton.onClick.AddListener(() => SwitchToCategory(CardType.Book));
        coinButton.onClick.AddListener(() => SwitchToCategory(CardType.Coin));

        // 将所有卡牌数据添加到背包中
        cardDatabase = CardManager.Instance.cardDatabase;
        foreach (CardData card in cardDatabase)
        {
            AddCard(card); 
        }

        // 3. 游戏开始时，默认显示第一个分类 (例如 "Role")
        SwitchToCategory(CardType.Role);
    }

    /// <summary>
    /// 向背包中添加一张新卡牌——只是加入数据，与代码分离
    /// 其他脚本（如游戏管理器、任务奖励等）将调用此方法
    /// </summary>
    /// <param name="data">要添加的卡牌数据</param>
    public void AddCard(CardData data)
    {
        if (data == null)
        {
            Debug.LogWarning("尝试添加空的卡牌数据！");
            return;
        }

        // 检查字典中是否存在该卡牌类型的列表
        if (cardCollection.ContainsKey(data.cardType))
        {
            // 将卡牌数据添加到对应的列表中
            cardCollection[data.cardType].Add(data);
            Debug.Log($"已将卡牌 '{data.cardName}' 添加到背包的 {data.cardType} 分类中");

            // 如果新添加的卡牌正好属于当前正在查看的分类，则刷新面板以立即显示新卡牌
            if (currentActivePanel == GetPanelForType(data.cardType))
            {
                PopulatePanel(currentActivePanel, data.cardType);
            }
        }
        else
        {
            Debug.LogWarning($"未知的卡牌类型: {data.cardType}, 无法添加到背包。");
        }
    }

    /// <summary>
    /// 切换并显示指定类型的卡牌面板
    /// </summary>
    /// <param name="category">要显示的卡牌类型</param>
    private void SwitchToCategory(CardType category)
    {
        // 先隐藏所有面板
        rolePanel.gameObject.SetActive(false);
        intelPanel.gameObject.SetActive(false);
        bookPanel.gameObject.SetActive(false);
        coinPanel.gameObject.SetActive(false);

        // 找到并激活目标面板
        Transform panelToActivate = GetPanelForType(category);
        if (panelToActivate != null)
        {
            panelToActivate.gameObject.SetActive(true);
            currentActivePanel = panelToActivate; // 更新当前激活的面板记录

            // 在激活的面板中生成所有对应的卡牌
            PopulatePanel(panelToActivate, category);
        }
    }

    /// <summary>
    /// 在指定面板中填充该类型的所有卡牌
    /// </summary>
    /// <param name="panel">卡牌要被创建在哪个父面板下</param>
    /// <param name="category">要生成的卡牌类型</param>
    private void PopulatePanel(Transform panel, CardType category)
    {
        Transform contentParent = panel.transform.Find("Viewport/Content");

        // 1. 清空面板中所有旧的卡牌/卡槽，防止重复生成
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 从数据集合中获取该分类的所有卡牌数据
        List<CardData> cardsToDisplay = cardCollection[category];

        // 3. 遍历数据列表，为每张卡牌创建UI实例
        foreach (CardData data in cardsToDisplay)
        {
            GameObject slotObj = Instantiate(CardSlotPrafbe, contentParent);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slot.acceptedCardType = data.cardType;
            slot.SetSlotColor(data.cardType);

            GameObject cardObj = Instantiate(CardPrafbe);
            Card card = cardObj.GetComponent<Card>();
            card.cardData = data;
            card.SetupCard();

            slot.SetChild(card);
        }
    }

    /// <summary>
    /// 根据卡牌类型返回对应的Transform面板
    /// </summary>
    private Transform GetPanelForType(CardType type)
    {
        return type switch
        {
            CardType.Role => rolePanel,
            CardType.Intel => intelPanel,
            CardType.Book => bookPanel,
            CardType.Coin => coinPanel,
            _ => null,
        };
    }

    /// <summary>
    /// 从背包中移除一张指定的卡牌
    /// </summary>
    /// <param name="dataToRemove">要移除的卡牌的数据</param>
    public void RemoveCard(CardData dataToRemove)
    {
        if (dataToRemove == null)
        {
            Debug.LogWarning("尝试移除一个空的卡牌数据！");
            return;
        }

        // 检查字典中是否存在该卡牌的分类
        if (cardCollection.ContainsKey(dataToRemove.cardType))
        {
            // 从对应的列表中移除第一个匹配的卡牌数据
            bool removed = cardCollection[dataToRemove.cardType].Remove(dataToRemove);

            if (removed)
            {
                Debug.Log($"已从数据中移除卡牌 '{dataToRemove.cardName}'");

                // 检查这张被移除的卡牌所在的分类面板当前是否为激活状态
                if (currentActivePanel == GetPanelForType(dataToRemove.cardType))
                {
                    // 如果是，则调用PopulatePanel来刷新整个面板，
                    // 这会自动移除不再存在于数据列表中的卡牌UI
                    PopulatePanel(currentActivePanel, dataToRemove.cardType);
                }
            }
            else
            {
                Debug.LogWarning($"在背包数据中未找到要移除的卡牌 '{dataToRemove.cardName}'。");
            }
        }
    }
}
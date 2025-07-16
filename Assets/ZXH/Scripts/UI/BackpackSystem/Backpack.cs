using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包系统 - 已修改为使用下拉菜单筛选
/// </summary>
public class Backpack : MonoBehaviour
{
    [Header("头部")]
    public TMP_Dropdown categoryDropdown; // 分类下拉菜单
    public Button equip; // 装备按钮
    public Button synthesis; // 合成按钮
    public Button system; // 系统按钮

    [Header("内容面板")]
    public Transform cardContentParent; // 所有卡牌UI的父物体，这是唯一的显示区域

    [Header("预制体 (Prefabs)")]
    public GameObject CardSlotPrafbe; // 卡槽预制体
    public GameObject CardPrafbe;     // 卡牌预制体

    [Header("数据")]
    // 使用字典按类型分类存储所有卡牌数据
    private Dictionary<CardType, List<CardData>> cardCollection = new Dictionary<CardType, List<CardData>>();
    public List<CardData> cardDatabase; // 初始是手动添加的卡牌

    [Header("装备")]
    // 存储当前的状态
    [SerializeField]private EquipState currentState = EquipState.None;
    // 临时存放第一步选中的角色卡
    [SerializeField]private RoleCardData selectedRole;

    // 定义装备流程的各个状态
    private enum EquipState
    {
        None,               // 正常浏览状态
        SelectingCharacter, // 等待玩家选择一个角色卡
        SelectingEquipment  // 等待玩家选择一件装备卡
    }


    private void OnEnable()
    {
        equip.onClick.AddListener(StartEquipProcess);
    }

    private void OnDisable()
    {
        equip.onClick.RemoveListener(StartEquipProcess);
    }

    private void Start()
    {
        InitializeBackpack();
    }

    #region 公用方法

    /// <summary>
    /// 向背包中添加一张新卡牌数据
    /// </summary>
    public void AddCard(CardData data)
    {
        if (data != null && cardCollection.ContainsKey(data.cardType))
        {
            cardCollection[data.cardType].Add(data);
            cardDatabase.Add(data);
            Debug.Log($"已将卡牌 '{data.cardName}' 添加到背包数据中。");

            // 刷新显示，以反映数据的变化
            OnFilterChanged(categoryDropdown.value);
        }

        //加入手牌堆
        CardManager.Instance.cardQueue.Enqueue(data);
        CardManager.Instance.AddCardData();
    }

    /// <summary>
    /// 从背包中移除一张指定的卡牌数据
    /// </summary>
    public void RemoveCard(CardData dataToRemove)
    {
        if (dataToRemove != null && cardCollection.ContainsKey(dataToRemove.cardType))
        {
            bool removed = cardCollection[dataToRemove.cardType].Remove(dataToRemove);
            if (removed)
            {
                cardDatabase.Remove(dataToRemove); // 从数据库中移除
                Debug.Log($"已从数据中移除卡牌 '{dataToRemove.cardName}'");

                // 刷新显示，以反映数据的变化
                OnFilterChanged(categoryDropdown.value);
            }
        }

        //从手牌堆中移除
        CardManager.Instance.RemoveCard(dataToRemove);
    }
    #endregion

    #region  下拉框
    /// <summary>
    /// 初始化背包：数据和UI和下拉框
    /// </summary>
    private void InitializeBackpack()
    {
        // 初始化数据存储结构
        cardCollection.Clear();
        foreach (CardType type in System.Enum.GetValues(typeof(CardType)))
        {
            cardCollection.Add(type, new List<CardData>());
        }

        // 加载初始数据
        //cardDatabase = CardManager.Instance.cardDatabase;
        foreach (CardData card in cardDatabase)
        {
            if (cardCollection.ContainsKey(card.cardType))
            {
                cardCollection[card.cardType].Add(card);
            }
        }

        // 设置下拉菜单的选项
        SetupFilterDropdown();

        // 绑定下拉菜单的值变化事件
        categoryDropdown.onValueChanged.AddListener(OnFilterChanged);

        // 游戏开始时，默认显示
        OnFilterChanged(0);
    }

    /// <summary>
    /// 显示对应的卡牌——当下拉菜单选项改变时调用
    /// </summary>
    /// <param name="index">选中项的索引</param>
    private void OnFilterChanged(int index)
    {
        ResetEquipProcess();//打断装备过程

        // 如果选择的是第一项“全部”
        if (index == 0)
        {
            PopulateDisplay(null); // 传入null代表显示全部
        }
        // 拿到其余选项
        else
        {
            // 我们的选项列表index=1对应枚举的index=0，所以要减1
            string typeName = categoryDropdown.options[index].text;
            CardType selectedType = (CardType)System.Enum.Parse(typeof(CardType), typeName);
            PopulateDisplay(selectedType);
        }
    }

    /// <summary>
    /// 在内容面板中填充卡牌
    /// </summary>
    /// <param name="filterType">要筛选的类型。如果为null，则显示所有类型的卡牌。</param>
    private void PopulateDisplay(CardType? filterType)
    {
        // 清空面板中所有旧的卡牌/卡槽
        foreach (Transform child in cardContentParent)
        {
            Destroy(child.gameObject);
        }

        // 根据筛选条件决定要显示哪些卡牌
        if (filterType.HasValue)
        {
            // 显示特定类型的卡牌 
            if (cardCollection.ContainsKey(filterType.Value))
            {
                List<CardData> cardsToDisplay = cardCollection[filterType.Value];
                CreateCardUI(cardsToDisplay);
            }
        }
        else
        {
            // 显示全部卡牌 
            foreach (var categoryList in cardCollection.Values)
            {
                CreateCardUI(categoryList);
            }
        }
    }

    /// <summary>
    /// 根据给定的数据列表创建UI元素
    /// </summary>
    /// <param name="dataList">卡牌数据列表</param>
    private void CreateCardUI(List<CardData> dataList)
    {
        foreach (CardData data in dataList)
        {
            // 实例化卡槽和卡牌
            GameObject slotObj = Instantiate(CardSlotPrafbe, cardContentParent);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slot.acceptedCardType = data.cardType;
            slot.SetSlotColor(data.cardType);

            GameObject cardObj = Instantiate(CardPrafbe);
            Card card = cardObj.GetComponent<Card>();
            card.cardData = data;
            card.SetupCard();

            // 将卡牌放入卡槽
            slot.SetChild(card);
        }
    }

    /// <summary>
    /// 设置筛选下拉菜单的选项——初始化时调用
    /// </summary>
    private void SetupFilterDropdown()
    {
        categoryDropdown.ClearOptions();
        List<string> options = new List<string> { "全部" };

        // 枚举中获取所有类型名称并添加到选项中
        options.AddRange(System.Enum.GetNames(typeof(CardType)));

        categoryDropdown.AddOptions(options);
    }

    #endregion

    #region 装备

    /// <summary>
    /// 开始装备流程，入口
    /// </summary>
    private void StartEquipProcess()
    {
        Debug.Log("开始装备流程：请选择一个角色");
        currentState = EquipState.SelectingCharacter;

        // 筛选并只显示角色卡牌
        PopulateDisplay(CardType.Role);
    }

    /// <summary>
    /// 重置/取消装备流程， 出口
    /// </summary>
    private void ResetEquipProcess()
    {
        currentState = EquipState.None;
        selectedRole = null;
        Debug.Log("装备流程已取消或完成");
    }

    /// <summary>
    /// 当卡牌被点击时
    /// </summary>
    /// <param name="cardData">被点击的卡牌</param>
    public void SelectCard(CardData cardData)
    {
        // 根据当前的装备状态，决定如何响应这次点击
        switch (currentState)
        {
            // 正在等待选择角色
            case EquipState.SelectingCharacter:
                if (cardData.cardType == CardType.Role)
                {
                    // 玩家点击了一张角色卡
                    selectedRole = cardData as RoleCardData; // 存储选中的角色
                    Debug.Log($"已选择角色: {selectedRole.cardName}，请选择一件装备");

                    // 选择装备
                    currentState = EquipState.SelectingEquipment;
                    PopulateDisplay(CardType.Equip); // 筛选并只显示装备卡
                }
                break;

            // 正在等待选择装备
            case EquipState.SelectingEquipment:
                if (cardData.cardType == CardType.Equip)
                {
                    // 玩家点击了一张装备卡
                    EquipCardData selectedEquip = cardData as EquipCardData;
                    Debug.Log($"准备为 {selectedRole.cardName} 装备 {selectedEquip.cardName}");

                    // 调用你已经写好的装备逻辑
                    Equip(selectedEquip, selectedRole);

                    // 装备完成，重置整个流程
                    ResetEquipProcess();

                    // 刷新UI回到默认的全部状态
                    OnFilterChanged(0);
                    categoryDropdown.value = 0; // 将下拉菜单的显示也重置为“全部”
                }
                break;

            // 常浏览状态，什么都不做 
            case EquipState.None:

            default:
                Debug.Log($"点击了卡牌: {cardData.cardName} (当前为普通浏览模式)");
                break;
        }
    }

    /// <summary>
    /// 装备、替换——向角色卡身上加装一张装备卡
    /// </summary>
    /// <param name="equipCard"></param>
    public void Equip(CardData equipCard, CardData roleCard)
    {
        RoleCardData role = roleCard as RoleCardData;
        EquipCardData equip = equipCard as EquipCardData;

        role.Equip(equip);
    }

    /// <summary>
    /// 卸下装备——从角色卡身上卸下装备卡
    /// </summary>
    /// <param name="equipCard"></param>
    /// <param name="roleCard"></param>
    public void UnEquip(CardData equipCard, CardData roleCard)
    {
        RoleCardData role = roleCard as RoleCardData;
        EquipCardData equip = equipCard as EquipCardData;

        role.UnEquip(equip);
    }

    #endregion

    #region 合成

    /// <summary>
    /// 
    /// </summary>
    /// <param name="equip">目标装备</param>
    /// <param name="craftMaterials">消耗材料</param>
    public void Craft(EquipCardData equip, List<CardData> craftMaterials)
    {
        
    }

    #endregion
}
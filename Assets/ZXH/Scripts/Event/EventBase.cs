using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class EventBase : MonoBehaviour
{
    [Header("辅助字段")]
    [SerializeField] protected int currentDay = 0; // 被激活的持续时间
    [SerializeField] public EventData eventData; // 用于存储当前事件数据
    [SerializeField] public bool isEventActive = false; // 是否有事件在进行中
    [SerializeField] public bool isBock;//是否锁住
    [SerializeField] public bool isTime; // 是否到了可执行的时间
    [SerializeField] public EventTriggerType triggerType; // 触发类型
    [SerializeField] protected bool IsRepeatable; //是否可重复触发
    [SerializeField] protected List<EventTriggerConditionBase> Conditions { get; set; } // 事件触发条件列表

    [Header("拖拽引用")]
    [SerializeField] protected GameObject One;
    [SerializeField] protected GameObject Two;
    [SerializeField] protected GameObject Three;
    [SerializeField] protected CanvasGroup mainCanvasGroup;

    [Header("One静态事件UI_拖拽")]
    [SerializeField] protected TextMeshProUGUI EventName; // 事件名称
    [SerializeField] public TextMeshProUGUI EventTime; // 事件时间

    // 事件UI的引用——每个UI都是和模板里的预制体名字相同的，我们直接通过名字获取组件引用即可，不然每个UI都要单独写一个脚本来获取组件引用，太麻烦了
    [Header("Two静态事件UI")]
    [SerializeField] protected TextMeshProUGUI Story;
    [SerializeField] protected TextMeshProUGUI Tips;
    [SerializeField] protected TextMeshProUGUI DurationDays;// 事件持续天数
    [SerializeField] protected Button saveButton; // 保存按钮
    [SerializeField] protected float successProbability = 1f;//成功概率
    [SerializeField] protected int numberOfSuccesses = 0;//成功次数
    [SerializeField] protected bool isSuccess_Dice; // 是否成功骰子
    [SerializeField] protected bool isSuccess_Event; // 是否成功事件

    [Header("Two动态生成引用")]
    [SerializeField] protected CardSlot[] CardSlots; // 卡牌槽数组，用于获取属性值——通过CardSlot来拿到卡牌槽
    [Tooltip("用于显示属性需求的'列表项'预制体")]
    [SerializeField] protected GameObject attributeRequirementItemPrefab;//属性需求项预制体，用于动态生成属性需求列表
    [Tooltip("用于放置属性需求列表项的容器")]
    [SerializeField] protected Transform attributesContainer; //场景直接拖入
    [SerializeField] protected List<GameObject> spawnedAttributeItems = new List<GameObject>(); // 存储当前生成的列表项——属性值

    [Header("Three静态事件UI_拖拽")]
    [SerializeField] protected TextMeshProUGUI Result_Story; // 事件结果文案
    [SerializeField] protected TextMeshProUGUI Reward_Card; // 事件奖励——卡牌——直接把奖励的卡牌名字打印出来，卡牌直接加入到手牌库里
    [SerializeField] protected TextMeshProUGUI Result_Dice; // 骰子的结果
    [SerializeField] protected TextMeshProUGUI Name; // 事件名字

    [Header("后续事件")]
    [SerializeField] protected string SuccessEvent; // 成功的后续事件ID
    [SerializeField] protected string FailedEvent; // 失败的后续事件ID

    protected virtual void Awake()
    {
        // 获取所有需要的组件
        Story = FindTMPDeep("Story");
        Tips = FindTMPDeep("Tips");
        DurationDays = FindTMPDeep("DurationDays");
        saveButton = FindButtonDeep("Right").GetComponent<Button>();

        CardSlots = transform.GetComponentsInChildren<CardSlot>(true);

        //遵循“创建者负责注册”的原则——防止顺序问题造成的事件管理器未初始化
        //CharacterEventManager.Instance?.RegisterEvent(this);
    }


    protected virtual void Start()
    {
        AddTime(); //第一天
    }

    #region 辅助
    /// <summary>
    /// 进行倒计时——天数增加时调用
    /// </summary>
    public void AddTime()
    {
        if (eventData.DurationDays <= currentDay && !isEventActive)
        {
            isTime = true; // 到期了
            ExecutionEvent(eventData); // 执行事件逻辑
        }

        EventTime.text = (eventData.DurationDays - currentDay).ToString();
        DurationDays.text = $"剩余: {eventData.DurationDays - currentDay} 天"; // 更新UI显示剩余天数
        currentDay++;//把当前天数加1放在判断的后面，防止事件在第一天就结束了——游戏一开始就会天数加1
    }

    /// <summary>
    /// 使用事件数据来初始化和填充整个UI面板
    /// </summary>
    public virtual void Initialize(EventData eventData)
    {
        this.eventData = eventData;
        triggerType = eventData.triggerType; 
        IsRepeatable = eventData.IsRepeatable;
        Conditions = eventData.Conditions;

        //One
        EventName.text = eventData.EventName;
        EventTime.text = eventData.DurationDays.ToString();

        //Two
        // 1. 填充静态数据
        Story.text = eventData.Story;
        Tips.text = eventData.Tips;
        DurationDays.text = $"持续时间: {eventData.DurationDays} 天";
        //SuccessThreshold.text = $"成功阈值: {eventData.SuccessThreshold}";

        // 2. 清理上一次生成的动态列表
        foreach (var item in spawnedAttributeItems)
        {
            Destroy(item);
        }
        spawnedAttributeItems.Clear();

        // 3. 动态生成属性需求列表
        foreach (string attributeName in eventData.RequiredAttributes)
        {
            Debug.Log($"Event_Item:添加属性{attributeName}");
            // 实例化列表项预制体
            GameObject itemInstance = Instantiate(attributeRequirementItemPrefab, attributesContainer);

            // 获取子预制体上的文本组件
            // 这里我们假设子预制体上的组件也叫 "AttributeNameText" 和 "AttributeValueText"
            TextMeshProUGUI nameText = itemInstance.transform.Find("AttributeNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI valueText = itemInstance.transform.Find("AttributeValueText").GetComponent<TextMeshProUGUI>();

            // 填充数据
            nameText.text = attributeName;

            // 卡牌槽当前的总属性值

            valueText.text = "0";

            // 如果玩家属性不足，可以改变数值颜色以作提示
            // if (playerValue < some_threshold) valueText.color = Color.red;

            // 将生成的实例添加到列表中，以便下次清理
            spawnedAttributeItems.Add(itemInstance);


            //Three
            Name.text = eventData.EventName;
            SuccessEvent = eventData.SuccessEvent; // 成功后续事件ID
            FailedEvent = eventData.FailedEvent; // 失败后续事件ID
            //剩下的要投完骰子才能用

        }

        //更新事件属性需求列表的值
        UpdateAttributeRequirementValues();
    }

    /// <summary>
    /// 结束事件
    /// </summary>
    public virtual void CloseEvent()
    {
        // 关闭所有面板
        CloseAllPanels();

        // 销毁事件UI对象
        Destroy(this.gameObject);
    }

    #endregion

    #region One


    #endregion

    #region Two

    /// <summary>
    /// 深度查找子物体中的 TextMeshProUGUI 组件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private TextMeshProUGUI FindTMPDeep(string name)
    {
        var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmps)
        {
            if (tmp.name == name)
                return tmp;
        }
        Debug.LogError($"Event_Item: 未找到名为 {name} 的 TextMeshProUGUI 组件！");
        return null;
    }

    /// <summary>
    /// 深度查找子物体中的 TextMeshProUGUI 组件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Button FindButtonDeep(string name)
    {
        var tmps = GetComponentsInChildren<Button>(true);
        foreach (var tmp in tmps)
        {
            if (tmp.name == name)
                return tmp;
        }
        Debug.LogError($"Event_Item: 未找到名为 {name} 的 Button 组件！");
        return null;
    }

    /// <summary>
    /// right按钮点击
    /// </summary>
    protected virtual void SetRight()
    {
        LockingEvent(); // 锁住事件面板
    }

    /// <summary>
    /// 锁定事件
    /// </summary>
    protected virtual void LockingEvent()
    {
        isBock = true; // 锁住事件面板，防止重复操作
        Debug.Log("确认选择，正在锁定事件卡槽...");

        // 遍历所有指定的卡槽
        foreach (CardSlot slot in CardSlots)
        {
            if (slot != null)
            {
                // 调用我们刚刚在CardSlot中创建的方法来禁用它
                slot.SetInteractable(false);
            }
        }

        if(saveButton != null)
        {
            saveButton.interactable = false; // 禁用保存按钮
        }
    }

    /// <summary>
    /// 更新属性需求项的属性值显示——每次属性变化时调用
    /// </summary>
    public virtual void UpdateAttributeRequirementValues()
    {
        // 实时刷新每个属性需求项的属性总值
        foreach (var item in spawnedAttributeItems)
        {
            var nameText = item.transform.Find("AttributeNameText").GetComponent<TextMeshProUGUI>();
            var valueText = item.transform.Find("AttributeValueText").GetComponent<TextMeshProUGUI>();
            if (nameText != null && valueText != null)
            {
                string attrName = nameText.text;

                valueText.text = Character.Instance.GetAttribute(attrName).ToString();

            }
        }
    }

    #endregion

    #region Three

    /// <summary>
    /// 执行事件逻辑，根据事件数据和成功概率来决定事件结果
    /// </summary>
    /// <param name="eventData"></param>
    protected virtual void ExecutionEvent(EventData eventData)
    {
        if (!isBock) return; // 如果没有锁定事件,表示玩家还没有确认选择就算到期也不会执行事件逻辑

        isEventActive = true;

        if (RollTheDice_CharacterStat(eventData, successProbability))
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑

            isSuccess_Event = true; // 设置事件成功标志
            GiveRewards_CharacterStat(eventData); // 发放奖励
        }
        else
        {
            // 失败逻辑
            isSuccess_Event = false; // 设置事件失败标志
            Result_Story.text = eventData.FailedResults;
            Result_Dice.text = $"成功骰子的个数：{numberOfSuccesses}";
            Reward_Card.text = "没有奖励";
        }

        // 展开Three面板
        ExpandThree();

        //消耗掉所有卡槽中的卡牌
        foreach (var cardSlot in CardSlots)
        {
            var card = cardSlot.GetComponentInChildren<Card>();
            if (card != null)
            {
                Inventory_ZXH.Instance.Backpack.RemoveCard(card.cardData);
            }

        }
    }

    /// <summary>
    /// 奖励——加入库存中
    /// </summary>
    protected virtual void GiveRewards_CharacterStat(EventData eventData)
    {
        if (eventData == null || eventData.RewardItemIDs == null || eventData.RewardItemIDs.Count == 0)
            return;

        foreach (string rewardName in eventData.RewardItemIDs)
        {
            var addedItemInfo = Character.Instance.inventory.AddItem(rewardName, 1);

            if (addedItemInfo.ItemStack != null)
            {
                Debug.Log($"成功将 {addedItemInfo.ItemStack.Amount} 个 '{rewardName}' 添加到了库存中");
            }
            else
            {
                Debug.LogWarning($"添加物品 '{rewardName}' 失败! 请检查物品名称是否正确，以及背包是否有足够空间");
            }
        }
    }


    /// <summary>
    /// 掷骰子，返回是否成功,successProbability=0.5表示50%概率成功
    /// </summary>
    protected virtual bool RollTheDice_CharacterStat(EventData eventData, float successProbability)
    {
        Debug.Log($"Event_Item: 掷骰子，成功概率为 {successProbability * 100}%");
        int diceSum = GetAllDiceCount(eventData);//骰子个数
        int threshold = eventData.SuccessThreshold;//成功阈值

        numberOfSuccesses = 0;//成功次数

        successProbability = Mathf.Clamp01(successProbability);

        for (int i = 1; i <= diceSum; i++)
        {
            // 掷骰子
            float rand = Random.value; // [0,1)
            bool isSuccess = rand < successProbability;
            if (isSuccess)
            {
                numberOfSuccesses++;
            }
        }

        if (numberOfSuccesses >= threshold)
        {
            isSuccess_Dice = true;
        }
        else
        {
            isSuccess_Dice = false;
        }

        return isSuccess_Dice;
    }

    /// <summary>
    /// 获得需要的所有属性的总和——可以用来产生骰子的个数
    /// </summary>
    /// <param name="eventData">事件数据</param>
    /// <returns></returns>
    protected int GetAllDiceCount(EventData eventData)
    {
        int sum = 0;
        foreach (var attribute in eventData.RequiredAttributes)
        {
            int attributeValue = Character.Instance.GetAttribute(attribute);
            sum += attributeValue;
        }
        return sum;
    }

    /// <summary>
    /// 按钮关闭事件——这里把玩家操作与事件管理分离了，当事件完成时就要被注销，而不是等玩家按下按钮
    /// </summary>
    public virtual void SetCloseEvent()
    {
        CloseEvent();
    }

    #endregion

    #region 三个面板部分

    // 展开One的方法
    public void ExpandOne()
    {
        CloseAllPanels();
        if (One != null)
        {
            One.SetActive(true);
        }
    }

    //关闭One的方法
    public void CloseOne()
    {
        if (One != null)
        {
            One.SetActive(false);
        }
    }


    // 展开Two的方法
    public void ExpandTwo()
    {
        CloseAllPanels();
        if (Two != null)
        {
            Two.SetActive(true);
        }
    }

    //关闭Two的方法
    public void CloseTwo()
    {
        if (Two != null)
        {
            Two.SetActive(false); ;
        }
    }


    // 展开Three的方法
    public void ExpandThree()
    {
        CloseAllPanels();
        if (Three != null)
        {
            Three.SetActive(true);
        }
    }

    //关闭Three的方法
    public void CloseThree()
    {
        if (Three != null)
        {
            Three.SetActive(false);
        }
    }

    //关闭所有面板的方法
    public void CloseAllPanels()
    {
        CloseOne();
        CloseTwo();
        CloseThree();
    }

    #endregion
}

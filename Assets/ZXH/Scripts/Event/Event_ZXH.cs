using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Event_ZXH : MonoBehaviour
{
    [Header("辅助字段")] 
    [SerializeField]private int currentDay = 1; // 倒计时，初始为1
    private EventData eventData; // 用于存储当前事件数据

    [Header("拖拽引用")]
    public GameObject One;
    public GameObject Two;
    public GameObject Three;

    [Header("One静态事件UI_拖拽")]
    [SerializeField] private TextMeshProUGUI EventName; // 事件名称
    [SerializeField] private TextMeshProUGUI EventTime; // 事件时间

    // 事件UI的引用——每个UI都是和模板里的预制体名字相同的，我们直接通过名字获取组件引用即可，不然每个UI都要单独写一个脚本来获取组件引用，太麻烦了
    [Header("Two静态事件UI")]
    [SerializeField] private TextMeshProUGUI Story;
    [SerializeField] private TextMeshProUGUI Tips;
    [SerializeField] private TextMeshProUGUI DurationDays;// 事件持续天数
    //[SerializeField] private TextMeshProUGUI RequiredAttributes;// 需要的属性列表——通过读取的数据填写——会有多个同名的，我们要按顺序存起来和下面的RequiredAttributes_Value对应起来
    //[SerializeField] private TextMeshProUGUI RequiredAttributes_Value;// 属性列表对应的属性值——读取卡牌槽的数值来填充
    [SerializeField] private TextMeshProUGUI SuccessThreshold;
    [SerializeField]private float successProbability = 1f;//成功概率
    [SerializeField]private int t= 0;//成功次数
    [SerializeField]private bool isSuccess;

    [Header("Two动态生成引用")]
    [SerializeField] private CardSlot[] CardSlots; // 卡牌槽数组，用于获取属性值——通过CardSlot来拿到卡牌槽
    [Tooltip("用于显示属性需求的'列表项'预制体")]
    [SerializeField] private GameObject attributeRequirementItemPrefab;//属性需求项预制体，用于动态生成属性需求列表
    [Tooltip("用于放置属性需求列表项的容器")]
    [SerializeField] private Transform attributesContainer; //场景直接拖入
    [SerializeField] private List<GameObject> spawnedAttributeItems = new List<GameObject>(); // 存储当前生成的列表项——属性值


    [Header("Three静态事件UI_拖拽")]
    [SerializeField] private TextMeshProUGUI Result_Story; // 事件结果文案
    [SerializeField] private TextMeshProUGUI Reward_Card; // 事件奖励——卡牌——直接把奖励的卡牌名字打印出来，卡牌直接加入到手牌库里
    [SerializeField] private TextMeshProUGUI Result_Dice; // 骰子的结果
    [SerializeField] private TextMeshProUGUI Name; // 事件名字


    private void Awake()
    {
        // 获取所有需要的组件
        Story = FindTMPDeep("Story");
        Tips = FindTMPDeep("Tips");
        DurationDays = FindTMPDeep("DurationDays");
        //SuccessThreshold = FindTMPDeep("SuccessThreshold");

        CardSlots = transform.GetComponentsInChildren<CardSlot>(); 
    }


    private void Update()
    {
        // 实时刷新每个属性需求项的属性总值
        foreach (var item in spawnedAttributeItems)
        {
            var nameText = item.transform.Find("AttributeNameText").GetComponent<TextMeshProUGUI>();
            var valueText = item.transform.Find("AttributeValueText").GetComponent<TextMeshProUGUI>();
            if (nameText != null && valueText != null)
            {
                string attrName = nameText.text;
                
                valueText.text = GetTotalAttributeValue(attrName).ToString();
                
            }
        }
    }

    #region 辅助
    /// <summary>
    /// 进行倒计时——天数增加时调用
    /// </summary>
    public void AddTime()
    {
        currentDay++;
        if(eventData.DurationDays <= currentDay)
        {
            ExecutionEvent(eventData); // 执行事件逻辑
        }
    }

    /// <summary>
    /// 使用事件数据来初始化和填充整个UI面板
    /// </summary>
    public void Initialize(EventData eventData)
    {
        this.eventData = eventData;

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
            Debug.Log($"Event_ZXH:添加属性{attributeName}");
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
            //剩下的要投完骰子才能用

        }
    }

    #endregion


    #region One


    #endregion


    #region Two

    /// <summary>
    /// 获取所有卡槽中指定属性的总和
    /// </summary>
    private int GetTotalAttributeValue(string attrName)
    {
        int total = 0;

        foreach (var slot in CardSlots)
        {
            var card = slot.GetComponentInChildren<Card>();
            if (card != null)
            {
                total += card.cardData.GetAttributeValue(attrName);
            }
        }
        return total;
    }


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
        Debug.LogError($"Event_ZXH: 未找到名为 {name} 的 TextMeshProUGUI 组件！");
        return null;
    }

    /// <summary>
    /// 获取所有属性valueText的值之和
    /// </summary>
    private int GetAllValueTextSum()
    {
        int sum = 0;
        foreach (var item in spawnedAttributeItems)
        {
            var valueText = item.transform.Find("AttributeValueText").GetComponent<TextMeshProUGUI>();
            if (valueText != null && int.TryParse(valueText.text, out int val))
            {
                sum += val;
            }
        }
        return sum;
    }

    #endregion

    #region Three

    /// <summary>
    /// 执行事件逻辑，根据事件数据和成功概率来决定事件结果
    /// </summary>
    /// <param name="eventData"></param>
    public void ExecutionEvent(EventData eventData)
    {
        if (RollTheDice(eventData,successProbability))
        {
            // 成功逻辑
            Result_Story.text = eventData.SuccessfulResults;
            Result_Dice.text = $"成功骰子的个数：{t}";
            Reward_Card.text = $"获得：{eventData.RewardItemIDs}"; // 这里可以替换为实际的奖励逻辑
            
            GiveRewards(eventData); // 发放奖励卡牌
        }
        else
        {
            // 失败逻辑
            Result_Story.text = eventData.FailedResults;
            Result_Dice.text = "成功骰子的个数：{t}";
            Reward_Card.text = "没有奖励";
        }
        // 展开Three面板
        ExpandThree();
    }

    /// <summary>
    /// 根据事件奖励ID列表，从CardManager.cardDatabase查找并实例化奖励卡牌到手牌
    /// </summary>
    public void GiveRewards(EventData eventData)
    {
        if (eventData == null || eventData.RewardItemIDs == null || eventData.RewardItemIDs.Count == 0)
            return;

        // 获取CardManager实例（假设场景中只有一个CardManager）
        CardManager cardManager = FindObjectOfType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("CardManager 未找到！");
            return;
        }

        foreach (string rewardName in eventData.RewardItemIDs)
        {
            // 查找cardDatabase中名字匹配的CardData
            CardData rewardCard = cardManager.cardDatabase.FirstOrDefault(cd => cd.cardName == rewardName);
            if (rewardCard != null)
            {
                cardManager.AddCard(rewardCard);
                Debug.Log($"奖励卡牌：{rewardCard.cardName} 已加入手牌");
            }
            else
            {
                Debug.LogWarning($"未在cardDatabase中找到名为 {rewardName} 的卡牌，无法发放奖励。");
            }
        }
    }

    /// <summary>
    /// 掷骰子，返回是否成功,successProbability=0.5表示50%概率成功
    /// </summary>
    public bool RollTheDice(EventData eventData, float successProbability)
    {
        Debug.Log($"Event_ZXH: 掷骰子，成功概率为 {successProbability * 100}%");
        int diceSum = GetAllValueTextSum();//骰子个数
        int threshold = eventData.SuccessThreshold;//成功阈值

        t = 0;//成功次数

        successProbability = Mathf.Clamp01(successProbability);

        for(int i = 1;i <=diceSum; i++)
        {
            // 掷骰子
            float rand = Random.value; // [0,1)
            bool isSuccess = rand < successProbability;
            if (isSuccess)
            {
                t++;
            }
        }

        if(t >= threshold)
        {
            isSuccess = true;
        }
        else
        {
            isSuccess = false;
        }

        return isSuccess;
    }
    #endregion

    #region 三个面板部分

    // 展开One的方法
    public void ExpandOne()
    {
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
        if (Two != null)
        {
            Two.SetActive(true);
            CloseOne();
        }
    }

    //关闭Two的方法
    public void CloseTwo()
    {
        if (Two != null)
        {
            Two.SetActive(false);
            ExpandOne();
        }
    }


    // 展开Three的方法
    public void ExpandThree()
    {
        if (Three != null)
        {
            Three.SetActive(true);
        }
        CloseTwo();
    }

    //关闭Three的方法
    public void CloseThree()
    {
        if (Three != null)
        {
            Three.SetActive(false);
        }
        ExpandOne();
    }

    #endregion



    // 一个简单的翻译函数
    private string TranslateAttributeName(string key)
    {
        switch (key.ToLower())
        {
            case "intelligence": return "智力";
            case "charm": return "魅力";
            case "strength": return "力量";
            default: return key;
        }
    }
}

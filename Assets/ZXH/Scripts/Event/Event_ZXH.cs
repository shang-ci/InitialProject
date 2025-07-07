using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Event_ZXH : MonoBehaviour
{
    [Header("拖拽引用")]
    public GameObject One;
    public GameObject Two;
    public GameObject Three;

    // 事件UI的引用——每个UI都是和模板里的预制体名字相同的，我们直接通过名字获取组件引用即可，不然每个UI都要单独写一个脚本来获取组件引用，太麻烦了
    [Header("静态事件UI")]
    [SerializeField] private TextMeshProUGUI Story;
    [SerializeField] private TextMeshProUGUI Tips;
    [SerializeField] private TextMeshProUGUI DurationDays;// 事件持续天数
    //[SerializeField] private TextMeshProUGUI RequiredAttributes;// 需要的属性列表——通过读取的数据填写——会有多个同名的，我们要按顺序存起来和下面的RequiredAttributes_Value对应起来
    //[SerializeField] private TextMeshProUGUI RequiredAttributes_Value;// 属性列表对应的属性值——读取卡牌槽的数值来填充
    [SerializeField] private TextMeshProUGUI SuccessThreshold;

    [Header("动态引用")]
    [SerializeField] private CardSlot[] CardSlots; // 卡牌槽数组，用于获取属性值——通过CardSlot来拿到卡牌槽
    [Tooltip("用于显示属性需求的'列表项'预制体")]
    [SerializeField] private GameObject attributeRequirementItemPrefab;//属性需求项预制体，用于动态生成属性需求列表
    [Tooltip("用于放置属性需求列表项的容器")]
    [SerializeField] private Transform attributesContainer; //场景直接拖入

    // 存储当前生成的列表项——属性值
    [SerializeField] private List<GameObject> spawnedAttributeItems = new List<GameObject>();



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

    /// <summary>
    /// 使用事件数据来初始化和填充整个UI面板
    /// </summary>
    public void Initialize(EventData eventData)
    {
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
        }
    }

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
    }

    //关闭Three的方法
    public void CloseThree()
    {
        if (Three != null)
        {
            Three.SetActive(false);
        }
    }

    #endregion


    /// <summary>
    /// 获取所有卡槽中指定属性的总和
    /// </summary>
    private int GetTotalAttributeValue(string attrName)
    {
        int total = 0;
        foreach (var slot in CardSlots)
        {
            if (slot.child != null && slot.child.cardData != null)
            {
                total += slot.child.cardData.GetAttributeValue(attrName);
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

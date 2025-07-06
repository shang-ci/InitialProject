using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Event_ZXH : MonoBehaviour
{
    // 事件UI的引用——每个UI都是和模板里的预制体名字相同的，我们直接通过名字获取组件引用即可，不然每个UI都要单独写一个脚本来获取组件引用，太麻烦了
    [Header("静态事件UI")]
    private TextMeshProUGUI Story;
    private TextMeshProUGUI Tips;
    private TextMeshProUGUI DurationDays;// 事件持续天数
    private TextMeshProUGUI RequiredAttributes;// 需要的属性列表——通过读取的数据填写——会有多个同名的，我们要按顺序存起来和下面的RequiredAttributes_Value对应起来
    private TextMeshProUGUI RequiredAttributes_Value;// 属性列表对应的属性值——读取卡牌槽的数值来填充
    private TextMeshProUGUI SuccessThreshold;

    [Header("动态引用")]
    private CardSlot[] CardSlots; // 卡牌槽数组，用于获取属性值——通过CardSlot来拿到卡牌槽
    [Tooltip("用于显示属性需求的'列表项'预制体")]
    [SerializeField] private GameObject attributeRequirementItemPrefab;//属性需求项预制体，用于动态生成属性需求列表
    [Tooltip("用于放置属性需求列表项的容器")]
    [SerializeField] private Transform attributesContainer; //场景直接拖入

    // 存储当前生成的列表项
    private List<GameObject> spawnedAttributeItems = new List<GameObject>();



    private void Awake()
    {
        // 获取所有需要的组件
        Story = transform.Find("Story").GetComponent<TextMeshProUGUI>();
        Tips = transform.Find("Tips").GetComponent<TextMeshProUGUI>();
        DurationDays = transform.Find("DurationDays").GetComponent<TextMeshProUGUI>();
        //RequiredAttributes = transform.Find("RequiredAttributes").GetComponent<TextMeshProUGUI>();
        //RequiredAttributes_Value = transform.Find("RequiredAttributes_Value").GetComponent<TextMeshProUGUI>();
        SuccessThreshold = transform.Find("SuccessThreshold").GetComponent<TextMeshProUGUI>();

        CardSlots = transform.GetComponentsInChildren<CardSlot>(); 
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
        SuccessThreshold.text = $"成功阈值: {eventData.SuccessThreshold}";

        // 2. 清理上一次生成的动态列表
        foreach (var item in spawnedAttributeItems)
        {
            Destroy(item);
        }
        spawnedAttributeItems.Clear();

        // 3. 动态生成属性需求列表
        foreach (string attributeName in eventData.RequiredAttributes)
        {
            // 实例化列表项预制体
            GameObject itemInstance = Instantiate(attributeRequirementItemPrefab, attributesContainer);

            // 获取子预制体上的文本组件
            // 这里我们假设子预制体上的组件也叫 "AttributeNameText" 和 "AttributeValueText"
            TextMeshProUGUI nameText = itemInstance.transform.Find("AttributeNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI valueText = itemInstance.transform.Find("AttributeValueText").GetComponent<TextMeshProUGUI>();

            // 填充数据
            nameText.text = attributeName; 

            // 卡牌槽当前的总属性值
            
            //valueText.text = playerValue.ToString();

            // 如果玩家属性不足，可以改变数值颜色以作提示
            // if (playerValue < some_threshold) valueText.color = Color.red;

            // 将生成的实例添加到列表中，以便下次清理
            spawnedAttributeItems.Add(itemInstance);
        }
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

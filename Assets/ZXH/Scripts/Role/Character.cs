using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Equipping;
using Opsive.Shared.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CharacterAttributes
{
    public int basePhysique, currentPhysique;
    public int baseSocial, currentSocial;
    public int baseSurvival, currentSurvival;
    public int baseIntelligence, currentIntelligence;
    public int baseCharm, currentCharm;
    public int baseCombat, currentCombat;
}

public class Character : MonoBehaviour
{
    public static Character Instance { get; private set; }

    [Header("角色基础属性")]
    [SerializeField]protected string characterName;
    [SerializeField]protected CharacterAttributes attributes;
    [SerializeField]protected int currentHealth = 30;
    protected int maxHealth = 100;

    [Header("加装备")]
    protected IEquipper m_Equipper;
    public Inventory inventory;
    //private Dictionary<string, ItemDefinition> m_DefinitionCache;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inventory = GetComponent<Inventory>();
        m_Equipper = GetComponent<IEquipper>();

        InitCharacterStat();
        //m_DefinitionCache = new Dictionary<string, ItemDefinition>();
    }

    private void OnEnable()
    {
        Opsive.Shared.Events.EventHandler.RegisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    private void OnDisable()
    {
        Opsive.Shared.Events.EventHandler.UnregisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    /// <summary>
    /// 初始化当前属性为基础属性
    /// </summary>
    private void InitCharacterStat()
    {
        attributes.currentPhysique = attributes.basePhysique;
        attributes.currentSocial = attributes.baseSocial;
        attributes.currentSurvival = attributes.baseSurvival;
        attributes.currentIntelligence = attributes.baseIntelligence;
        attributes.currentCharm = attributes.baseCharm;
        attributes.currentCombat = attributes.baseCombat;
    }

    /// <summary>
    /// 当装备发生变化时调用
    /// </summary>
    /// <param name="equipper"></param>
    private void OnEquipmentChanged()
    {
        UpdateAllAttributes();
    }

    /// <summary>
    /// 重新计算所有属性：基础 + 装备加成
    /// </summary>
    public void UpdateAllAttributes()
    {
        // 从装备中获取属性加成
        attributes.currentPhysique = attributes.basePhysique + m_Equipper.GetEquipmentStatInt("physique");
        attributes.currentSocial = attributes.baseSocial + m_Equipper.GetEquipmentStatInt("social");
        attributes.currentSurvival = attributes.baseSurvival + m_Equipper.GetEquipmentStatInt("survival");
        attributes.currentIntelligence = attributes.baseIntelligence + m_Equipper.GetEquipmentStatInt("intelligence");
        attributes.currentCharm = attributes.baseCharm + m_Equipper.GetEquipmentStatInt("charm");
        attributes.currentCombat = attributes.baseCombat + m_Equipper.GetEquipmentStatInt("combat");
   
        // 可在此同步 UI等
        Debug.Log($"Updated Attributes: P{attributes.currentPhysique}, S{attributes.currentSocial}, " +
                  $"Sur{attributes.currentSurvival}, Int{attributes.currentIntelligence}, " +
                  $"C{attributes.currentCharm}, Combat{attributes.currentCombat}");

        // 这里可以添加事件通知系统，通知其他系统属性已更新
        foreach( var baseEvent in CharacterEventManager.Instance?.activeEvents)
        {
            baseEvent.UpdateAttributeRequirementValues();
        }
    }


    [ContextMenu("加入2张Armor到背包")]
    public void AddToInventory()
    {
        
        inventory.AddItem("Armor",2);
    }

    #region action
    public void Heal(int amount)
    {
        if(currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    #endregion

    #region 属性操作——装备属于加成的，剧情的属性属于基础的

    public int GetAttribute(string attrName)
    {
        return attrName switch
        {
            "physique" => attributes.currentPhysique,
            "social" => attributes.currentSocial,
            "survival" => attributes.currentSurvival,
            "intelligence" => attributes.currentIntelligence,
            "charm" => attributes.currentCharm,
            "combat" => attributes.currentCombat,
            _ => 0
        };
    }

    public void SetBaseAttribute(string attrName, int value)
    {
        switch (attrName)
        {
            case "physique": attributes.basePhysique = value; break;
            case "social": attributes.baseSocial = value; break;
            case "survival": attributes.baseSurvival = value; break;
            case "intelligence": attributes.baseIntelligence = value; break;
            case "charm": attributes.baseCharm = value; break;
            case "combat": attributes.baseCombat = value; break;
        }
        UpdateAllAttributes();
    }


    /// <summary>
    /// 为一个基础属性增加指定的值
    /// </summary>
    /// <param name="attrName">属性名称</param>
    /// <param name="valueToAdd">要增加的值</param>
    public void AddToBaseAttribute(string attrName, int valueToAdd)
    {
        // 这个函数会先获取当前基础值，加上增量，然后再设置回去
        // 这样可以复用你已经写好的SetBaseAttribute逻辑，非常方便
        switch (attrName)
        {
            case "physique": SetBaseAttribute("physique", attributes.basePhysique + valueToAdd); break;
            case "social": SetBaseAttribute("social", attributes.baseSocial + valueToAdd); break;
            case "survival": SetBaseAttribute("survival", attributes.baseSurvival + valueToAdd); break;
            case "intelligence": SetBaseAttribute("intelligence", attributes.baseIntelligence + valueToAdd); break;
            case "charm": SetBaseAttribute("charm", attributes.baseCharm + valueToAdd); break;
            case "combat": SetBaseAttribute("combat", attributes.baseCombat + valueToAdd); break;
        }
        Debug.Log($"属性 {attrName} 的基础值增加了 {valueToAdd}!");
    }

    #endregion

    #region 装备相关
    /// <summary>
    /// 判断角色是否装备了指定物品（通过物品定义名称）
    /// </summary>
    /// <param name="itemDefinitionName">物品定义名称</param>
    /// <returns>是否已装备该物品</returns>
    public bool IsEquippedByDefinitionName(string itemDefinitionName)
    {
        if (string.IsNullOrEmpty(itemDefinitionName))
        {
            Debug.LogWarning("物品定义名称为空！");
            return false;
        }

        if (m_Equipper == null)
        {
            Debug.LogWarning("角色未绑定 IEquipper 组件，无法判断装备。");
            return false;
        }

        var itemDef = InventorySystemManager.GetItemDefinition(itemDefinitionName);
        if (itemDef == null)
        {
            Debug.LogError($"未找到物品定义: {itemDefinitionName}");
            return false;
        }

        // 遍历所有装备槽
        // 推荐用 GetEquippedItem(index) 或 GetEquippedItems()，具体看你的 Equipper 实现
        int slotCount = 3; // 可根据实际装备槽数量调整
        for (int i = 0; i < slotCount; i++)
        {
            var equippedItem = m_Equipper.GetEquippedItem(i);
            if (equippedItem != null && equippedItem.ItemDefinition == itemDef)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断角色装备是否拥有列表中的所有装备
    /// </summary>
    /// <param name="itemDefinitionNames"></param>
    /// <returns></returns>
    public bool HasAllEquipByDefinitionNames(List<string> itemDefinitionNames)
    {
        if (itemDefinitionNames == null || itemDefinitionNames.Count == 0)
            return false;

        foreach (var name in itemDefinitionNames)
        {
            if (!IsEquippedByDefinitionName(name))
                return false;
        }
        return true;
    }
    #endregion


}

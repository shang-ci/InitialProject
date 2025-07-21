using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Equipping;
using Opsive.Shared.Events;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("角色基础属性")]
    protected string characterName;
    [SerializeField]protected int currentHealth = 30;
    protected int maxHealth = 100;
    protected int baseDamage;
    [SerializeField]protected int currentDamage;
    protected int armor;

    [Header("加装备")]
    protected IEquipper m_Equipper;

    public Inventory inventory;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        m_Equipper = GetComponent<IEquipper>();
    }

    private void OnEnable()
    {
        Opsive.Shared.Events.EventHandler.RegisterEvent<IEquipper>(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    private void OnDisable()
    {
        Opsive.Shared.Events.EventHandler.UnregisterEvent<IEquipper>(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    private void OnEquipmentChanged(IEquipper equipper)
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        float bounsCombat = m_Equipper.GetEquipmentStatInt("combat");
        currentDamage = baseDamage + Mathf.RoundToInt(bounsCombat);
        Debug.Log($"当前伤害 = {currentDamage}");
    }


    [ContextMenu("加入2张Armor到背包")]
    public void AddToInventory()
    {
        
        inventory.AddItem("Armor",2);
    }

    public void Heal(int amount)
    {
        if(currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}

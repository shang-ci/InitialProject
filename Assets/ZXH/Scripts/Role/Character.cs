using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("角色基础属性")]
    protected string characterName;
    protected int currentHealth = 30;
    protected int maxHealth = 100;
    protected int damage;
    protected int armor;

    public Inventory inventory;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
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

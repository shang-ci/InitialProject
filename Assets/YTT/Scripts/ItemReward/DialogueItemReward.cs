/*
7.17
Dialogue System中可通过Lua调用给玩家添加道具的方法
直接奖励道具放入inventory
*/
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.Events;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Core;

public class DialogueItemReward : MonoBehaviour
{
    [Tooltip("玩家的 Inventory_ZXH 组件")]
    public Inventory_ZXH playerInventory;

    void Start()
    {
        // 注册 Lua 函数，让 Dialogue System 可以调用
        Lua.RegisterFunction("GiveItemToPlayer", this, SymbolExtensions.GetMethodInfo(() => GiveItemToPlayer(string.Empty)));
    }

    // Dialogue System 通过 Lua 调用此方法
    public void GiveItemToPlayer(string stringName)
    {
        var itemDefinition = InventorySystemManager.GetItemDefinition(stringName);
        if (itemDefinition == null)
        {
            Debug.LogWarning($"未找到名为 {stringName} 的道具定义");
            return;
        }

        var item = InventorySystemManager.CreateItem(itemDefinition);
        //playerInventory.AddItem(item);
        Debug.Log($"已将道具 {stringName} 添加至玩家背包");
    }
}

using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.Core.AttributeSystem;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using System.Collections.Generic;

public class InventoryToDialogueSync : MonoBehaviour
{
    [Tooltip("引用玩家的Inventory组件")]
    public Inventory playerInventory;

    private HashSet<string> cachedAttributeNames = new HashSet<string>();

    private void OnEnable()
    {
        if (playerInventory == null)
        {
            Debug.LogError("InventoryToDialogueSync: 未绑定playerInventory！");
            return;
        }
        Opsive.Shared.Events.EventHandler.RegisterEvent(playerInventory, "OnInventoryUpdate", OnInventoryChanged);
        SyncAllAttributesToDSU();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            Opsive.Shared.Events.EventHandler.UnregisterEvent(playerInventory, "OnInventoryUpdate", OnInventoryChanged);
        }
    }

    private void OnInventoryChanged()
    {
        SyncAllAttributesToDSU();
    }

    /// <summary>
    /// 自动收集所有物品的属性名，并同步所有属性到DSU
    /// </summary>
    private void SyncAllAttributesToDSU()
    {
        cachedAttributeNames.Clear();

        var allItemInfos = playerInventory?.AllItemInfos;
        if (allItemInfos == null) return;

        // 自动收集所有属性名
        foreach (var itemInfo in allItemInfos)
        {
            if (itemInfo.Item == null) continue;
            var attributes = itemInfo.Item.ItemAttributeCollection;
            if (attributes == null) continue;

            foreach (var attr in attributes)
            {
                if (attr == null) continue;
                cachedAttributeNames.Add(attr.Name);
            }
        }

        // 计算并同步所有属性
        foreach (var attrName in cachedAttributeNames)
        {
            float total = CalculateTotalAttribute(attrName, allItemInfos);
            DialogueLua.SetVariable(attrName, total);
            Debug.Log($"[InventoryToDialogueSync] 自动同步属性：{attrName} = {total}");
        }
    }

    /// <summary>
    /// 计算某属性在所有物品中的总和
    /// </summary>
    private float CalculateTotalAttribute(string attributeName, IReadOnlyList<ItemInfo> allItemInfos)
    {
        float total = 0;
        if (allItemInfos == null) return total;

        foreach (var itemInfo in allItemInfos)
        {
            if (itemInfo.Item == null) continue;

            if (itemInfo.Item.TryGetAttributeValue<float>(attributeName, out float fValue))
            {
                total += fValue * itemInfo.Amount;
            }
            else if (itemInfo.Item.TryGetAttributeValue<int>(attributeName, out int iValue))
            {
                total += iValue * itemInfo.Amount;
            }
        }
        return total;
    }
}



// using Opsive.UltimateInventorySystem.Core;
// using Opsive.UltimateInventorySystem.Core.InventoryCollections;
// using Opsive.UltimateInventorySystem.Core.DataStructures;
// using PixelCrushers.DialogueSystem;
// using UnityEngine;
// using System.Collections.Generic;

// public class InventoryToDialogueSync : MonoBehaviour
// {
//     [Tooltip("引用玩家的Inventory组件")]
//     public Inventory playerInventory;

//     [Tooltip("需要同步到DSU的属性名（区分大小写）")]
//     public List<string> attributeNames = new List<string> {  };

//     private void OnEnable()
//     {
//         if (playerInventory == null)
//         {
//             Debug.LogError("InventoryToDialogueSync: 未绑定playerInventory！");
//             return;
//         }
//         // 注册UIS的Inventory更新事件
//         Opsive.Shared.Events.EventHandler.RegisterEvent(playerInventory, "OnInventoryUpdate", OnInventoryChanged);
//         SyncAllAttributesToDSU();
//     }

//     private void OnDisable()
//     {
//         if (playerInventory != null)
//         {
//             Opsive.Shared.Events.EventHandler.UnregisterEvent(playerInventory, "OnInventoryUpdate", OnInventoryChanged);
//         }
//     }

//     private void OnInventoryChanged()
//     {
//         SyncAllAttributesToDSU();
//     }

//     /// <summary>
//     /// 计算所有属性总和并同步到DSU
//     /// </summary>
//     private void SyncAllAttributesToDSU()
//     {
//         foreach (var attrName in attributeNames)
//         {
//             float total = CalculateTotalAttribute(attrName);
//             DialogueLua.SetVariable(attrName, total);
//             Debug.Log($"[InventoryToDialogueSync] 同步属性：{attrName} = {total}");
//         }
//     }

//     /// <summary>
//     /// 计算某属性在所有物品中的总和
//     /// </summary>
//     private float CalculateTotalAttribute(string attributeName)
//     {
//         float total = 0;
//         if (playerInventory == null) return total;

//         var allItemInfos = playerInventory.AllItemInfos;
//         if (allItemInfos == null) return total;

//         foreach (var itemInfo in allItemInfos)
//         {
//             if (itemInfo.Item == null) continue;

//             if (itemInfo.Item.TryGetAttributeValue<float>(attributeName, out float fValue))
//             {
//                 total += fValue * itemInfo.Amount;
//             }
//             else if (itemInfo.Item.TryGetAttributeValue<int>(attributeName, out int iValue))
//             {
//                 total += iValue * itemInfo.Amount;
//             }
//         }
//         return total;
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Opsive.UltimateInventorySystem.ItemActions;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using System;
using Opsive.Shared.Game;
using Opsive.UltimateInventorySystem.Core.AttributeSystem;

[Serializable]
public class HealAction : ItemAction
{
    [SerializeField] protected string m_AttributeName = "HealAmount";

    protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        var character = itemUser.gameObject.GetCachedComponent<Character>();

        if(character == null) return false;

        if(itemInfo.Item.GetAttribute(m_AttributeName) == null) return false;

        return true;
    }

    protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        var character = itemUser?.gameObject.GetCachedComponent<Character>();

        character.Heal(itemInfo.Item.GetAttribute<Attribute<int>>(m_AttributeName).GetValue());

        itemInfo.Inventory.RemoveItem(1,itemInfo);
    }
}

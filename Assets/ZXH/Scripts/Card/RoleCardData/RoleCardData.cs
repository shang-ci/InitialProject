using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New RoleCardData", menuName = "Card/Role Card Data")]
public class RoleCardData : CardData
{
    public List<EquipCardData> equipments = new List<EquipCardData>(); // 角色装备列表
    public Dictionary<EquipmentType, EquipCardData> equipmentDictionary = new Dictionary<EquipmentType, EquipCardData>() ;// 装备字典，快速查找


    private void OnEnable()
    {
        cardType = CardType.Role;
    }

    /// <summary>
    /// 添加\替换装备
    /// </summary>
    /// <param name="equip"></param>
    public void Equip(EquipCardData equip)
    {
        EquipCardData oldEquip = null;

        if(equipmentDictionary != null)
        {
            foreach (var equipment in equipmentDictionary)
            {
                if (equipment.Key == equip.equipmentType)
                {
                    oldEquip = equipment.Value;
                }
            }
        }

        // 将旧装备放回库存
        if (oldEquip != null)
        {
            Inventory_ZXH.Instance.Backpack.AddCard(oldEquip); 
            equipments.Remove(oldEquip);
            equipmentDictionary.Remove(oldEquip.equipmentType);

            // 减少属性
            RemoveAttributes(oldEquip.attributes);
        }

        // 添加新装备
        Inventory_ZXH.Instance.Backpack.RemoveCard(equip);//刷新过了
        equipments.Add(equip);
        equipmentDictionary[equip.equipmentType] = equip;

        //增加属性
        AddAttributes(equip.attributes);

    }

    /// <summary>
    /// 移除装备
    /// </summary>
    /// <param name="equip"></param>
    public void UnEquip(EquipCardData equip)
    {
        if (equipments.Contains(equip))
        {
            equipments.Remove(equip);
            equipmentDictionary.Remove(equip.equipmentType);

            // 将装备放回库存
            Inventory_ZXH.Instance.Backpack.AddCard(equip);

            // 减少属性
            RemoveAttributes(equip.attributes);
        }
    }

}
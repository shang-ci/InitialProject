using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备类型/部位
/// </summary>
public enum EquipmentType
{
    Weapon,//武器
    Armor,//盔甲
    Amlet,//护身符
}

/// <summary>
/// 装备卡牌数据类
/// </summary>
[CreateAssetMenu(fileName = "New EquipCardData", menuName = "Card/Equip Card Data")]
public class EquipCardData : CardData
{
    public EquipmentType equipmentType;

    public List<CardData> craftMaterials;// 制作材料——就只是单张卡就可以合成，不用3张金剑卡这样的


    private void Awake()
    {
        cardType = CardType.Equip; 
    }
}

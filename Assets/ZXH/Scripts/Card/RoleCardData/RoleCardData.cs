using UnityEngine;

[CreateAssetMenu(fileName = "New RoleCardData", menuName = "Card/Role Card Data")]
public class RoleCardData : CardData
{
    private void Awake()
    {
        cardType = CardType.Role; 
    }


}
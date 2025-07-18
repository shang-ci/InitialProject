using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("½ÇÉ«»ù´¡ÊôÐÔ")]
    protected string characterName;
    protected int currentHealth = 30;
    protected int maxHealth = 100;
    protected int damage;
    protected int armor;


    public void Heal(int amount)
    {
        if(currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}

/*
7.10
制作一个属性版
实时更新玩家的属性值
从dialogue system中实时获取各属性的数值
*/
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
using TMPro;

public class PlayerStatUI : MonoBehaviour
{
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI hardworkingText;
    public TextMeshProUGUI angerText;

    void Update()
    {
        int wisdom = DialogueLua.GetVariable("Wisdom").asInt;
        int hardworking = DialogueLua.GetVariable("Hardworking").asInt;
        int anger = DialogueLua.GetVariable("Anger").asInt;
        wisdomText.text = $"Wisdom: {wisdom}";
        hardworkingText.text = $"Hardworking: {hardworking}";
        angerText.text = $"Anger: {anger}";
    }
}
 using UnityEngine;
using PixelCrushers.DialogueSystem;

public class CustomDialogueUI : StandardDialogueUI
{
    public PortraitManager portraitManager;

    void Awake()
    {
        Debug.Log("CustomDialogueUI Awake - Component initialized");
    }

    public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
        Debug.Log($"CustomDialogueUI.ShowResponses called with {responses.Length} responses");
        
        base.ShowResponses(subtitle, responses, timeout);
        
        // 当Player回答选项出现时，显示Player的立绘
        if (portraitManager != null)
        {
            Debug.Log("Calling portraitManager.OnPlayerResponseMenuOpen");
            portraitManager.OnPlayerResponseMenuOpen(responses);
        }
        else
        {
            Debug.LogWarning("CustomDialogueUI: portraitManager is null!");
        }
    }
}
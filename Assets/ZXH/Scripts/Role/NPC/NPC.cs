using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NpcDefinition npcDefinition; // 与场景中 NPC 关联的 SO 数据
    public AffinityRule[] customAffinityRules; // NPC 特有的好感规则
}

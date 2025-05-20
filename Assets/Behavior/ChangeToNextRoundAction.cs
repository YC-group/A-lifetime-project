using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = System.Random;

/// <summary>
/// 切換到下個回合 - Jerry0401
/// </summary>
///

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Change to next round", story: "Change to next [round] via [GameManager]", category: "Action", id: "c094b19df103e16f6023c66f190b0ec0")]
public partial class ChangeToNextRoundAction : Action
{
    [SerializeReference] public BlackboardVariable<RoundState> Round;
    [SerializeReference] public BlackboardVariable<GameManager> GameManager;
    protected override Status OnStart()
    {
        if (Round == null)
        {
            return Status.Failure;
        }
        GameManager.Value.SetCurrentRound(Round);
        return Status.Success;
    }
    
}


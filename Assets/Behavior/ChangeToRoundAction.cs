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
[NodeDescription(name: "Change to round", story: "Change to [round] via [GameManager]", category: "Action", id: "c094b19df103e16f6023c66f190b0ec0")]
public partial class ChangeToRoundAction : Action
{
    [SerializeReference] public BlackboardVariable<RoundState> Round;
    [SerializeReference] public BlackboardVariable<GameObject> GameManager;
    protected override Status OnStart()
    {
        if (Round == null)
        {
            return Status.Failure;
        }
        GameManager.Value.GetComponent<GameManager>().SetCurrentRound(Round);
        return Status.Success;
    }
    
}


using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateRoundState", story: "Update [roundState] and assign [gameManager]", category: "Action", id: "6bdec673a3b78563226f4eba791ca95a")]
public partial class UpdateRoundStateAction : Action
{
    [SerializeReference] public BlackboardVariable<RoundState> RoundState;
    [SerializeReference] public BlackboardVariable<GameManager> GameManager;
    // protected override Status OnStart()
    // {
    //     return Status.Running;
    // }

    protected override Status OnUpdate()
    {
        RoundState.Value = GameManager.Value.GetCurrentRound();
        return RoundState == null ? Status.Failure : Status.Success;
    }

    // protected override void OnEnd()
    // {
    // }
}


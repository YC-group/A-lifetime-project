using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get enemy counts with GameManager", story: "Get [EnemyCounts] with [GameManager]", category: "Action", id: "6bfa3e605d35743dff47901196190c1d")]
public partial class GetEnemyCountsWithGameManagerAction : Action
{
    [SerializeReference] public BlackboardVariable<int> EnemyCounts;
    [SerializeReference] public BlackboardVariable<GameManager> GameManager;

    protected override Status OnStart()
    {
        EnemyCounts.Value = GameManager.Value.GetEnemyCounts();
        return Status.Success;
    }
    
}


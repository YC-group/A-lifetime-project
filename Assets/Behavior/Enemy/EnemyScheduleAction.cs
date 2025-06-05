using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy Schedule", story: "Enemy Start Action via [GameManager]", category: "Action", id: "49577187cb5262803cdae9b979b9dff6")]
public partial class EnemyScheduleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> GameManager;
    protected override Status OnStart()
    {
        EnemyActionScheduler enemyActionScheduler =
           GameManager.Value.GetComponent<EnemyActionScheduler>();
        enemyActionScheduler.EnemyMoveSchedule();
        enemyActionScheduler.EnemyNextPositions.Clear();
        return Status.Success;
    }

    // protected override Status OnUpdate()
    // {
    //     return Status.Success;
    // }
    //
    // protected override void OnEnd()
    // {
    // }
}


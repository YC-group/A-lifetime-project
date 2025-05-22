using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Add", story: "[Value] ++", category: "Action", id: "f527b9a7071a08acb14f1d1d40b66c73")]
public partial class AddAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Value;

    protected override Status OnStart()
    {
        Value.Value = Value.Value+=1;
        return Status.Success;
    }
}


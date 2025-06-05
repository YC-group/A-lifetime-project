using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyActionScheduler : MonoBehaviour
{
    public Dictionary<GameObject, Vector3> EnemyNextPositions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        EnemyNextPositions = new Dictionary<GameObject, Vector3>();
    }

    public void EnemyMoveSchedule()
    {
        Dictionary<GameObject, Vector3> EnemyNextPositionsTmp = new Dictionary<GameObject, Vector3>(EnemyNextPositions);
        foreach (var enp in EnemyNextPositionsTmp)
        {
            foreach (var tmp in EnemyNextPositionsTmp)
            {
                if (enp.Key != tmp.Key && enp.Value == tmp.Value)
                {
                    if (enp.Key.GetComponent<EnemyScript>().movePriority >
                        tmp.Key.GetComponent<EnemyScript>().movePriority)
                    {
                        EnemyNextPositions.Remove(tmp.Key);
                    }
                    else
                    {
                        EnemyNextPositions.Remove(enp.Key);
                        break;
                    }
                }
            }
        }

        foreach (var e in EnemyNextPositions)
        {
            Debug.Log(e.Value);   
            e.Key.GetComponent<NavMeshAgent>().SetPath(e.Key.GetComponent<EnemyScript>().GetPath());
        }
    }
}

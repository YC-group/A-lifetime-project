using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 顯示移動路徑 - Jerry0401
/// </summary>
///

[RequireComponent(typeof(NavMeshAgent))]
public class NavPathVisualizer : MonoBehaviour
{
    [SerializeField] bool IsPathVisualize;
    private NavMeshAgent agent;

    void Awake()
    {
        // IsPathVisualize = false;
        agent = GetComponent<NavMeshAgent>();
    }

    void OnDrawGizmos()
    {
        if (agent == null || agent.path == null || !IsPathVisualize)
            return;

        var path = agent.path;
        var corners = path.corners;
        // Debug.Log(path.corners.Length);
        if (agent.path.corners.Length < 2)
            return;
        
        // 顯示路徑
        Gizmos.color = Color.red;
        
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(path.corners[i], path.corners[i + 1]); 
        }
        
        // 顯示移動點
        Gizmos.color = Color.green;
        
        for (int i = 0; i < corners.Length - 1; i++) 
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.1f);
        }

        if (corners.Length > 0) // 如果沒有移動的時候
        {
            Gizmos.DrawSphere(corners[corners.Length - 1], 0.1f);
        }
    }
}
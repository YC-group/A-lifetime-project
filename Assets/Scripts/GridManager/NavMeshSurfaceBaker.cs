using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 自動烘焙場景 - Jerry0401
/// </summary>
///
public class NavMeshSurfaceBaker : MonoBehaviour
{
    public NavMeshSurface surface;
    [Header("Bake Settings")] public bool bakeOnStart = true;
    public bool clearOnBake = true;
    public string navMeshAgentType = "Player";
    public string navMeshLayerMask = "Level";

    private void Awake()
    {
        int? agentTypeID = GetNavMeshAgentID(navMeshAgentType); // 獲取 Agent List 的 ID
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                Debug.LogError(
                    "NavMeshSurfaceBaker: NavMeshSurface component not found on this GameObject or assigned in Inspector.");
                return;
            }

            surface.layerMask = LayerMask.GetMask(navMeshLayerMask); // 烘焙的遮罩
            surface.collectObjects = CollectObjects.All; // 選擇烘焙的模式
            if (agentTypeID != null)
            {
                surface.agentTypeID = agentTypeID.Value;
            }
        }
        // 自動烘焙
        if (bakeOnStart)
        {
            if (surface != null)
            {
                if (clearOnBake)
                {
                    surface.RemoveData();
                    Debug.Log("NavMeshSurfaceBaker: Existing NavMesh data cleared.");
                }
                
                surface.BuildNavMesh();
                Debug.Log("NavMeshSurfaceBaker: NavMesh bake completed.");
            }
            else
            {
                Debug.LogError("NavMeshSurfaceBaker: NavMesh isn't assigned, bake failed.");
            }
        }
    }
    private int? GetNavMeshAgentID(string name)
    {
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
            if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID))
            {
                return settings.agentTypeID;
            }
        }

        return null;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
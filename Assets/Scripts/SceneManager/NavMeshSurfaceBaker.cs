using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshSurfaceBaker : MonoBehaviour
{
    public NavMeshSurface surface;
    [Header("Bake Settings")]
    public bool bakeOnStart = true;
    public bool clearOnBake = true;
    public string navMeshAgentType = "Player";
    public string navMeshLayerMask = "Level";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int? agentTypeID = GetNavMeshAgentID(navMeshAgentType);
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                Debug.LogError("NavMeshSurfaceBaker: NavMeshSurface component not found on this GameObject or assigned in Inspector.");
                return;
            }
            surface.layerMask = LayerMask.GetMask(navMeshLayerMask);
            surface.collectObjects = CollectObjects.All;
            if (agentTypeID != null)
            {
                surface.agentTypeID = agentTypeID.Value;
            }
        }

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

    // Update is called once per frame
    void Update()
    {
        
    }
}

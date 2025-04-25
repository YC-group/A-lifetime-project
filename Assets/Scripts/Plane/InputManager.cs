using UnityEngine;

/// <summary>
/// 鼠標偵測腳本 - Jerry0401
/// </summary>
public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;
    
    private Vector3 lastPosition;
    
    [SerializeField] private LayerMask placementLayerMask;

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayerMask))
        {
            return hit.point;
        }
        return lastPosition;
    }
}

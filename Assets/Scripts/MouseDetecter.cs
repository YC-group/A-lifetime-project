using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDetecter : MonoBehaviour
{
    [SerializeField] private LayerMask gridLayer;

    private Vector3 hitPosition;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;
    private bool hitSomething = false; // 是否有擊中物體

    private void OnDrawGizmos()
    {
        if (hitSomething)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin, rayDirection * Vector3.Distance(rayOrigin, hitPosition)); // 只畫到碰撞點
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPosition, 0.1f); // 在擊中點畫一個紅色小圓球
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(rayOrigin, rayDirection * 100f); // 沒擊中時，畫一條長 100 單位的射線
        }
    }

    public Vector3 GetMouseRaycastPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, gridLayer))
        {
            hitPosition = hit.point;
            hitSomething = true; // 記錄擊中狀態
        }
        else
        {
            hitSomething = false; // 沒有擊中
        }

        rayOrigin = ray.origin;       // 記錄射線起點
        rayDirection = ray.direction; // 記錄射線方向

        return hitPosition;
    }

    private void Update()
    {
        GetMouseRaycastPosition();
    }
}

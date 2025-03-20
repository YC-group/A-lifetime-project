using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDetecter : MonoBehaviour
{
    [SerializeField] private LayerMask gridLayer;

    private Vector3 hitPosition;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;
    private bool hitSomething = false; // �O�_����������

    private void OnDrawGizmos()
    {
        if (hitSomething)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin, rayDirection * Vector3.Distance(rayOrigin, hitPosition)); // �u�e��I���I
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPosition, 0.1f); // �b�����I�e�@�Ӭ���p��y
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(rayOrigin, rayDirection * 100f); // �S�����ɡA�e�@���� 100 ��쪺�g�u
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
            hitSomething = true; // �O���������A
        }
        else
        {
            hitSomething = false; // �S������
        }

        rayOrigin = ray.origin;       // �O���g�u�_�I
        rayDirection = ray.direction; // �O���g�u��V

        return hitPosition;
    }

    private void Update()
    {
        GetMouseRaycastPosition();
    }
}

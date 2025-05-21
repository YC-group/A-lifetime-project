using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class EnemyScript : MonoBehaviour
{
    [SerializeField] public EnemyData enemySO;
    public Grid grid;
    private Vector2 moveVector;
    private Vector3Int currentCell;

    [Header("視野設定")]
    public float viewRadius = 6f;                // 偵測半徑
    [Range(0f, 360f)]
    public float viewAngle = 90f;                // 偵測角度
    public int rayCount = 45;                    // 射線數量（影響偵測精度）
    public float eyeHeight = 1.5f;               // 射線發射高度

    private Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);

        GameObject go = GameObject.FindWithTag("Player"); // 假設玩家是 Player tag
        if (go != null) player = go.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (DetectPlayer())
        {
            print("偵測到玩家");
        }
    }

    bool DetectPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("❌ 玩家尚未指定或找不到 Player Tag");
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // 取得玩家的水平位置（忽略高度差）
        Vector3 playerPos = new Vector3(player.position.x, origin.y, player.position.z);
        Vector3 toPlayer = playerPos - origin;
        float distanceToPlayer = toPlayer.magnitude;

        // 不在半徑內
        if (distanceToPlayer > viewRadius)
        {
            return false;
        }

        // 不在視角內
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle / 2f)
        {
            return false;
        }

        Ray ray = new Ray(origin, toPlayer.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distanceToPlayer);

        foreach (RaycastHit hit in hits)
        {
            Building building = hit.collider.GetComponent<Building>();
            if (building != null)
            {
                if (building.buildingSO.isVisionBlocking)
                {
                    return false;
                }
            }
        }

        return true;
    }

    //畫圖形
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // 畫出偵測範圍（圓形邊界）
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 淡綠色
        Gizmos.DrawWireSphere(origin, viewRadius);

        // 畫出扇形邊界（中心線 + 左右邊線）
        float halfAngle = viewAngle / 2f;

        // 中心線（藍色）
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + transform.forward * viewRadius);

        // 左邊界（黃色）
        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
        Gizmos.DrawLine(origin, origin + leftDir * viewRadius);

        // 右邊界（黃色）
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * transform.forward;
        Gizmos.DrawLine(origin, origin + rightDir * viewRadius);

        // 額外：扇形內部的射線（模擬視野解析度）
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // 淡黃色
        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Gizmos.DrawLine(origin, origin + dir * viewRadius);
        }
    }
}



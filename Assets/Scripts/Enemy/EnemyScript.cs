using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine.AI;
using Object = System.Object;

/// <summary>
/// 敵人腳本 - Jerry0401 / Mobias0315
/// </summary>
///
public enum MoveMode // 行為模式列舉
{
    UpLeftDownRight,
    DownRightUpLeft,
    Stay
}

public class EnemyScript : MonoBehaviour
{
    [SerializeField] public EnemyData enemySO;
    
    private Vector2 moveVector;
    private Vector3Int currentCell;
    private NavMeshPath path;
    private NavMeshAgent agent;

    [Header("狀態")] 
    public float hp; // 血量
    public bool isAlert; // 警戒狀態
    public bool isStun; // 擊暈狀態
    public int stunRound = 0; // 擊暈持續回合

    [Header("移動模式")] 
    public MoveMode moveMode;
    public int movePriority;
    public Vector3 targetPosition; // 移動到下個位置
    [Tooltip("Speed in NavMeshAgent , units / sec")]
    public float moveSpeed; // unit / second
    [Tooltip("Acceleration in NavMeshAgent , units^2 / sec")]
    public float moveAcceleration; // unit^2 / second
    [Tooltip("Angular speed in SmoothRotation , speed * deltaTime at each frame")]
    public float moveAngularSpeed; // speed * deltaTime

    [Header("視野高度")] 
    public float eyeHeight = 1.5f; // 射線發射高度

    [Header("視野設定(前)")] 
    public float viewRadiusForward = 6f; // 偵測半徑
    [Range(0f, 360f)] 
    public float viewAngleForward = 90f; // 偵測角度

    [Header("視野設定(側邊)")] 
    public float viewRadiusBack = 4f; // 偵測半徑
    [Range(0f, 360f)] 
    public float viewAngleBack = 270f; // 偵測角度

    [Header("視野設定(警戒)")] 
    public float viewRadiusAlert = 10f; // 偵測半徑
    [Range(0f, 360f)] 
    public float viewAngleAlert = 360f; // 偵測角度
    
    public Coroutine rotationCoroutine = null; 
    public List<ItemScript> pocketList; // 口袋

    void Start()
    {
        EnemyDataInitializer();
    }

    void Update()
    {
        // HACK: 暫時方案，敵人生成後無法偵測玩家
        if (PlayerScript.GetInstance() != null)
        {
            if (hp == 0)
            {
                ObjectPoolManager.ReturnObjectToPool(this.gameObject);
                GridManager.GetInstance().RemoveGameObjectFromMoveGrid(this.gameObject);
            }
        
            if (DetectPlayer())
            {
                DetectAlert();
                //Debug.Log("偵測到玩家");
            }
        
            if (GameManager.GetInstance().GetCurrentRound() == RoundState.EnemyTurn)
            {
                // 經過一回合恢復狀態
                if (GameManager.GetInstance().GetAfterRoundsCounts() >= stunRound + 3 && isStun)
                {
                    isStun = false;
                }
            }
        }
    }

    private void EnemyDataInitializer() // 初始化
    {
        
        hp = enemySO.hp;
        isAlert = false;
        isStun = false;
        targetPosition = transform.position;
        movePriority = enemySO.movePriority;
        // 設定移動網格
        currentCell = GridManager.GetInstance().moveGrid.WorldToCell(transform.position);
        transform.position = GridManager.GetInstance().moveGrid.GetCellCenterWorld(currentCell);
        // 設定移動速度與加速度
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.acceleration = moveAcceleration;
        agent.updateRotation = false; // 停用 NavMeshAgent 轉向方面的功能
        
        ObjectPoolManager.AddExistObjetToPool(this.gameObject);
    }

    public List<Vector3Int> SetMoveModeDirection(MoveMode mode) // 設定行為模式
    {
        switch (mode)
        {
            case MoveMode.DownRightUpLeft:
                return new List<Vector3Int>()
                    { Vector3Int.back, Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.zero };
            case MoveMode.UpLeftDownRight:
                return new List<Vector3Int>()
                    { Vector3Int.forward, Vector3Int.left, Vector3Int.back, Vector3Int.right, Vector3Int.zero };
            case MoveMode.Stay:
                return new List<Vector3Int>() { Vector3Int.zero };
            default:
                return new List<Vector3Int>()
                    { Vector3Int.back, Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.zero };
        }
    }
    
    // HACK: 平滑轉向動畫
    public IEnumerator SmoothRotation(Vector3Int direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction); // 轉向方向
        // Debug.Log("targetRotation : " + targetRotation);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 5f) // 轉向差值小於 5 就判定為轉向結束
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, moveAngularSpeed * Time.deltaTime);
            yield return null; // 結束這一 Frame 的 Coroutine
        }

        transform.rotation = targetRotation; // 確保精準面向
    }

    public bool DetectPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // 取得玩家的水平位置（忽略高度差）
        Vector3 playerPos = new Vector3(PlayerScript.GetInstance().transform.position.x, origin.y, PlayerScript.GetInstance().transform.position.z);
        Vector3 toPlayer = playerPos - origin;
        float distanceToPlayer = toPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, toPlayer);

        // 角度與距離判斷
        bool inForward = distanceToPlayer <= viewRadiusForward && angle <= viewAngleForward / 2f;
        bool inBack = distanceToPlayer <= viewRadiusBack && angle <= viewAngleBack / 2f;
        bool inAlert = distanceToPlayer <= viewRadiusAlert && angle <= viewAngleAlert / 2f;
        if (!isAlert && !(inForward || inBack))
        {
            return false;
        }
        else if (isAlert && !inAlert)
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
            Door door = hit.collider.GetComponent<Door>();
            if (door != null)
            {
                return false;
            }
        }

        // ✅ 畫出一條輔助線從敵人眼睛高度連到玩家位置
        Debug.DrawLine(origin, playerPos, Color.red);
        return true;
    }

    //畫圖形
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        // ✅ 畫三種模式下的視野
        if (isAlert)
        {
            DrawFOV(origin, viewRadiusAlert, viewAngleAlert, Color.red);
        }
        else
        {
            DrawFOV(origin, viewRadiusForward, viewAngleForward, Color.green);
            DrawFOV(origin, viewRadiusBack, viewAngleBack, Color.cyan);
        }
    }

    // ✅ 抽出一個方法來畫扇形視野（可以多種角度）
    private void DrawFOV(Vector3 origin, float radius, float angle, Color color)
    {
        int rayCount = 45;
        float halfAngle = angle / 2f;
        Gizmos.color = color;

        Vector3 centerDir = transform.forward;
        Gizmos.DrawLine(origin, origin + centerDir * radius);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * transform.forward;

        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);

        Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Gizmos.DrawLine(origin, origin + dir * radius);
        }
    }

    private void DetectAlert()
    {
        isAlert = true;
    }
    
    // TODO: 敵人暈眩示意
    // public void ShowEnemyIsStun()
    // {
    //     if (GameManager.Instance.SHOWENEMYSTUNSTATE)
    //     {
    //         
    //     }
    // }
}
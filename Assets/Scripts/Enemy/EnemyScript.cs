using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class EnemyScript : MonoBehaviour
{
    [SerializeField] public EnemyData enemyObj;
    public Grid grid;
    private Vector2 moveVector;
    private Vector3Int currentCell;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

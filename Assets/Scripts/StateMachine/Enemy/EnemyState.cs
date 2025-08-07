using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine enemyStateMachine;
    protected EnemyScript enemy;
    protected string animatorName;
    
    public EnemyState(EnemyScript enemy, EnemyStateMachine enemyStateMachine, string animBoolName)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
        this.animatorName = animBoolName;
    }
    
    public EnemyState(EnemyScript enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine; 
    }

    public virtual void Enter()
    {
        enemy.isMoving = false;
    }

    public virtual void Update()
    {
        
    }

    public virtual void Exit()
    {
        
    }
}

using UnityEngine;

public class HellWalkerEnemy : EnemyAIBase
{
    [Header("Hell Walker Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.8f;
    private float baseMoveSpeed;

    protected override void Start()
    {
        base.Start();
        baseMoveSpeed = moveSpeed;
        Debug.Log("Hell Walker telah bangkit!");
    }

    protected override void LogicChaseState()
    {
        moveSpeed = baseMoveSpeed * chaseSpeedMultiplier;
        base.LogicChaseState();        // Tetap panggil base
    }

    protected override void LogicPatrolState()
    {
        moveSpeed = baseMoveSpeed;
        base.LogicPatrolState();
    }
}
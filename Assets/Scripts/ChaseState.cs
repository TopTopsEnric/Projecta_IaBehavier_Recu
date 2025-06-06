using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase State", menuName = "AI/States/Chase State")]
public class ChaseState : EnemyState
{
    [Header("Chase Settings")]
    public float lostPlayerTimeout = 3f;
    public float updateDestinationRate = 0.5f;

    private float lostPlayerTimer;
    private float updateTimer;

    public override void OnEnter(EnemyAI enemy)
    {
        enemy.SetSpeed(moveSpeed);
        lostPlayerTimer = 0f;
        updateTimer = 0f;

        Debug.Log($"{enemy.name} entró en estado: {stateName}");
    }

    public override void OnUpdate(EnemyAI enemy)
    {
        Debug.Log($"persiguiendo");
        // Actualizar destino periódicamente
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateDestinationRate)
        {
            if (enemy.CanSeePlayer())
            {
                enemy.SetDestination(enemy.GetPlayer().position);
                lostPlayerTimer = 0f;
            }
            else
            {
                lostPlayerTimer += updateDestinationRate;
            }
            updateTimer = 0f;
        }
    }

    public override void OnExit(EnemyAI enemy)
    {
        lostPlayerTimer = 0f;
        updateTimer = 0f;
        Debug.Log($"{enemy.name} salió del estado: {stateName}");
    }

    public override EnemyState CheckTransitions(EnemyAI enemy)
    {
        if (enemy.ShouldFlee())
        {
            return enemy.GetState("Flee");
        }

        if (enemy.IsPlayerInAttackRange())
        {
            return enemy.GetState("Attack");
        }

        if (lostPlayerTimer >= lostPlayerTimeout)
        {
            return enemy.GetState("Patrol");
        }

        return null;
    }
}


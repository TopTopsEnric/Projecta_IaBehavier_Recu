using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Flee State", menuName = "AI/States/Flee State")]
public class FleeState : EnemyState
{
    [Header("Flee Settings")]
    public float fleeDistance = 10f;
    public float recalculateFleeRate = 1f;
    public float safeDistance = 15f;

    private float recalculateTimer;

    public override void OnEnter(EnemyAI enemy)
    {
        enemy.SetSpeed(moveSpeed);
        recalculateTimer = 0f;
        enemy.SetDestination(enemy.GetFleeDirection());

        Debug.Log($"{enemy.name} entró en estado: {stateName}");
    }

    public override void OnUpdate(EnemyAI enemy)
    {
        recalculateTimer += Time.deltaTime;

        if (recalculateTimer >= recalculateFleeRate)
        {
            if (enemy.CanSeePlayer())
            {
                enemy.SetDestination(enemy.GetFleeDirection());
            }
            recalculateTimer = 0f;
        }
    }

    public override void OnExit(EnemyAI enemy)
    {
        recalculateTimer = 0f;
        Debug.Log($"{enemy.name} salió del estado: {stateName}");
    }

    public override EnemyState CheckTransitions(EnemyAI enemy)
    {
        // Si recupera vida, volver a patrullar
        if (!enemy.ShouldFlee())
        {
            return enemy.GetState("Patrol");
        }

        // Si está lejos y no ve al jugador, volver a patrullar
        if (!enemy.CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.GetPlayer().position);
            if (distanceToPlayer >= safeDistance)
            {
                return enemy.GetState("Patrol");
            }
        }

        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Patrol State", menuName = "AI/States/Patrol State")]
public class PatrolState : EnemyState
{
    [Header("Patrol Settings")]
    public float waitTimeAtWaypoint = 1f;
    public bool useRandomOrder = true;

    // Variables de estado 
    private float waitTimer;

    public override void OnEnter(EnemyAI enemy)
    {
        enemy.SetSpeed(moveSpeed);
        waitTimer = 0f;

        // Ir al waypoint actual
        if (enemy.HasWaypoints())
        {
            enemy.SetDestination(enemy.GetCurrentWaypoint());
        }

        Debug.Log($"{enemy.name} entró en estado: {stateName}");
    }

    public override void OnUpdate(EnemyAI enemy)
    {
        // Verificar transiciones se hace en CheckTransitions

        // Continuar patrullando entre waypoints
        if (enemy.HasWaypoints() && enemy.HasReachedDestination())
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtWaypoint)
            {
                enemy.MoveToNextWaypoint();
                enemy.SetDestination(enemy.GetCurrentWaypoint());
                waitTimer = 0f;
            }
        }
    }

    public override void OnExit(EnemyAI enemy)
    {
        waitTimer = 0f;
        Debug.Log($"{enemy.name} salió del estado: {stateName}");
    }

    public override EnemyState CheckTransitions(EnemyAI enemy)
    {
        if (enemy.CanSeePlayer())
        {
            if (enemy.ShouldFlee())
            {
                return enemy.GetState("Flee");
            }
            else
            {
                return enemy.GetState("Chase");
            }
        }

        return null; // No hay transición
    }
}

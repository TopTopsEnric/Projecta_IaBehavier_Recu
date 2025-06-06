using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack State", menuName = "AI/States/Attack State")]
public class AttackState : EnemyState
{
    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    public float attackDamage = 10f;
    public bool stopMovementDuringAttack = true;

    private float attackTimer;

    public override void OnEnter(EnemyAI enemy)
    {
        if (stopMovementDuringAttack)
        {
            enemy.SetSpeed(0f);
        }
        else
        {
            enemy.SetSpeed(moveSpeed);
        }

        attackTimer = 0f;
        Debug.Log($"{enemy.name} entró en estado: {stateName}");
    }

    public override void OnUpdate(EnemyAI enemy)
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            PerformAttack(enemy);
            attackTimer = 0f;
        }
    }

    public override void OnExit(EnemyAI enemy)
    {
        attackTimer = 0f;
        Debug.Log($"{enemy.name} salió del estado: {stateName}");
    }

    public override EnemyState CheckTransitions(EnemyAI enemy)
    {
        if (enemy.ShouldFlee())
        {
            return enemy.GetState("Flee");
        }

        if (!enemy.IsPlayerInAttackRange())
        {
            if (enemy.CanSeePlayer())
            {
                return enemy.GetState("Chase");
            }
            else
            {
                return enemy.GetState("Patrol");
            }
        }

        return null;
    }

    private void PerformAttack(EnemyAI enemy)
    {
        Debug.Log($"{enemy.name} atacó por {attackDamage} de daño!");
        // Aquí puedes agregar lógica de ataque real
    }
}


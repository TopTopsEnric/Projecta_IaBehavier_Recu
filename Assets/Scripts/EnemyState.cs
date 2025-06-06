using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Enemy State", menuName = "AI/Enemy State")]
public abstract class EnemyState : ScriptableObject
{
    [Header("State Info")]
    public string stateName;
    [TextArea(2, 4)]
    public string stateDescription;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    // Métodos abstractos que cada estado debe implementar
    public abstract void OnEnter(EnemyAI enemy);
    public abstract void OnUpdate(EnemyAI enemy);
    public abstract void OnExit(EnemyAI enemy);

    // Método para verificar transiciones de estado
    public abstract EnemyState CheckTransitions(EnemyAI enemy);
}

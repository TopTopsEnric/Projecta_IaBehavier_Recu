using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamageController : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 20f;

    [Header("UI References")]
    public Button damageButton;

    [Header("Death Settings")]
    public float destroyDelay = 0.5f; // Tiempo antes de destruir el objeto

    [Header("Enemy Reference")]
    public EnemyAI targetEnemy; // Arrastra aquí el enemigo desde el inspector

    void Start()
    {
        // Configurar el botón para llamar a la función de daño
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(DamageEnemy);
        }
    }

    void Update()
    {

    }

    // Función pública que se llama desde el botón
    public void DamageEnemy()
    {
        if (targetEnemy != null)
        {
            // Reducir vida del enemigo
            targetEnemy.currentHealth -= damageAmount;

            // Asegurar que la vida no baje de 0
            targetEnemy.currentHealth = Mathf.Max(0, targetEnemy.currentHealth);

            Debug.Log($"Enemigo {targetEnemy.name} recibió {damageAmount} de daño. Vida actual: {targetEnemy.currentHealth}");

            // Si el enemigo muere
            if (targetEnemy.currentHealth <= 0)
            {
                HandleEnemyDeath();
            }
        }
        else
        {
            Debug.LogWarning("No hay enemigo asignado para recibir daño!");
        }
    }

    // Función para manejar la muerte del enemigo
    void HandleEnemyDeath()
    {
        Debug.Log($"Enemigo {targetEnemy.name} ha muerto!");

        // Aquí puedes agregar efectos adicionales antes de destruir:
        // - Efectos de partículas
        // - Sonidos de muerte
        // - Animaciones
        // - Dar puntos al jugador
        // - Soltar objetos (loot)

        // Ejemplo de efectos opcionales:
        // PlayDeathEffect();
        // GivePlayerPoints();
        // DropLoot();

        // Destruir el GameObject del enemigo después del delay
        if (destroyDelay > 0)
        {
            StartCoroutine(DestroyEnemyAfterDelay());
        }
        else
        {
            DestroyEnemyImmediate();
        }
    }

    // Corrutina para destruir con delay
    private IEnumerator DestroyEnemyAfterDelay()
    {
        // Opcional: Desactivar la IA mientras esperamos
        if (targetEnemy != null)
        {
            targetEnemy.enabled = false; // Desactiva el script de IA

            // Opcional: Desactivar el NavMeshAgent para que no se mueva
            UnityEngine.AI.NavMeshAgent agent = targetEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }
        }

        yield return new WaitForSeconds(destroyDelay);

        DestroyEnemyImmediate();
    }

    // Destruir inmediatamente
    private void DestroyEnemyImmediate()
    {
        if (targetEnemy != null)
        {
            GameObject enemyGameObject = targetEnemy.gameObject;
            Debug.Log($"Destruyendo {enemyGameObject.name}");

            // Limpiar la referencia antes de destruir
            targetEnemy = null;

            // Destruir el GameObject
            Destroy(enemyGameObject);
        }
    }

    // Función pública para curar al enemigo (pruebas)
    public void HealEnemy()
    {
        if (targetEnemy != null)
        {
            targetEnemy.currentHealth += damageAmount;
            targetEnemy.currentHealth = Mathf.Min(targetEnemy.maxHealth, targetEnemy.currentHealth);
            Debug.Log($"Enemigo {targetEnemy.name} curado. Vida actual: {targetEnemy.currentHealth}");
        }
    }
}
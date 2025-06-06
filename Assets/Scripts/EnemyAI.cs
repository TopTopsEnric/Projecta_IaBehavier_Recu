using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Vision")]
    public float visionRange = 10f;
    public float visionAngle = 60f;
    public LayerMask playerLayer = 7;
    public LayerMask obstacleLayer = 3;

    [Header("Vision Debug")]
    public bool showVisionInGame = true;
    public Color visionRangeColor = Color.yellow;
    public Color visionConeColor = Color.blue;
    public Color playerDetectedColor = Color.red;
    public int visionRayCount = 20;

    [Header("Runtime Debug")]
    public bool showDebugInPlay = true;
    public bool enableDebugLogs = true;

    [Header("Attack")]
    public float attackRange = 2f;

    [Header("Patrol Waypoints")]
    public Transform[] waypoints;

    [Header("AI States")]
    public EnemyState[] availableStates;
    public EnemyState initialState;

    // Referencias
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform player;
    private int currentWaypointIndex = 0;

    // Estado actual
    private EnemyState currentState;
    private Dictionary<string, EnemyState> stateDict;

    // Debug visual en runtime
    private List<LineRenderer> debugLines = new List<LineRenderer>();
    private GameObject debugParent;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;

        // Crear diccionario de estados para acceso rápido
        InitializeStates();

        // Inicializar waypoints
        InitializeWaypoints();

        // Setup debug visual
        if (showDebugInPlay)
        {
            SetupDebugVisuals();
        }

        // Comenzar con el estado inicial
        if (initialState != null)
        {
            ChangeState(initialState);
        }
        else if (availableStates.Length > 0)
        {
            ChangeState(availableStates[0]);
        }

        // Debug inicial
        if (enableDebugLogs)
        {
            Debug.Log($"{name}: Inicializado con estado {currentState?.stateName}");
            Debug.Log($"{name}: Player encontrado: {player?.name}");
        }
    }

    void Update()
    {
        if (currentState != null)
        {
            // Debug de estado actual
            if (enableDebugLogs)
            {
                Debug.Log($"{name}: Estado actual: {currentState.stateName}");
                Debug.Log($"{name}: Puede ver jugador: {CanSeePlayer()}");
                Debug.Log($"{name}: Debería huir: {ShouldFlee()}");
                Debug.Log($"{name}: En rango de ataque: {IsPlayerInAttackRange()}");
            }

            // Actualizar estado actual
            currentState.OnUpdate(this);

            // Verificar transiciones con debug detallado
            EnemyState newState = currentState.CheckTransitions(this);

            if (newState != null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"{name}: Transición detectada de {currentState.stateName} a {newState.stateName}");
                }

                if (newState != currentState)
                {
                    ChangeState(newState);
                }
                else if (enableDebugLogs)
                {
                    Debug.Log($"{name}: Transición al mismo estado ignorada");
                }
            }
            else if (enableDebugLogs)
            {
                Debug.Log($"{name}: No hay transiciones disponibles desde {currentState.stateName}");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogError($"{name}: ¡currentState esta vacio!");
        }

        // Actualizar debug visual en runtime
        if (showDebugInPlay && debugLines.Count > 0)
        {
            UpdateDebugVisuals();
        }
    }
    // comprobante de funcionalidad de debug visual, me estaba dando muchos problemas
    void SetupDebugVisuals()
    {
        // Crear GameObject padre para las líneas de debug
        debugParent = new GameObject("VisionDebug");
        debugParent.transform.SetParent(transform);

        // Crear múltiples LineRenderers para el cono de visión
        for (int i = 0; i <= visionRayCount; i++)
        {
            GameObject lineObj = new GameObject($"VisionRay_{i}");
            lineObj.transform.SetParent(debugParent.transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            // Configurar LineRenderer
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;

            // Asegurar que el material no sea null
            if (lr.material == null)
            {
                lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            }

            debugLines.Add(lr);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"{name}: Debug visual configurado con {debugLines.Count} líneas");
        }
    }

    void UpdateDebugVisuals()
    {
        Vector3 eyePosition = transform.position + Vector3.up * 0.5f;
        bool canSee = CanSeePlayer();

        for (int i = 0; i < debugLines.Count && i <= visionRayCount; i++)
        {
            LineRenderer lr = debugLines[i];

            // Verificar que el LineRenderer y su material no sean null
            if (lr == null || lr.material == null) continue;

            // Calcular dirección del rayo
            float currentAngle = -visionAngle / 2 + (visionAngle / visionRayCount) * i;
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            // Hacer raycast
            RaycastHit hit;
            float rayDistance = visionRange;
            Color rayColor = canSee ? playerDetectedColor : visionConeColor;

            if (Physics.Raycast(eyePosition, rayDirection, out hit, visionRange, obstacleLayer))
            {
                rayDistance = hit.distance;
                rayColor = Color.gray;
            }

            // Actualizar LineRenderer
            lr.SetPosition(0, eyePosition);
            lr.SetPosition(1, eyePosition + rayDirection * rayDistance);
            lr.material.color = rayColor;
            lr.enabled = showDebugInPlay;
        }
    }

    void InitializeStates()
    {
        stateDict = new Dictionary<string, EnemyState>();

        foreach (EnemyState state in availableStates)
        {
            if (state != null)
            {
                stateDict[state.stateName] = state;
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"{name}: Estados disponibles: {string.Join(", ", stateDict.Keys)}");
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (newState == null) return;

        // Salir del estado actual
        currentState?.OnExit(this);

        string previousStateName = currentState?.stateName ?? "None";

        // Cambiar al nuevo estado
        currentState = newState;
        currentState.OnEnter(this);

        if (enableDebugLogs)
        {
            Debug.Log($"{name}: Cambio de estado: {previousStateName} → {currentState.stateName}");
        }
    }

    public EnemyState GetState(string stateName)
    {
        stateDict.TryGetValue(stateName, out EnemyState state);
        if (state == null && enableDebugLogs)
        {
            Debug.LogWarning($"{name}: Estado '{stateName}' no encontrado!");
        }
        return state;
    }

    public EnemyState GetCurrentState()
    {
        return currentState;
    }

    // Métodos de waypoints
    void InitializeWaypoints()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning($"{name}: Sin waypoints! Enemigo no patrullara.");
            return;
        }

        currentWaypointIndex = Random.Range(0, waypoints.Length);
    }

    public Vector3 GetCurrentWaypoint()
    {
        if (waypoints.Length == 0)
        {
            return transform.position;
        }

        return waypoints[currentWaypointIndex].position;
    }

    public void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        // Siempre aleatorio p
        int newIndex;
        do
        {
            newIndex = Random.Range(0, waypoints.Length);
        }
        while (newIndex == currentWaypointIndex && waypoints.Length > 1);

        currentWaypointIndex = newIndex;
    }

    public bool HasWaypoints()
    {
        return waypoints.Length > 0;
    }

    // MÉTODO PRINCIPAL DE DETECCIÓN - CORREGIDO
    public bool CanSeePlayer()
    {
        if (player == null)
        {
            if (enableDebugLogs)
                Debug.Log($"{name}: Player es null");
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        // comprobantes de si ve al jugador, siempre tiene que verlo mucho no solo 3 raycast, minimo la mitad de los rayos
        if (distanceToPlayer > visionRange)
        {
            if (enableDebugLogs)
                Debug.Log($"{name}: Jugador fuera de rango: {distanceToPlayer:F2} > {visionRange}");
            return false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > visionAngle / 2)
        {
            if (enableDebugLogs)
                Debug.Log($"{name}: Jugador fuera del ángulo: {angle:F2} > {visionAngle / 2}");
            return false;
        }

        // Raycast para verificar obstáculos 
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        // Raycast hacia el jugador
        if (Physics.Raycast(rayStart, directionToPlayer, out hit, distanceToPlayer))
        {
            if (enableDebugLogs)
                Debug.Log($"{name}: Raycast tocó: {hit.collider.name} (Tag: {hit.collider.tag})");

            // Si el primer objeto que toca es el jugador, puede verlo
            if (hit.collider.CompareTag("Player"))
            {
                if (enableDebugLogs)
                    Debug.Log($"{name}: ¡JUGADOR DETECTADO! Dist: {distanceToPlayer:F2}, Angle: {angle:F2}");
                return true;
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"{name}: Obstáculo bloqueando: {hit.collider.name}");
                return false;
            }
        }

        // Si el raycast no toca nada, también puede verlo
        if (enableDebugLogs)
            Debug.Log($"{name}: ¡JUGADOR VISIBLE! (No hay obstáculos) Dist: {distanceToPlayer:F2}, Angle: {angle:F2}");
        return true;
    }

    // Método mejorado para múltiples raycasts de visión
    public bool CanSeePlayerAdvanced()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > visionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > visionAngle / 2) return false;

        // Múltiples raycasts para mejor detección
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3[] rayDirections = {
            directionToPlayer,                                    // Centro
            directionToPlayer + Vector3.up * 0.3f,              // Arriba
            directionToPlayer + Vector3.down * 0.3f,            // Abajo
            directionToPlayer + transform.right * 0.2f,         // Derecha
            directionToPlayer + transform.right * -0.2f         // Izquierda
        };

        int successfulRays = 0;
        foreach (Vector3 rayDir in rayDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(rayStart, rayDir.normalized, out hit, distanceToPlayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    successfulRays++;
                }
            }
            else
            {
                successfulRays++; // No hay obstáculo
            }
        }

        // Si al menos la mitad de los rayos tienen éxito
        return successfulRays >= (rayDirections.Length / 2);
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        bool inRange = Vector3.Distance(transform.position, player.position) <= attackRange;

        if (enableDebugLogs && inRange)
            Debug.Log($"{name}: Jugador en rango de ataque!");

        return inRange;
    }

    public bool ShouldFlee()
    {
        bool shouldFlee = currentHealth < (maxHealth * 0.5f);

        if (enableDebugLogs && shouldFlee)
            Debug.Log($"{name}: Debería huir! Vida: {currentHealth}/{maxHealth}");

        return shouldFlee;
    }

    // Métodos de movimiento
    public void SetDestination(Vector3 destination)
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(destination);

            if (enableDebugLogs)
                Debug.Log($"{name}: Destino establecido: {destination}");
        }
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public Vector3 GetFleeDirection()
    {
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionFromPlayer * 10f;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(fleeTarget, out hit, 10f, 1))
        {
            return hit.position;
        }

        return transform.position;
    }

    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance < 0.5f;
    }

    public Transform GetPlayer()
    {
        return player;
    }

    // Método público para activar/desactivar debug
    public void ToggleDebugLogs()
    {
        enableDebugLogs = !enableDebugLogs;
        Debug.Log($"{name}: Debug logs {(enableDebugLogs ? "activados" : "desactivados")}");
    }

    public void ToggleVisualDebug()
    {
        showDebugInPlay = !showDebugInPlay;

        // Activar/desactivar LineRenderers
        foreach (LineRenderer lr in debugLines)
        {
            lr.enabled = showDebugInPlay;
        }
    }

    // Visualización en tiempo real y editor
    void OnDrawGizmos()
    {
        if (!showVisionInGame) return;

        DrawVisionDebug();
    }

    void DrawVisionDebug()
    {
        if (Application.isPlaying)
        {
            // Color basado en si detecta al jugador
            Color currentVisionColor = CanSeePlayer() ? playerDetectedColor : visionConeColor;

            // Dibujar cono de visión con múltiples rayos
            Vector3 eyePosition = transform.position + Vector3.up * 0.5f;

            for (int i = 0; i <= visionRayCount; i++)
            {
                float currentAngle = -visionAngle / 2 + (visionAngle / visionRayCount) * i;
                Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

                RaycastHit hit;
                float rayDistance = visionRange;
                Color rayColor = currentVisionColor;

                if (Physics.Raycast(eyePosition, rayDirection, out hit, visionRange, obstacleLayer))
                {
                    rayDistance = hit.distance;
                    rayColor = Color.gray; // Color diferente si hay obstáculo
                }

                Gizmos.color = rayColor;
                Gizmos.DrawRay(eyePosition, rayDirection * rayDistance);
            }

            // Círculo de rango de visión
            Gizmos.color = visionRangeColor;
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        DrawVisionDebug();
        DrawWaypointDebug();
        DrawStateDebug();
    }

    void DrawWaypointDebug()
    {
        // Waypoints
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.green;

            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawWireSphere(waypoint.position, 0.5f);
                }
            }

            if (Application.isPlaying && waypoints[currentWaypointIndex] != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 0.7f);
            }
        }
    }

    void DrawStateDebug()
    {
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Mostrar estado actual en el editor, era para pruebas
        #if UNITY_EDITOR
        if (currentState != null)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                $"Estado: {currentState.stateName}\nVida: {currentHealth:F0}/{maxHealth:F0}");
        }
        #endif
    }

    // Cleanup
    void OnDestroy()
    {
        if (debugParent != null)
        {
            DestroyImmediate(debugParent);
        }
    }
}
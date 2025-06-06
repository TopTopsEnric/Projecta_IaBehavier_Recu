
using UnityEngine;

[System.Serializable]
public class VisionSettings
{
    // Clase que usaba para hacer pruebas de vision en las versiones anteriores dentro del modo de edicion, no de juego
    [Header("Basic Vision")]
    public float range = 10f;
    public float angle = 60f;

    [Header("Advanced Detection")]
    public bool useMultipleRays = true;
    public int rayCount = 5;
    public float raySpread = 0.5f;

    [Header("Wall Detection")]
    public LayerMask wallLayers = -1;
    public float wallCheckDistance = 0.2f;
    public bool ignoreSmallObstacles = true;
    public float minObstacleHeight = 1f;

    [Header("Visual Feedback")]
    public bool showInGame = true;
    public Color normalVisionColor = Color.blue;
    public Color detectedColor = Color.red;
    public Color blockedColor = Color.gray;
}


public class EnemyVisionTester : MonoBehaviour
{
    [Header("Vision Testing")]
    public VisionSettings visionSettings;
    public Transform testTarget;
    public bool continuousTesting = true;

    [Header("Results")]
    [SerializeField] private bool canSeeTarget;
    [SerializeField] private float distanceToTarget;
    [SerializeField] private string blockingObject;

    void Update()
    {
        if (continuousTesting && testTarget != null)
        {
            TestVision();
        }
    }

    public void TestVision()
    {
        if (testTarget == null) return;

        // Calcular distancia
        distanceToTarget = Vector3.Distance(transform.position, testTarget.position);

        // Verificar si está en rango
        if (distanceToTarget > visionSettings.range)
        {
            canSeeTarget = false;
            blockingObject = "Out of range";
            return;
        }

        // Verificar ángulo
        Vector3 directionToTarget = (testTarget.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (angle > visionSettings.angle / 2)
        {
            canSeeTarget = false;
            blockingObject = "Out of vision angle";
            return;
        }

        // Hacer raycast
        Vector3 eyePosition = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (visionSettings.useMultipleRays)
        {
            canSeeTarget = MultiRaycast(eyePosition, directionToTarget, out blockingObject);
        }
        else
        {
            if (Physics.Raycast(eyePosition, directionToTarget, out hit, visionSettings.range, visionSettings.wallLayers))
            {
                if (hit.collider.transform == testTarget)
                {
                    canSeeTarget = true;
                    blockingObject = "Clear";
                }
                else
                {
                    canSeeTarget = false;
                    blockingObject = hit.collider.name;
                }
            }
            else
            {
                canSeeTarget = true;
                blockingObject = "Clear";
            }
        }
    }

    bool MultiRaycast(Vector3 origin, Vector3 baseDirection, out string blocker)
    {
        blocker = "Clear";
        int successfulRays = 0;

        for (int i = 0; i < visionSettings.rayCount; i++)
        {
            Vector3 rayDirection = baseDirection;

            // Añadir variación al rayo
            if (i > 0)
            {
                float spread = visionSettings.raySpread;
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spread, spread),
                    Random.Range(-spread, spread),
                    0
                );
                rayDirection = (baseDirection + randomOffset).normalized;
            }

            RaycastHit hit;
            if (Physics.Raycast(origin, rayDirection, out hit, visionSettings.range, visionSettings.wallLayers))
            {
                if (hit.collider.transform == testTarget)
                {
                    successfulRays++;
                }
                else if (blocker == "Clear")
                {
                    blocker = hit.collider.name;
                }
            }
            else
            {
                successfulRays++;
            }
        }

        // Si al menos la mitad de los rayos tienen éxito
        return successfulRays >= (visionSettings.rayCount / 2);
    }

    void OnDrawGizmos()
    {
        if (!visionSettings.showInGame || testTarget == null) return;

        // Dibujar cono de visión
        Vector3 eyePosition = transform.position + Vector3.up * 0.5f;

        // Color basado en detección
        Color gizmoColor = canSeeTarget ? visionSettings.detectedColor : visionSettings.normalVisionColor;

        // Dibujar rayos del cono
        int rayCount = 20;
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -visionSettings.angle / 2 + (visionSettings.angle / rayCount) * i;
            Vector3 rayDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            RaycastHit hit;
            float rayDistance = visionSettings.range;
            Color rayColor = gizmoColor;

            if (Physics.Raycast(eyePosition, rayDirection, out hit, visionSettings.range, visionSettings.wallLayers))
            {
                rayDistance = hit.distance;
                if (hit.collider.transform != testTarget)
                {
                    rayColor = visionSettings.blockedColor;
                }
            }

            Gizmos.color = rayColor;
            Gizmos.DrawRay(eyePosition, rayDirection * rayDistance);
        }

        // Círculo de rango
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionSettings.range);

        // Línea al objetivo
        if (testTarget != null)
        {
            Gizmos.color = canSeeTarget ? Color.green : Color.red;
            Gizmos.DrawLine(eyePosition, testTarget.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Información adicional en el inspector
        if (testTarget != null)
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, 
                $"Can See: {canSeeTarget}\n" +
                $"Distance: {distanceToTarget:F1}m\n" +
                $"Blocker: {blockingObject}");
#endif
        }
    }
}
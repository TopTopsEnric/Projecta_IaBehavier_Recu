using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // Grados por segundo

    private CharacterController characterController;
    private Vector3 moveDirection;

    void Start()
    {
        // Intentar obtener CharacterController, si no existe usar transform
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.Log("No CharacterController found, using Transform movement");
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Obtener input del teclado
        float horizontal = Input.GetAxis("Horizontal"); // A/D o flechas izq/der
        float vertical = Input.GetAxis("Vertical");     // W/S o flechas arr/abajo

        // Crear vector de movimiento
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

        // Solo moverse si hay input
        if (inputDirection.magnitude > 0.1f)
        {
            // Normalizar para movimiento consistente en diagonales
            inputDirection.Normalize();

            // Rotar hacia la dirección de movimiento
            if (inputDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Mover el jugador
            Vector3 moveVector = inputDirection * moveSpeed * Time.deltaTime;

            if (characterController != null)
            {
                // Usar CharacterController si está disponible
                characterController.Move(moveVector);
            }
            else
            {
                // Usar Transform si no hay CharacterController
                transform.Translate(moveVector, Space.World);
            }
        }
    }

    // Método público para obtener si el jugador se está moviendo
    public bool IsMoving()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
    }

    // Método público para obtener la velocidad actual
    public Vector3 GetVelocity()
    {
        if (characterController != null)
        {
            return characterController.velocity;
        }
        return Vector3.zero;
    }
}

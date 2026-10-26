using UnityEngine;
using UnityEngine.InputSystem; // Importante para el nuevo Input System

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))] // Añade automáticamente el componente PlayerInput
public class PlayerControllerInputSystem : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 8f;
    public float turnSpeed = 720f;

    private Rigidbody rb;
    private Vector2 inputVector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Bloquear rotaciones en X y Z para evitar que el jugador se voltee
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Este método se llama automáticamente desde el componente PlayerInput cuando el jugador usa WASD/Sticks
    public void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Convertir el Vector2 de entrada a un Vector3 de movimiento en el mundo 3D (X, Z)
        Vector3 movement = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        if (movement.magnitude > 0.1f)
        {
            // Mover la posición
            Vector3 targetPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            // Rotar hacia la dirección del movimiento
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }
}
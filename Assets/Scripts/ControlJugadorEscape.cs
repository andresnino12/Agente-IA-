using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class ControlJugadorEscape : MonoBehaviour
{
    [SerializeField] private Transform agente;
    [SerializeField] private float velocidadEscape = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (agente == null) return;


        Vector3 direccionEscape = (transform.localPosition - agente.localPosition).normalized;
        direccionEscape.y = 0;


        rb.AddForce(direccionEscape * velocidadEscape);
    }
}
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

/// Clase que define el comportamiento de un agente basado en Aprendizaje por Refuerzo.
/// Dependiendo del estado del GameManager, el agente aprende a perseguir o huir de un objetivo.
public class AgenteBuscador : Agent
{
    [Header("Configuración del Agente")]
    [SerializeField] private GameObject objetivo; // Referencia al objeto que debe perseguir o del que debe huir
    [SerializeField] private float velocidad = 15f; // Fuerza de aceleración aplicada al movimiento

    private Rigidbody rb;         // Física del propio agente
    private Rigidbody rbObjetivo; // Física del objetivo (para obtener su velocidad)

    /// Se ejecuta una sola vez al iniciar. Funciona como el Awake/Start para ML-Agents.
    public override void Initialize()
    {
        // Obtiene el componente Rigidbody del agente
        rb = GetComponent<Rigidbody>();

        // Si existe un objetivo, obtiene su Rigidbody para leer sus datos físicos
        if (objetivo != null)
        {
            rbObjetivo = objetivo.GetComponent<Rigidbody>();
        }
    }

    /// Se llama automáticamente al inicio de cada episodio de entrenamiento.
    /// Reinicia las posiciones y velocidades del agente y del objetivo.
    public override void OnEpisodeBegin()
    {
        // Detiene cualquier movimiento lineal o rotacional previo del agente
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Detiene el movimiento del objetivo si tiene Rigidbody
        if (rbObjetivo != null)
        {
            rbObjetivo.linearVelocity = Vector3.zero;
            rbObjetivo.angularVelocity = Vector3.zero;
        }

        // Reubica al agente en una posición aleatoria dentro del área (-4 a 4 en X y Z)
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));

        // Reubica al objetivo en otra posición aleatoria dentro del área
        if (objetivo != null)
        {
            objetivo.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        }
    }

    /// Captura y envía la información del entorno a la red neuronal (observaciones).
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Vector de dirección/distancia relativa desde el agente hacia el objetivo (3 valores: X, Y, Z)
        Vector3 vectorHaciaObjetivo = objetivo.transform.localPosition - transform.localPosition;
        sensor.AddObservation(vectorHaciaObjetivo);

        // 2. Velocidad actual del agente (3 valores: X, Y, Z)
        sensor.AddObservation(rb.linearVelocity);

        // 3. Velocidad actual del objetivo (3 valores: X, Y, Z)
        if (rbObjetivo != null)
        {
            sensor.AddObservation(rbObjetivo.linearVelocity);
        }
        else
        {
            sensor.AddObservation(Vector3.zero); // Si no hay Rigidbody, envía un vector cero para no romper el tamaño del vector
        }
    }

    /// Se ejecuta en cada paso de simulación. Recibe las acciones tomadas por la IA (o la heurística),
    /// aplica física y calcula la recompensa del agente.
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Lee los valores de movimiento enviados por la red neuronal y los limita entre -1 y 1
        float movimientoX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float movimientoZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Consulta al GameManager para saber si el rol actual es Perseguidor o Presa
        bool cuboEsPerseguidor = GameManager.Instance == null || GameManager.Instance.cuboEsPerseguidor;

        // Si el rol es huir, invierte la dirección del movimiento decidido por la red
        if (!cuboEsPerseguidor)
        {
            movimientoX = -movimientoX;
            movimientoZ = -movimientoZ;
        }

        // Aplica fuerza física al Rigidbody en los ejes X y Z
        Vector3 fuerza = new Vector3(movimientoX, 0, movimientoZ);
        rb.AddForce(fuerza * velocidad, ForceMode.Acceleration);

        // --- SISTEMA DE RECOMPENSAS INTERMEDIAS ---

        // Calcula el alineamiento entre la dirección a la que se mueve el agente y la posición del objetivo
        Vector3 dirObjetivo = (objetivo.transform.localPosition - transform.localPosition).normalized;
        float alineacion = Vector3.Dot(rb.linearVelocity.normalized, dirObjetivo);

        // Premia la alineación si persigue, o la penaliza si huye
        if (cuboEsPerseguidor)
        {
            AddReward(alineacion * 0.005f); // Pequeña recompensa si se mueve hacia el objetivo
        }
        else
        {
            AddReward(-alineacion * 0.005f); // Pequeña recompensa si se mueve en dirección opuesta al objetivo
        }

        // Penalización por tiempo (fuerza al agente a completar el objetivo rápido)
        AddReward(-0.001f);

        // --- CONDICIONES DE FINALIZACIÓN DE EPISODIO ---

        float distanciaAlObjetivo = Vector3.Distance(transform.localPosition, objetivo.transform.localPosition);

        // Condición 1: Atrapó/Alcanzó al objetivo (Colisión por cercanía)
        if (distanciaAlObjetivo < 1.42f)
        {
            // Otorga +1.5 si es perseguidor (éxito) o -1.0 si debía escapar (fracaso)
            SetReward(cuboEsPerseguidor ? 1.5f : -1.0f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegistrarAtrapada();
            }

            EndEpisode();
        }
        // Condición 2: El agente se cayó del escenario (Y < 0)
        else if (transform.localPosition.y < 0)
        {
            SetReward(-1.0f); // Penalización por caerse
            EndEpisode();
        }
        // Condición 3: El objetivo se cayó del escenario
        else if (objetivo.transform.localPosition.y < 0)
        {
            EndEpisode(); // Finaliza el episodio sin modificar la recompensa final
        }
    }

    /// Permite a un jugador humano controlar manualmente al agente mediante el teclado (Teclas WASD).
    /// Se activa cuando el comportamiento del Agent está configurado en "Heuristic Only".
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var accionesContinuas = actionsOut.ContinuousActions;
        accionesContinuas[0] = 0f;
        accionesContinuas[1] = 0f;

        // Lee la entrada del nuevo Input System de Unity
        if (Keyboard.current != null)
        {
            // Movimiento Horizontal (Eje X: A / D)
            if (Keyboard.current.aKey.isPressed) accionesContinuas[0] = -1.0f;
            else if (Keyboard.current.dKey.isPressed) accionesContinuas[0] = 1.0f;

            // Movimiento Vertical (Eje Z: W / S)
            if (Keyboard.current.wKey.isPressed) accionesContinuas[1] = 1.0f;
            else if (Keyboard.current.sKey.isPressed) accionesContinuas[1] = -1.0f;
        }
    }
}
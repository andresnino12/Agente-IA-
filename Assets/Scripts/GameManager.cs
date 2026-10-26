using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración del Juego")]
    public int intentosRestantes = 1;
    public bool cuboEsPerseguidor = true;

    [Header("Referencias de Objetos")]
    public GameObject cuboAgente;
    public GameObject bolaJugador;

    [Header("Interfaz UI")]
    public TextMeshProUGUI textoIntentos;
    public TextMeshProUGUI textoRol;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActualizarUI();
    }


    public void RegistrarAtrapada()
    {
        intentosRestantes--;

        if (intentosRestantes <= 0)
        {
            InvertirRoles();
        }
        else
        {
            ActualizarUI();
        }
    }

    void InvertirRoles()
    {
        cuboEsPerseguidor = !cuboEsPerseguidor;
        intentosRestantes = 1;


        if (cuboEsPerseguidor)
        {
            Debug.Log("Agente Persigue");
        }
        else
        {
            Debug.Log("Jugador Persigue");
        }

        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (textoIntentos != null)
            textoIntentos.text = "Intentos restantes: " + intentosRestantes;

        if (textoRol != null)
        {
            if (cuboEsPerseguidor)
                textoRol.text = "CORREEEEEE";
            else
                textoRol.text = "ATRAPALO!";
        }
    }
}
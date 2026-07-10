using UnityEngine;
using TMPro;

public class IntroContador : MonoBehaviour
{
    [Header("Referencias de UI")]
    // El Canvas de la presentación (este sí se apaga de forma normal)
    public GameObject presentationCanvas;

    // Cambiamos el GameObject por una referencia directa al script de tu Lobby OnGUI
    public LobbyYellowFlag lobbyScript;

    // El componente de texto para los números
    public TextMeshProUGUI textoContador;

    [Header("Configuración")]
    public float tiempoRestante = 3.0f;

    private bool cuentaActiva = true;

    void Start()
    {
        // Al iniciar, desactivamos el script del Lobby.
        // Esto evita por completo que la función OnGUI() se ejecute y dibuje los botones en pantalla.
        if (lobbyScript != null)
        {
            lobbyScript.enabled = false;
        }
    }

    void Update()
    {
        if (!cuentaActiva) return;

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString();
        }
        else
        {
            tiempoRestante = 0;
            textoContador.text = "0";
            cuentaActiva = false;

            // Transición de pantallas
            TerminarPresentacion();
        }
    }

    void TerminarPresentacion()
    {
        // 1. Apagamos la presentación (Canvas)
        if (presentationCanvas != null)
        {
            presentationCanvas.SetActive(false);
        }

        // 2. Encendemos el script del Lobby. 
        // Al activarse el script, Unity llamará inmediatamente a OnGUI() y el menú aparecerá.
        if (lobbyScript != null)
        {
            lobbyScript.enabled = true;
            Debug.Log("Intro finalizada. Interfaz OnGUI de Yellow Flag activada.");
        }
    }
}
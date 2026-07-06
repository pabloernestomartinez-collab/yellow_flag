using UnityEngine;
using TMPro; // ¡Obligatorio para poder controlar el texto!

public class IntroContador : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject presentationCanvas;
    public TextMeshProUGUI textoContador;

    [Header("Configuración")]
    private float tiempoRestante = 5.0f;    // Tiempo en segundos para la cuenta regresiva


    private bool cuentaActiva = true;

    void Update()
    {
        if (!cuentaActiva) return;        // Si la cuenta ya terminó, no hacemos nada más


        if (tiempoRestante > 0)        // Si todavía queda tiempo...

        {
            tiempoRestante -= Time.deltaTime;

            textoContador.text = Mathf.CeilToInt(tiempoRestante).ToString();            // Usamos Mathf.CeilToInt para redondear hacia arriba (así muestra 3, luego 2, luego 1).

        }
        else
        {
            tiempoRestante = 0;
            textoContador.text = "0";
            cuentaActiva = false;

            // Llamamos a la función para apagar el Canvas
            TerminarPresentacion();
        }
    }

    void TerminarPresentacion()
    {
        if (presentationCanvas != null)
        {
            presentationCanvas.SetActive(false);
            Debug.Log("¡Cuenta regresiva terminada! Canvas apagado.");
        }
    }
}
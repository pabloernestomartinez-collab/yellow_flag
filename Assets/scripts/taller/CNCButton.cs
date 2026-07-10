using UnityEngine;
using Unity.Netcode; // Es indispensable que herede de NetworkBehaviour en lugar de MonoBehaviour

public class CncButton : NetworkBehaviour, IInteractable
{
    [Header("Configuración Visual")]
    public Renderer botonRenderer;       // El componente MeshRenderer del botón
    public Color colorPresionado = Color.green;
    private Color colorOriginal;

    [Header("Configuración de Animación (Opcional)")]
    public Animator botonAnimator;       // Si tienes una animación de hundirse

    private void Start()
    {
        // Guardamos el color original del botón al iniciar la escena
        if (botonRenderer != null)
        {
            colorOriginal = botonRenderer.material.color;
        }
    }

    // Este es el método que llama el script 'PlayerInteraction' del Host al hacer clic
    public void Interact()
    {
        // REGLA DE SEGURIDAD: Solo el Host/Servidor procesa la lógica de interacción
        if (!IsServer) return;

        // 1. Ejecutamos la acción en el servidor (opcional, ej: prender el husillo)
        EjecutarLogicaInternaCNC();

        // 2. LE ORDENAMOS A TODOS LOS CLIENTES QUE CAMBIEN EL COLOR
        SincronizarEsteticaBotonClientRpc();
    }

    private void EjecutarLogicaInternaCNC()
    {
        Debug.Log($"[Instructor] Botón presionado. Enviando comando a la máquina.");
    }

    // ------------------------------------------------------------------------
    // EL TRUCO DE RED: ClientRpc
    // ------------------------------------------------------------------------
    // Todo lo que se escriba dentro de un ClientRpc se ejecutará en las pantallas
    // de TODOS los alumnos conectados en milisegundos.
    [ClientRpc]
    private void SincronizarEsteticaBotonClientRpc()
    {
        // A) Cambiamos el color en el monitor de todos (Host y Clientes)
        if (botonRenderer != null)
        {
            botonRenderer.material.color = colorPresionado;
        }

        // B) Si tienes animación de pulsación, también se sincroniza aquí
        if (botonAnimator != null)
        {
            botonAnimator.SetTrigger("Press");
        }

        // C) Programamos el regreso al color original después de un breve instante (0.2 segundos)
        Invoke(nameof(RestaurarColorOriginal), 0.2f);
    }

    private void RestaurarColorOriginal()
    {
        if (botonRenderer != null)
        {
            botonRenderer.material.color = colorOriginal;
        }
    }

    // Métodos obligatorios de la interfaz de selección visual (Hover)
    public void OnHoverEnter()
    {
        // Solo el Instructor ve la silueta o iluminación al pasar el mouse por encima
        if (IsServer && botonRenderer != null)
        {
            botonRenderer.material.SetColor("_EmissionColor", Color.white * 0.2f);
        }
    }

    public void OnHoverExit()
    {
        if (IsServer && botonRenderer != null)
        {
            botonRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}
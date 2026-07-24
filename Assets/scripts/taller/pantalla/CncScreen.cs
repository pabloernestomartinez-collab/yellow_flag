using UnityEngine;
using UnityEngine.InputSystem; // Sistema de Input de Unity
using Unity.Netcode;

public class CncScreen : NetworkBehaviour, IInteractable
{
    [Header("Configuración Visual 3D")]
    public Renderer pantallaRenderer;

    [Header("Interfaz de Usuario (UI)")]
    public GameObject pantalla_CNC;

    private void Start()
    {
        if (pantalla_CNC != null)
        {
            pantalla_CNC.SetActive(false);
        }
    }

    private void Update()
    {
        // Solo el Instructor (Server) puede procesar el cierre por teclado
        if (!IsServer) return;

        // Si la pantalla está abierta y el Instructor presiona la tecla ESCAPE o ESPACIO
        if (pantalla_CNC != null && pantalla_CNC.activeSelf)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                // Cerramos la pantalla en todas las computadoras conectadas
                SincronizarEstadoPantallaClientRpc(false);
            }
        }
    }

    // ------------------------------------------------------------------------
    // INTERACCIÓN (Llamada al hacer clic en la pantalla 3D)
    // ------------------------------------------------------------------------

    public void Interact()
    {
        if (!IsServer) return;

        if (pantalla_CNC != null)
        {
            // Si está cerrada la abrimos; si ya está abierta la cerramos
            bool nuevoEstado = !pantalla_CNC.activeSelf;
            SincronizarEstadoPantallaClientRpc(nuevoEstado);
        }
    }

    public void OnHoverEnter()
    {
        if (IsServer && pantallaRenderer != null)
        {
            pantallaRenderer.material.SetColor("_EmissionColor", Color.white * 0.15f);
        }
    }

    public void OnHoverExit()
    {
        if (IsServer && pantallaRenderer != null)
        {
            pantallaRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }

    // ------------------------------------------------------------------------
    // SINCRONIZACIÓN EN RED (ClientRpc)
    // ------------------------------------------------------------------------

    [ClientRpc]
    private void SincronizarEstadoPantallaClientRpc(bool estaActiva)
    {
        if (pantalla_CNC != null)
        {
            pantalla_CNC.SetActive(estaActiva);
        }

        Debug.Log($"[YELLOW FLAG] Estado del Canvas pantalla_CNC: {estaActiva}");
    }

    // Método público por si querés conectar un botón "X" de UI en tu Canvas
    public void BotonCerrarPantallaUI()
    {
        if (IsServer)
        {
            SincronizarEstadoPantallaClientRpc(false);
        }
    }
}
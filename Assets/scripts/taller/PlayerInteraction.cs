using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public float interactionRange = 2.0f;
    public LayerMask interactableLayer;

    [Header("Interfaz de Usuario (UI)")]
    // Arrastra aquí el GameObject de la imagen de tu mira en el Canvas
    public GameObject miraInteractivaUI;

    // Arrastra aquí el Canvas 'pantalla_CNC' para bloquear el raycast cuando esté abierto
    public GameObject pantalla_CNC;

    private IInteractable currentInteractable;

    void Start()
    {
        // Nos aseguramos de que la mira empiece apagada al iniciar el taller
        if (miraInteractivaUI != null)
        {
            miraInteractivaUI.SetActive(false);
        }
    }

    void Update()
    {
        // ------------------------------------------------------------------------
        // REGLA DE AUTORIDAD DE YELLOW FLAG
        // ------------------------------------------------------------------------
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            LimpiarSeleccionYMira();
            return;
        }
        // ------------------------------------------------------------------------

        // ------------------------------------------------------------------------
        // BLOQUEO POR PANTALLA CNC ABIERTA (OPCIÓN 1)
        // ------------------------------------------------------------------------
        // Si la pantalla CNC está abierta en el Canvas, cancelamos toda interacción 3D.
        if (pantalla_CNC != null && pantalla_CNC.activeSelf)
        {
            LimpiarSeleccionYMira();
            return; // Detenemos la ejecución aquí; el Raycast no se disparará
        }
        // ------------------------------------------------------------------------

        if (Mouse.current == null) return;

        // 1. Trazar el Rayo (Solo para el Host)
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        // 2. Disparar el Raycast
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();

            if (interactableObject != null)
            {
                if (interactableObject != currentInteractable)
                {
                    if (currentInteractable != null)
                        currentInteractable.OnHoverExit();

                    currentInteractable = interactableObject;
                    currentInteractable.OnHoverEnter();

                    // Encendemos la mira al apuntar a un objeto interactivo
                    if (miraInteractivaUI != null)
                    {
                        miraInteractivaUI.SetActive(true);
                    }
                }

                // 3. Detectar el Clic Izquierdo
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    currentInteractable.Interact();
                }
            }
        }
        else
        {
            // Si el rayo no toca nada interactivo, limpiamos el estado
            LimpiarSeleccionYMira();
        }

        // Posicionamiento dinámico de la mira sobre el cursor
        if (miraInteractivaUI != null && miraInteractivaUI.activeSelf)
        {
            miraInteractivaUI.transform.position = mousePos;
        }
    }

    // Método auxiliar para evitar repetición de código al apagar la mira y limpiar selecciones
    private void LimpiarSeleccionYMira()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
        }

        if (miraInteractivaUI != null && miraInteractivaUI.activeSelf)
        {
            miraInteractivaUI.SetActive(false);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // ¡Añadimos esta librería para saber quién es quién en la red!

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public float interactionRange = 2.0f;
    public LayerMask interactableLayer;

    private IInteractable currentInteractable;

    void Update()
    {
        // ------------------------------------------------------------------------
        // REGLA DE AUTORIDAD DE YELLOW FLAG
        // ------------------------------------------------------------------------
        // Si el NetworkManager está corriendo y el jugador local NO es el Servidor/Host...
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            // El cliente es un mero observador. Si estaba mirando un botón, lo deseleccionamos.
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }

            return; // Cortamos el Update aquí. El código de abajo nunca se ejecutará para el cliente.
        }
        // ------------------------------------------------------------------------

        if (Mouse.current == null) return;

        // 1. Trazar el Rayo (Solo se ejecutará en la pantalla del Host)
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
                }

                // 3. Detectar el Clic Izquierdo (Solo permitido al Host)
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    currentInteractable.Interact();
                }
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
        }
    }
}
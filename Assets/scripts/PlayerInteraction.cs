using UnityEngine;
using UnityEngine.InputSystem; // ¡Añadimos esta línea crucial!

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    public float interactionRange = 2.0f;
    public LayerMask interactableLayer;

    private IInteractable currentInteractable;

    void Update()
    {
        // Verificamos si hay un mouse conectado para evitar errores
        if (Mouse.current == null) return;

        // 1. Trazar el Rayo usando el Nuevo Input System
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

                // 3. Detectar el Clic Izquierdo con el Nuevo Input System
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
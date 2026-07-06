using UnityEngine;
using UnityEngine.Events; // Permite asignar funciones desde el Inspector de Unity

// Este script hereda de MonoBehaviour y usa nuestra interfaz IInteractable
public class CNCButton : MonoBehaviour, IInteractable
{
    [Header("Conexión con la Máquina")]
    // Esto te permitirá arrastrar en Unity qué función del MachineStateManager llamar
    public UnityEvent onClickAction; 

    [Header("Feedback Visual")]
    public Material normalMaterial;
    public Material highlightMaterial;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.material = normalMaterial;
    }

    // --- Implementación de la Interfaz ---

    public void OnHoverEnter()
    {
        // Cambiar el material o el cursor para indicar que se puede hacer clic
        if (meshRenderer != null && highlightMaterial != null)
            meshRenderer.material = highlightMaterial;
    }

    public void OnHoverExit()
    {
        // Volver al material normal
        if (meshRenderer != null && normalMaterial != null)
            meshRenderer.material = normalMaterial;
    }

    public void Interact()
    {
        // Ejecutar la acción configurada en el Inspector
        // (Ej: Llamar a manager.OnCycleStartPresionado() )
        onClickAction?.Invoke(); 
        
        // Aquí podrías agregar un sonido de "Click" industrial
        Debug.Log("Se hizo clic en el botón: " + gameObject.name);
    }
}
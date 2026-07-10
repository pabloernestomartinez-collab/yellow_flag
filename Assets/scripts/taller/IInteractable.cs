using UnityEngine;

// No hereda de MonoBehaviour. Es solo una interfaz.
public interface IInteractable
{
    // Qué pasa cuando el mouse pasa por encima (para iluminar el botón)
    void OnHoverEnter(); 
    
    // Qué pasa cuando el mouse sale del botón
    void OnHoverExit();  
    
    // Qué pasa cuando el usuario hace clic izquierdo
    void Interact();     
}
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode; // Sincronización con el entorno de red de Yellow Flag

public class ControlCamaraCNC : MonoBehaviour
{
    [Header("Velocidades de Movimiento")]
    public float velocidadMovimiento = 5.0f;
    public float sensibilidadMouse = 0.2f;

    [Header("Límites de Rotación")]
    public float limiteVerticalMinimo = -60f;
    public float limiteVerticalMaximo = 60f;

    private Vector3 rotacionActual;
    private bool estaRotando = false;

    void Start()
    {
        // Inicializamos la rotación con la orientación que ya tenga la cámara en la escena
        rotacionActual = transform.localEulerAngles;

        // Corrección por si Unity devuelve valores mayores a 180 grados
        if (rotacionActual.x > 180) rotacionActual.x -= 360;
    }

    void Update()
    {
        // ------------------------------------------------------------------------
        // REGLA DE RED OPTIONAL: Si en el futuro quieres congelar al alumno, 
        // podrías usar una verificación aquí. Por ahora ambos pueden mirar el taller.
        // ------------------------------------------------------------------------

        ProcesarRotacion();
        ProcesarMovimiento();
    }

    private void ProcesarRotacion()
    {
        if (Mouse.current == null) return;

        // Activamos la rotación SOLO cuando se mantiene presionado el Clic Derecho
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            estaRotando = true;
            Cursor.lockState = CursorLockMode.Locked; // Oculta el cursor para no salirse de la pantalla
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            estaRotando = false;
            Cursor.lockState = CursorLockMode.None; // Devuelve el cursor para poder usar la mira del CNC
        }

        // Si el usuario está sosteniendo el clic derecho, rotamos la cámara
        if (estaRotando)
        {
            Vector2 deltaMouse = Mouse.current.delta.ReadValue();

            // Calculamos los ejes (Mouse X mueve el giro horizontal, Mouse Y el vertical)
            rotacionActual.x -= deltaMouse.y * sensibilidadMouse;
            rotacionActual.y += deltaMouse.x * sensibilidadMouse;

            // Ponemos un tope vertical para que el operador no pueda dar vueltas en 360 estilo voltereta
            rotacionActual.x = Mathf.Clamp(rotacionActual.x, limiteVerticalMinimo, limiteVerticalMaximo);

            // Aplicamos la rotación final a la cámara
            transform.localRotation = Quaternion.Euler(rotacionActual.x, rotacionActual.y, 0f);
        }
    }

    private void ProcesarMovimiento()
    {
        if (Keyboard.current == null) return;

        Vector3 direccionDeMovimiento = Vector3.zero;

        // Captura de teclas de dirección clásicas (WASD o Flechas)
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            direccionDeMovimiento += transform.forward;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            direccionDeMovimiento -= transform.forward;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            direccionDeMovimiento -= transform.right;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            direccionDeMovimiento += transform.right;

        // Movimiento vertical opcional (Q para bajar, E para subir en el espacio)
        if (Keyboard.current.eKey.isPressed) direccionDeMovimiento += Vector3.up;
        if (Keyboard.current.qKey.isPressed) direccionDeMovimiento += Vector3.down;

        // Aplicamos el desplazamiento final normalizado para que no camine más rápido en diagonal
        transform.position += direccionDeMovimiento.normalized * velocidadMovimiento * Time.deltaTime;
    }
}

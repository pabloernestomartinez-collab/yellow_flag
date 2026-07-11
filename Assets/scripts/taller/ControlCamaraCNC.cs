using UnityEngine;
using UnityEngine.InputSystem;

// Estas líneas obligan a Unity a poner los componentes físicos necesarios automáticamente
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
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
    private Rigidbody rb;

    void Start()
    {
        rotacionActual = transform.localEulerAngles;
        if (rotacionActual.x > 180) rotacionActual.x -= 360;

        // Configuramos los parámetros físicos por código para evitar errores
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;                  // La cámara no debe caerse al vacío
        rb.isKinematic = false;                 // Debe ser falsa para que las colisiones la detengan
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Suaviza el movimiento físico

        // Bloqueamos las rotaciones físicas para que los golpes no hagan girar la cámara como loca
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Ajustamos el colisionador de la cámara
        SphereCollider col = GetComponent<SphereCollider>();
        col.radius = 0.2f;                      // Un tamaño pequeño para que pueda acercarse al panel CNC
        col.isTrigger = false;
    }

    void Update()
    {
        ProcesarRotacion();
    }

    void FixedUpdate()
    {
        // El movimiento físico SIEMPRE debe ejecutarse en FixedUpdate para evitar que traspase objetos a altos FPS
        ProcesarMovimientoFisico();
    }

    private void ProcesarRotacion()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            estaRotando = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            estaRotando = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (estaRotando)
        {
            Vector2 deltaMouse = Mouse.current.delta.ReadValue();

            rotacionActual.x -= deltaMouse.y * sensibilidadMouse;
            rotacionActual.y += deltaMouse.x * sensibilidadMouse;
            rotacionActual.x = Mathf.Clamp(rotacionActual.x, limiteVerticalMinimo, limiteVerticalMaximo);

            transform.localRotation = Quaternion.Euler(rotacionActual.x, rotacionActual.y, 0f);
        }
    }

    private void ProcesarMovimientoFisico()
    {
        if (Keyboard.current == null) return;

        Vector3 direccionDeMovimiento = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            direccionDeMovimiento += transform.forward;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            direccionDeMovimiento -= transform.forward;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            direccionDeMovimiento -= transform.right;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            direccionDeMovimiento += transform.right;

        if (Keyboard.current.eKey.isPressed) direccionDeMovimiento += Vector3.up;
        if (Keyboard.current.qKey.isPressed) direccionDeMovimiento += Vector3.down;

        // En lugar de mover el "transform.position" directamente (lo cual ignora colisiones),
        // calculamos la velocidad del Rigidbody para que Unity valide los impactos en el camino.
        Vector3 velocidadObjetivo = direccionDeMovimiento.normalized * velocidadMovimiento;
        rb.linearVelocity = velocidadObjetivo;
    }
}

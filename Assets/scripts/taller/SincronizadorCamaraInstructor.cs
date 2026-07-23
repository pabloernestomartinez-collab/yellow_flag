using UnityEngine;
using Unity.Netcode;

public class SincronizadorCamaraInstructor : NetworkBehaviour
{
    [Header("Configuración de Suavizado")]
    public float velocidadSuavizado = 15f;

    // Variables de red para sincronizar Posición y Rotación en tiempo real de Host a Clientes
    private NetworkVariable<Vector3> redPosicionCamara = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone
    );

    private NetworkVariable<Quaternion> redRotacionCamara = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone
    );

    private void Update()
    {
        // 1. SI ES EL INSTRUCTOR (Servidor/Host): Transmite su posición y rotación a la red
        if (IsServer)
        {
            redPosicionCamara.Value = transform.position;
            redRotacionCamara.Value = transform.rotation;
        }
        // 2. SI ES UN ALUMNO (Cliente): Recibe las coordenadas y mueve su cámara para copiar al Instructor
        else
        {
            // Lerp para que el movimiento de la cámara en la pantalla del alumno sea fluido y no dé saltos
            transform.position = Vector3.Lerp(transform.position, redPosicionCamara.Value, Time.deltaTime * velocidadSuavizado);
            transform.rotation = Quaternion.Slerp(transform.rotation, redRotacionCamara.Value, Time.deltaTime * velocidadSuavizado);
        }
    }
}
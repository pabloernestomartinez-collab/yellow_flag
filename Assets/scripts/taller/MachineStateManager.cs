using UnityEngine;

public class MachineStateManager : MonoBehaviour
{
    // Definimos los estados lógicos de la máquina
    public enum MachineState
    {
        Apagado,            // Pantalla negra, sin energía.
        ParadaDeEmergencia, // Energía ON, pero bloqueada por la Seta de Emergencia.
        Modo_JOG,           // E-Stop liberado, modo manual (perilla en JOG).
        Modo_AUTO,          // Máquina lista para leer Código G (perilla en AUTO).
        Ejecutando,         // Programa corriendo (Cycle Start presionado).
        Alarma              // Bandera Roja o Amarilla (ej. puerta abierta).
    }

    [Header("Estado Actual")]
    public MachineState currentState = MachineState.Apagado;

    [Header("Sensores Físicos")]
    public bool isChuckClosed = false;
    public bool isDoorClosed = false;

    // Referencias a otros sistemas (UI, Animaciones, etc.)
    // public ScreenController screenController;
    
    // ----------------------------------------------------
        // METODOS LLAMADOS POR LOS CLICS DEL USUARIO
        // ----------------------------------------------------
    
        public void OnBotonEncendidoPresionado()
        {
            if (currentState == MachineState.Apagado)
            {
                // Transición de estado
                currentState = MachineState.ParadaDeEmergencia;
                Debug.Log("Máquina Encendida. Seta de emergencia activa por defecto.");
                // Aquí llamarías al script de la pantalla para que se encienda
            }
        }
    
        public void OnSetaEmergenciaLiberada()
        {
            if (currentState == MachineState.ParadaDeEmergencia)
            {
                // Asumimos que la perilla por defecto está en JOG
                currentState = MachineState.Modo_JOG;
                Debug.Log("E-Stop liberado. Máquina lista en modo JOG.");
            }
        }
    
        public void OnPedalMordazaPresionado()
        {
            // Solo podemos abrir/cerrar mordazas si NO estamos mecanizando
            if (currentState != MachineState.Ejecutando && currentState != MachineState.Apagado)
            {
                isChuckClosed = !isChuckClosed;
                Debug.Log(isChuckClosed ? "Mordaza Cerrada" : "Mordaza Abierta");
            }
            else
            {
                Debug.LogWarning("BLOQUEADO: No puedes operar la mordaza en este estado.");
            }
        }
    
        public void OnCycleStartPresionado()
        {
            // Regla de seguridad estricta: Solo arranca si está en AUTO y seguro
            if (currentState == MachineState.Modo_AUTO)
            {
                if (!isChuckClosed)
                {
                    DispararAlarma("ALARM: CHUCK UNCLAMPED");
                    return; // Corta la ejecución aquí
                }
    
                if (!isDoorClosed)
                {
                    DispararAlarma("MSG: CERRAR PUERTA ANTES DE AUTO");
                    return; // Corta la ejecución aquí
                }
    
                // Si pasa todas las reglas de seguridad:
                currentState = MachineState.Ejecutando;
                Debug.Log("*** RUN *** Programa iniciado.");
                // Aquí llamarías al script que lee el Código G
            }
            else
            {
                Debug.LogWarning("BLOQUEADO: Cycle Start solo funciona en modo AUTO.");
            }
        }
    
        // ----------------------------------------------------
        // SISTEMA DE ALARMAS (BANDERAS)
        // ----------------------------------------------------
        private void DispararAlarma(string mensajeError)
        {
            currentState = MachineState.Alarma;
            Debug.LogError("YELLOW FLAG: " + mensajeError);
            // Aquí mandarías el texto a la línea roja de la pantalla del Fanuc
        }
}


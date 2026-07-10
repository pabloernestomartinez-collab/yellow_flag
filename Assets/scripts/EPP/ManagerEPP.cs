using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ManagerEPP : NetworkBehaviour
{
    [Header("Validación de Seguridad")]
    // Marcamos cuáles son obligatorios para este taller (ej: Gafas y Botas)
    public bool requiereGafas = true;
    public bool requiereBotas = true;
    public bool requiereCasco = false; // El torno bajo techo quizás no lo requiera
    public bool requiereFajaLumbar = false; 
    public bool requiereProtectorAuditivo = true; 
    public bool requiereChaleco = false; // El torno bajo techo quizás no lo requiera
    public bool requiereGuantes = true; 



    // Variables de red sincronizadas automáticamente de Host a Clientes
    private NetworkVariable<bool> tieneGafas = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneBotas = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneCasco = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneFajaLumbar = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneProtectorAuditivo = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneChaleco = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> tieneGuantes = new NetworkVariable<bool>(false);

    // Esto se ejecutará en OnGUI para mostrar el estado a todos
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 400, 500));

        GUILayout.Label("=== CONTROL DE INGRESO: PROTOCOLO EPP ===");
        GUILayout.Label("El Instructor debe validar el equipo de protección requerido.");
        GUILayout.Space(20);

        // 1. Mostrar los elementos y su estado actual en red
        MostrarElementoUI("Gafas de Seguridad", tieneGafas.Value, requiereGafas, 1);
        MostrarElementoUI("Botas de seguridad", tieneBotas.Value, requiereBotas, 2);
        MostrarElementoUI("Casco de Protección", tieneCasco.Value, requiereCasco, 3);
        MostrarElementoUI("Faja Lumbar", tieneFajaLumbar.Value, requiereFajaLumbar, 4);
        MostrarElementoUI("Protector auditivo", tieneProtectorAuditivo.Value, requiereProtectorAuditivo, 5);
        MostrarElementoUI("Chaleco Refractario", tieneChaleco.Value, requiereChaleco, 6);
        MostrarElementoUI("Guantes de seguridad", tieneGuantes.Value, requiereGuantes, 7);

        GUILayout.Space(30);

        // 2. Verificar si el protocolo está completo
        bool protocoloValido = ValidarProtocolo();

        if (protocoloValido)
        {
            GUI.backgroundColor = Color.green;
            GUILayout.Box("✓ PROTOCOLO DE SEGURIDAD APROBADO");
        }
        else
        {
            GUI.backgroundColor = Color.red;
            GUILayout.Box("✗ EQUIPAMIENTO INSUFICIENTE PARA INGRESAR AL TALLER");
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(20);

        // 3. Botón de acceso exclusivo para el Instructor
        if (IsServer)
        {
            GUI.enabled = protocoloValido; // Solo se puede clickear si el protocolo es válido
            if (GUILayout.Button("ABRIR ACCESO AL TALLER CNC", GUILayout.Height(40)))
            {
                // Sincroniza la carga de la escena final para todos
                NetworkManager.Singleton.SceneManager.LoadScene("TallerCNC", LoadSceneMode.Single);
            }
            GUI.enabled = true;
        }
        else
        {
            GUILayout.Label("Esperando que el Instructor verifique el EPP regulamentario...");
        }

        GUILayout.EndArea();
    }

    private void MostrarElementoUI(string nombre, bool seleccionado, bool esObligatorio, int ID_Elemento)
    {
        GUILayout.BeginHorizontal();

        // Indicador visual de estado
        string prefijo = seleccionado ? "[ X ] " : "[   ] ";
        GUILayout.Label(prefijo + nombre);

        // Solo el Instructor puede hacer clic para alternar los elementos
        if (IsServer)
        {
            if (GUILayout.Button(seleccionado ? "Quitar" : "Colocar", GUILayout.Width(80)))
            {
                AlternarElementoServer(ID_Elemento);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    // El servidor cambia el valor de las NetworkVariables y estas se replican solas en los clientes
    private void AlternarElementoServer(int id)
    {
        if (id == 1) tieneGafas.Value = !tieneGafas.Value;
        if (id == 2) tieneBotas.Value = !tieneBotas.Value;
        if (id == 3) tieneCasco.Value = !tieneCasco.Value;
        if (id == 4) tieneFajaLumbar.Value = !tieneFajaLumbar.Value;
        if (id == 5) tieneProtectorAuditivo.Value = !tieneProtectorAuditivo.Value;
        if (id == 6) tieneChaleco.Value = !tieneChaleco.Value;
        if (id == 7) tieneGuantes.Value = !tieneGuantes.Value;
    }

    private bool ValidarProtocolo()
    {
        // Si requiere un elemento y no está seleccionado, el protocolo falla
        if (requiereGafas && !tieneGafas.Value) return false;
        if (requiereBotas && !tieneBotas.Value) return false;
        if (requiereCasco && !tieneCasco.Value) return false;
        if (requiereFajaLumbar && !tieneFajaLumbar.Value) return false;
        if (requiereProtectorAuditivo && !tieneProtectorAuditivo.Value) return false;
        if (requiereChaleco && !tieneChaleco.Value) return false;
        if (requiereGuantes && !tieneGuantes.Value) return false;

        return true;
    }
}
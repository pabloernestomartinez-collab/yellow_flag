using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class LobbyYellowFlag : MonoBehaviour
{
    private string ipServidor = "127.0.0.1";

    private bool hostDetectado = false;
    private bool buscandoHost = false;
    private string mensajeEstado = "Seleccione la modalidad de la sesión para comenzar.";

    private void Start()
    {
        hostDetectado = false;
        buscandoHost = false;
        mensajeEstado = "Seleccione la modalidad de la sesión para comenzar.";

        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarseDelServidor;
            NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectarseDelServidor;

            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseConExito;
            NetworkManager.Singleton.OnClientConnectedCallback += AlConectarseConExito;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarseDelServidor;
            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseConExito;
        }
    }

    private void AlConectarseConExito(ulong id)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            hostDetectado = true;
            buscandoHost = false;
            mensajeEstado = "¡Conexión establecida con el Aula Virtual del Instructor!";
        }
    }

    private void AlDesconectarseDelServidor(ulong idCliente)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsServer) return;

        if (buscandoHost)
        {
            hostDetectado = false;
            buscandoHost = false;
            mensajeEstado = "La estación del Instructor no está activa en la IP indicada.";
            return;
        }

        if (SceneManager.GetActiveScene().name == "MenuPrincipal")
        {
            hostDetectado = false;
            buscandoHost = false;
            mensajeEstado = "Sesión finalizada de forma correcta. Seleccione modalidad.";
            return;
        }

        NetworkManager.Singleton.Shutdown();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator ComprobarSiExisteHost()
    {
        if (NetworkManager.Singleton == null) yield break;

        hostDetectado = false;
        buscandoHost = true;
        mensajeEstado = "Localizando estación del Instructor en la red local...";

        ConfigurarIpTransporte(ipServidor);
        NetworkManager.Singleton.StartClient();

        float tiempoEspera = 0f;
        while (tiempoEspera < 4f && !hostDetectado)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                hostDetectado = true;
                buscandoHost = false;
                mensajeEstado = "¡Terminal de Instructor detectada! Sincronizando entorno...";
                yield break;
            }

            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        if (!hostDetectado)
        {
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            buscandoHost = false;
            mensajeEstado = "Error de enlace: El Instructor no ha iniciado la sesión o la IP es inválida.";
        }
    }

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        // ESTADO 1: MENÚ DE CONEXIÓN INICIAL
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            GUILayout.BeginArea(new Rect(20, 20, 350, 450));

            GUILayout.Label("SOFTWARE DE CAPACITACIÓN INDUSTRIAL");
            GUILayout.Label("PROGRAMA: YELLOW FLAG - SIMULADOR CNC");
            GUILayout.Box($"SISTEMA: {mensajeEstado}");
            GUILayout.Space(20);

            if (GUILayout.Button("Iniciar como Terminal INSTRUCTOR (Host)"))
            {
                NetworkManager.Singleton.StartHost();
            }

            GUILayout.Space(20);
            GUILayout.Label("Dirección IP de la Terminal del Instructor:");
            ipServidor = GUILayout.TextField(ipServidor, 30);

            if (!hostDetectado && !buscandoHost)
            {
                if (GUILayout.Button("Verificar Conexión con Instructor"))
                {
                    StartCoroutine(ComprobarSiExisteHost());
                }
            }

            if (hostDetectado && !buscandoHost)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Vincular como Terminal OPERARIO (Cliente)"))
                {
                    ConfigurarIpTransporte(ipServidor);
                    NetworkManager.Singleton.StartClient();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(30);
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("Salir a Windows"))
            {
                StartCoroutine(CierreOrdenadoMenu());
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndArea();
        }
        // ESTADO 2: LOBBY DE ENLACE ACTIVO (CUENTA DE CONECTADOS)
        else
        {
            GUILayout.BeginArea(new Rect(20, 20, 350, 300));

            // Llevamos la cuenta en tiempo real consultando la lista interna de Netcode
            int totalConectados = NetworkManager.Singleton.ConnectedClients.Count;
            int cantidadAlumnos = totalConectados - 1; // Restamos 1 para no contarte a ti mismo (Host)

            GUILayout.Label("=== TELEMETRÍA DE LA SESIÓN ===");
            GUILayout.Label($"Total de terminales en red: {totalConectados}");
            GUILayout.Label($"Alumnos/Observadores en línea: {(cantidadAlumnos < 0 ? 0 : cantidadAlumnos)}");
            GUILayout.Space(15);

            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label("Estado: Listo para iniciar la instrucción técnica.");
                GUILayout.Space(10);

                // El botón ahora está liberado; no depende de ninguna condición de cantidad
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("INICIAR SIMULACIÓN TÉCNICA"))
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("EPP", LoadSceneMode.Single);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label("Estado: Sincronizado en modo OBSERVADOR.");
                GUILayout.Box("Aguardando la orden del Instructor para desplegar el taller...");
            }
            GUILayout.EndArea();
        }
    }

    private IEnumerator CierreOrdenadoMenu()
    {
        mensajeEstado = "Cerrando sistemas de red...";

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        yield return null;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ConfigurarIpTransporte(string nuevaIp)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.gameObject.TryGetComponent<UnityTransport>(out UnityTransport transporte))
        {
            transporte.ConnectionData.Address = nuevaIp.Trim();
        }
    }
}
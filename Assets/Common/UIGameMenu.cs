using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starter
{
    /// <summary>
    /// Shows in-game menu, handles player connecting/disconnecting to the network game and cursor locking.
    /// </summary>
    public class UIGameMenu : MonoBehaviour
    {
        [Header("Start Game Setup")]
        [Tooltip("Specifies which game mode player should join - e.g. Platformer, ThirdPersonCharacter")]
        public string GameModeIdentifier;
        public NetworkRunner RunnerPrefab;
        public int MaxPlayerCount = 8;

        [Header("Debug")]
        [Tooltip("For debug purposes it is possible to force single-player game (starts faster)")]
        public bool ForceSinglePlayer;

        [Header("UI Setup")]
        public CanvasGroup PanelGroup;
        public TMP_InputField RoomText;
        public TMP_InputField NicknameText;
        public TextMeshProUGUI StatusText;
        public GameObject StartGroup;
        public GameObject DisconnectGroup;

        private NetworkRunner _runnerInstance;
        private static string _shutdownStatus;

        public async void StartGame()
        {
            await Disconnect();

            ChatManager.Instance.Initialize(NicknameText.text);

            _runnerInstance = Instantiate(RunnerPrefab);

            // Add listener for shutdowns so we can handle unexpected shutdowns
            var events = _runnerInstance.GetComponent<NetworkEvents>();
            events.OnShutdown.AddListener(OnShutdown);

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

            var startArguments = new StartGameArgs()
            {
                GameMode = Application.isEditor && ForceSinglePlayer ? GameMode.Single : GameMode.Shared,
                SessionName = RoomText.text,
                PlayerCount = MaxPlayerCount,
                SessionProperties = new Dictionary<string, SessionProperty> { ["GameMode"] = GameModeIdentifier },
                Scene = sceneInfo,
            };

            StatusText.text = startArguments.GameMode == GameMode.Single ? "Starting single-player..." : "Connecting...";

            var startTask = _runnerInstance.StartGame(startArguments);
            await startTask;

            if (startTask.Result.Ok)
            {
                StatusText.text = "";
                PanelGroup.gameObject.SetActive(false);
            }
            else
            {
                StatusText.text = $"Connection Failed: {startTask.Result.ShutdownReason}";
            }
        }

        public async void DisconnectClicked()
        {
            await Disconnect();
        }

        public async void BackToMenu()
        {
            await Disconnect();
            SceneManager.LoadScene(1);
        }

        public void TogglePanelVisibility()
        {
            if (PanelGroup.gameObject.activeSelf && _runnerInstance == null)
                return; // Panel cannot be hidden if the game is not running

            PanelGroup.gameObject.SetActive(!PanelGroup.gameObject.activeSelf);
        }

        private void OnEnable()
        {
            string nickname = null;

            if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.playerName))
            {
                nickname = GameManager.Instance.playerName;
                NicknameText.interactable = false; // Khóa không cho chỉnh nếu đã có tên
            }
            else
            {
                nickname = "Player" + UnityEngine.Random.Range(10000, 99999);
                NicknameText.interactable = true;
            }

            NicknameText.text = nickname;
            StatusText.text = _shutdownStatus ?? string.Empty;
            _shutdownStatus = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePanelVisibility();
            }

            if (PanelGroup.gameObject.activeSelf)
            {
                StartGroup.SetActive(_runnerInstance == null);
                DisconnectGroup.SetActive(_runnerInstance != null);
                RoomText.interactable = _runnerInstance == null;
                // NicknameText.interactable đã được xử lý trong OnEnable
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public async Task Disconnect()
        {
            if (_runnerInstance == null)
                return;

            StatusText.text = "Disconnecting...";
            PanelGroup.interactable = false;

            var events = _runnerInstance.GetComponent<NetworkEvents>();
            events.OnShutdown.RemoveListener(OnShutdown);

            await _runnerInstance.Shutdown();
            _runnerInstance = null;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            _shutdownStatus = $"Shutdown: {reason}";
            Debug.LogWarning(_shutdownStatus);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

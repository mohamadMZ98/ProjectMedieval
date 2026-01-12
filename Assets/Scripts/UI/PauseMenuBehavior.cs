using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject pausePanel;       // main pause menu panel
    [SerializeField] private GameObject settingsPanel;    // settings panel
   

    [Header("Pause Behavior")]
    [SerializeField] private bool pauseAudio = true;
    [SerializeField] private bool freezeTime = true;
    [SerializeField] private bool unlockCursorOnPause = false;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu Scene";

    public bool IsPaused { get; private set; }

    float previousTimeScale = 1f;
    bool previousAudioPaused = false;

    private void Awake()
    {
        Debug.Log("PauseManager Awake on: " + gameObject.name, this);
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // If you want it to persist across scenes, uncomment:
        // DontDestroyOnLoad(gameObject);

        // Defensive UI init
        //if(pauseRoot != null) pauseRoot.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Start()
    {
        Debug.Log("PauseManager Start. pauseRoot = " + (pauseRoot ? pauseRoot.name : "NULL"), this);

        if (pauseRoot != null)
            pauseRoot.SetActive(false);
    }

    private void Update()
    {
        Debug.Log("Pause update running");
        // Example keybind: Escape toggles pause
        // Replace with your input system hook if you use the new Input System
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Pausing Game");
            if (pauseRoot == null)
            {
                Debug.LogError("pauseRoot is NULL - drag your Canvas into the PauseManager field!", this);
                return;
            }
            if (!IsPaused) Pause();
            else Resume();
        }
    }

    // =======================
    // Core Pause Controls
    // =======================

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;

        // UI
        if (pauseRoot != null) pauseRoot.SetActive(true);
        ShowPausePanel();

        // Freeze time
        if (freezeTime)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // Pause audio
        if (pauseAudio)
        {
            previousAudioPaused = AudioListener.pause;
            AudioListener.pause = true;
        }

        if (unlockCursorOnPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;

        if(pauseRoot != null)
            pauseRoot.SetActive(false);

        // Restore time
        if (freezeTime)
            Time.timeScale = previousTimeScale;

        // Restore audio pause state
        if (pauseAudio)
            AudioListener.pause = previousAudioPaused;
    }

    // =======================
    // Button Hook Methods
    // =======================

    // Resume button
    public void OnResumePressed()
    {
        Resume();
    }

    // Settings button
    public void OnSettingsPressed()
    {
        ShowSettingsPanel();
    }

    // Back button in settings
    public void OnBackFromSettingsPressed()
    {
        ShowPausePanel();
    }

    // Quit button (choose confirm panel or direct)
    public void OnQuitPressed()
    {
        QuitToMainMenu();
    }

   

    private void ShowPausePanel()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void ShowSettingsPanel()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    

    // =======================
    // Quit Logic
    // =======================

    private void QuitToMainMenu()
    {
        // Important: restore time/audio BEFORE leaving scene
        if (freezeTime) Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        IsPaused = false;

        SceneManager.LoadScene("MainMenuScene");
    }
}


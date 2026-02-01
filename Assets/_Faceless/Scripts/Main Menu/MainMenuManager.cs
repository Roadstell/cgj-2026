using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Botones del Menú")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;

    [Header("Configuración de Escenas")]
    [SerializeField] private string scene1Name = "Scene1"; // Nombre de la escena a cargar
    [SerializeField] private int scene1BuildIndex = 1; // Índice en Build Settings

    [Header("Efecto de Fundido")]
    [SerializeField] private CanvasGroup fadeOverlay; // CanvasGroup para el fade
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Menú de Opciones")]
    [SerializeField] private GameObject optionsMenuPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Button closeOptionsButton;
    [SerializeField] private Button applyOptionsButton;

    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource menuMusic;

    private bool isTransitioning = false;

    void Start()
    {
        // Configurar listeners de botones
        playButton.onClick.AddListener(OnPlayButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        closeOptionsButton.onClick.AddListener(CloseOptionsMenu);
        applyOptionsButton.onClick.AddListener(ApplyOptions);

        // Configurar menú de opciones
        if (optionsMenuPanel != null)
            optionsMenuPanel.SetActive(false);

        // Inicializar valores del menú de opciones
        InitializeOptionsMenu();

        // Configurar fade overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false);
            fadeOverlay.alpha = 0f;
        }

        // Reproducir música de menú
        if (menuMusic != null && !menuMusic.isPlaying)
            menuMusic.Play();
    }

    #region Funciones de Botones Principales

    private void OnPlayButtonClicked()
    {
        if (isTransitioning) return;

        PlayButtonSound();
        StartCoroutine(LoadSceneWithFade());
    }

    private void OnOptionsButtonClicked()
    {
        PlayButtonSound();
        OpenOptionsMenu();
    }

    private void OnExitButtonClicked()
    {
        PlayButtonSound();
        ExitGame();
    }

    #endregion

    #region Sistema de Fundido y Carga de Escena

    private IEnumerator LoadSceneWithFade()
    {
        isTransitioning = true;

        // Activar y hacer fade in
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        // Pausa adicional para mostrar el fundido completo
        yield return new WaitForSeconds(0.5f);

        // Cargar la escena
        LoadScene1();
    }

    private void LoadScene1()
    {
        // Intentar cargar por nombre primero, luego por índice
        if (!string.IsNullOrEmpty(scene1Name))
        {
            SceneManager.LoadScene(scene1Name);
        }
        else
        {
            SceneManager.LoadScene(scene1BuildIndex);
        }
    }

    #endregion

    #region Menú de Opciones

    private void OpenOptionsMenu()
    {
        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(true);
            UpdateOptionsDisplay();
        }
    }

    private void CloseOptionsMenu()
    {
        PlayButtonSound();

        if (optionsMenuPanel != null)
        {
            optionsMenuPanel.SetActive(false);
        }
    }

    private void InitializeOptionsMenu()
    {
        // Configurar música
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Configurar efectos de sonido
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.9f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Configurar pantalla completa
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // Configurar calidad gráfica
        if (qualityDropdown != null)
        {
            // Obtener nombres de calidades
            string[] qualityNames = QualitySettings.names;
            qualityDropdown.ClearOptions();

            foreach (string name in qualityNames)
            {
                qualityDropdown.options.Add(new Dropdown.OptionData(name));
            }

            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    private void UpdateOptionsDisplay()
    {
        // Actualizar UI con valores actuales
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.9f);

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;

        if (qualityDropdown != null)
            qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    private void ApplyOptions()
    {
        PlayButtonSound();

        // Guardar configuraciones
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.Save();

        CloseOptionsMenu();
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (menuMusic != null)
            menuMusic.volume = value;
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (buttonClickSound != null)
            buttonClickSound.volume = value;
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    #endregion

    #region Salida del Juego

    private void ExitGame()
    {
#if UNITY_EDITOR
        // Si estamos en el editor, detener la reproducción
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // En build final, salir de la aplicación
            Application.Quit();
#endif
    }

    #endregion

    #region Utilidades

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
            buttonClickSound.Play();
    }

    // Para control desde teclado
    void Update()
    {
        // Atajos de teclado
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayButtonClicked();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuPanel != null && optionsMenuPanel.activeSelf)
            {
                CloseOptionsMenu();
            }
            else
            {
                ExitGame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            OpenOptionsMenu();
        }
    }

    #endregion
}
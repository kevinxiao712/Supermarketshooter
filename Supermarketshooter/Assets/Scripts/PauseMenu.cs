using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;  // If you're using Netcode for GameObjects

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;  // The Panel that shows the pause menu
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;

    [Header("Sensitivity Settings")]
    public float sensitivityMin = 0.1f;
    public float sensitivityMax = 600f;

    // Keeps track of whether the menu is currently open
    private bool menuOpen = true;

    // We'll fill these once we find a local player
    private PlayerCam playerCam;
    private Playermovement playerMovement;

    void Start()
    {
        // Make sure the pause menu starts hidden if we want it closed at the beginning
        pauseMenuPanel.SetActive(menuOpen);

        // If the menu is closed, lock and hide the cursor
        if (!menuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Optional: set up slider ranges in code (you can also do this in the Inspector)
        sensitivityXSlider.minValue = sensitivityMin;
        sensitivityXSlider.maxValue = sensitivityMax;
        sensitivityYSlider.minValue = sensitivityMin;
        sensitivityYSlider.maxValue = sensitivityMax;

        // (We haven't found the player yet, since they spawn later.)
        // So the sliders won't do anything until we find the local player.

        // Add listeners for the sliders
        sensitivityXSlider.onValueChanged.AddListener(OnSensitivityXChanged);
        sensitivityYSlider.onValueChanged.AddListener(OnSensitivityYChanged);
    }

    void Update()
    {

        if (playerCam == null || playerMovement == null)
        {
            TryFindLocalPlayer();
        }


        if (playerMovement != null && Input.GetKeyDown(KeyCode.Escape))
        {
            menuOpen = !menuOpen;
            ToggleMenu(menuOpen);
        }
    }

    private void TryFindLocalPlayer()
    {
        // Look for any PlayerCam in the scene
        var cams = FindObjectsByType<PlayerCam>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (cam.IsOwner) 
            {
                playerCam = cam;
                break;
            }
        }


        var moves = FindObjectsByType<Playermovement>(FindObjectsSortMode.None);
        foreach (var move in moves)
        {
            if (move.IsOwner)
            {
                playerMovement = move;
                break;
            }
        }


        // If we found both, initialize the sliders to the current sensitivity
        if (playerCam != null && playerMovement != null)
        {
            sensitivityXSlider.value = playerCam.sensX;
            sensitivityYSlider.value = playerCam.sensY;

            Debug.Log("Local player references found! PauseMenu is now linked.");
        }
    }

    /// <summary>
    /// Toggles pause menu UI and locks/unlocks movement & cursor.
    /// </summary>
    private void ToggleMenu(bool show)
    {
        pauseMenuPanel.SetActive(show);

        // If we have a valid local player script, enable/disable movement
        if (playerMovement != null)
        {
            // If menu is open, block movement
            playerMovement.canMove = !show;
        }

        if (show)
        {
            // Show mouse cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Hide mouse cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Called when the X slider¡¯s value changes.
    /// </summary>
    private void OnSensitivityXChanged(float newValue)
    {
        if (playerCam != null)
        {
            playerCam.sensX = newValue;
        }
    }

    /// <summary>
    /// Called when the Y slider¡¯s value changes.
    /// </summary>
    private void OnSensitivityYChanged(float newValue)
    {
        if (playerCam != null)
        {
            playerCam.sensY = newValue;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerControllerBehaviour player;

    [SerializeField]
    private GameObject pauseMenu = default;

    [SerializeField]
    bool gamePaused = false;

    [SerializeField]
    private Image healthSlider;

    [SerializeField]
    private Image staminaSlider;

    public PlayerControllerBehaviour Player { get => player; set => player = value; }

    void Update()
    {
        healthSlider.fillAmount = player.Health / 100;
        staminaSlider.fillAmount = player.Stamina / 100;

        if (Input.GetButtonDown("Cancel"))
        {
            gamePaused = togglePause();
        }
    }

    public bool togglePause()
    {
        if (Time.timeScale == 0f) //Unpause
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            return false;
        }
        else //Pause
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            return true;
        }
    }

    public void pause()
    {
        gamePaused = togglePause();
    }

    public void Quit()
    {
        Application.Quit();
    }
}

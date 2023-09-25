using System.Collections;
using System.Collections.Generic;
using ABMU.Core;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ProgramController : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject SimulationInizializer;
    public GameObject SimulationController;
    private WorldController controller;
    private Scientist scientist;

    public int citizens = 0;
    private int criticalNodes = 0;
    public int citizensWithFriends = 0;

    [SerializeField] private Slider sliderCiudadanos;
    [SerializeField] private Slider sliderFriendships;

    void Start()
    {
        controller = SimulationController.GetComponent<WorldController>();
        scientist = SimulationInizializer.GetComponent<Scientist>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ExitProgram();
    }

    #region Execution Utils

    public void StartSimulation()
    {
        SimulationInizializer.SetActive(true);
        SimulationController.SetActive(true);
    }

    public void ExitProgram()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            controller.isSimulationPaused = true;
        }
        else
        {
            Time.timeScale = 1f;
            controller.isSimulationPaused = false;
        }
    }

    public void ChangePauseText(Button pauseButton)
    {
        TextMeshProUGUI tmp = 
            pauseButton.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = isPaused ? "Continuar" : "Pausa";
    }
    #endregion

    #region UI Utils

    public IEnumerator Carga()
    {
        Debug.LogError("Cargando");
        int total = 1000;
        sliderCiudadanos.gameObject.SetActive(true);
        sliderFriendships.gameObject.SetActive(true);
        while (citizens < total)
        {
            sliderCiudadanos.value = citizens / total;
            Debug.LogError("Cargando... " + sliderCiudadanos.value);
            yield return null;
        }
        sliderCiudadanos.value = citizens / total;
        Debug.LogError(citizens);
    }

    #endregion
}

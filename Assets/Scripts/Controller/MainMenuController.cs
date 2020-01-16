using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour {
    public GameObject MainPanel;
    public GameObject PlayPanel;
    public GameObject OptionPanel;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            OpenMainPanel();
    }

    public void OpenMainPanel() {
        PlayPanel.SetActive(false);
        OptionPanel.SetActive(false);
        MainPanel.SetActive(true);
    }
    public void OpenPlayPanel() {
        PlayPanel.SetActive(true);
        OptionPanel.SetActive(false);
        MainPanel.SetActive(false);
    }
    public void OpenOptionPanel() {
        PlayPanel.SetActive(false);
        OptionPanel.SetActive(true);
        MainPanel.SetActive(false);
    }
    public void SwitchToEditor() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("EditorScene");
    }

    public void CloseApplication() {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

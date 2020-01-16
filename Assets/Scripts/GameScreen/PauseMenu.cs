using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour {
    public GameObject PauseMenuGO;
    // Start is called before the first frame update
    protected virtual void Start() {
        PauseMenuGO.SetActive(false);
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(PauseMenuGO.activeSelf) {
                ClosePauseMenu();
            } else {
                PauseMenuGO.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    public void ClosePauseMenu() {
        PauseMenuGO.SetActive(false);
        Time.timeScale = 1;
    }
    public void QuitMenu() {
        ClosePauseMenu();
        if(PlayerController.Instance!=null)
            Destroy(PlayerController.Instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitDesktop() {
        Application.Quit();
    }
}

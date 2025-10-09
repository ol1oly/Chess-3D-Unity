using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pauseMenu;

    public GameObject settingsMenu;
    public GameObject PanelInfo;

    public static bool isPaused= false;
    void Start()
    { 
       resumeGame();
       //settingsMenu.SetActive(false);
       //PanelInfo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(isPaused){
                Debug.Log("resume");
                resumeGame();
            }
            else{
                Debug.Log("pause");
                pauseGame();
            }
        }
        
    }
    public void resumeGame(){
        pauseMenu.SetActive(false); 
        Time.timeScale = 1f;
        isPaused=false;
        //Debug.Log("resume");
         Cursor.visible = false;
         Cursor.lockState = CursorLockMode.Locked;
    }
    public void pauseGame(){
        pauseMenu.SetActive(true); 
        Time.timeScale = 0f;
        isPaused=true;
        //Debug.Log("pauser");
         Cursor.visible = true;
         Cursor.lockState = CursorLockMode.None;
    }
    public void mainMenu(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("main_menu");
        
    }
    public void settings(){
        settingsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }
    public void ok(){
        settingsMenu.SetActive(false);
        resumeGame();
    }
    public void info(){
        PanelInfo.SetActive(true);
    }
    public void infoOk(){
        PanelInfo.SetActive(false);
    }
    public void reloadScene(){
         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

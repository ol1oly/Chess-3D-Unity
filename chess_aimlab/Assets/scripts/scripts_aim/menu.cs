using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_MainMenu : MonoBehaviour
{
  

    // Start is called before the first frame update
    void Start()
    {
       
    }
    public void jouerButton()
    {
        // Play Now Button has been pressed, here you can initialize your game (For example Load a Scene called GameLevel etc.)
        UnityEngine.SceneManagement.SceneManager.LoadScene("examen_final");
        
    }
    public void echec()
    {
        // Play Now Button has been pressed, here you can initialize your game (For example Load a Scene called GameLevel etc.)
        UnityEngine.SceneManagement.SceneManager.LoadScene("chess_3D");
        
    }
    public void quitterButton()
    {
        // Quit Game
        Debug.Log("quitter");
        Application.Quit();
    }
}
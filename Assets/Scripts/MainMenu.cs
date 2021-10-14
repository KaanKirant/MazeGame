using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public InputField inputX;
    public InputField inputY;
    public Text alert;
    public void PlayGame()
    {
        if(inputX.text != "")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            Utils.x = int.Parse(inputX.text);
            Utils.y = int.Parse(inputY.text);
        }
        else
        {
            alert.text = "Lütfen X ve Y Değerlerini Giriniz.";
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

}

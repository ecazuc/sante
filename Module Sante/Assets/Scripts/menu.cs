using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public Button guidButton;
    public Button libreButton;
    public static bool mode;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Menu");
        guidButton.onClick.AddListener(() => OnClickButton(0));
        libreButton.onClick.AddListener(() => OnClickButton(1));
    }
    
    public void OnClickButton(int n)
    {
        if (n == 0)
        {
            mode = false;
            Debug.Log("Guidé");
        }
        else
        {
            mode = true;
            Debug.Log("Libre");
        }
        SceneManager.LoadScene("TP5");
        Debug.Log("Chargement de la scène");
    }

    public void guide()
    {
        mode = false;
        Debug.Log("Guidé");
        SceneManager.LoadScene("TP5");
        Debug.Log("Chargement de la scène");
    }

    
}

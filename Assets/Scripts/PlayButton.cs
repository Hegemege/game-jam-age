using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour 
{
    void Awake() 
    {
        
    }

    void Start()
    {
        var button = GetComponent<Button>();

        button.onClick.AddListener(Play);
    }

    private void Play()
    {
        SceneManager.LoadScene("Instructions");
    }

    void Update() 
    {
        
    }
}

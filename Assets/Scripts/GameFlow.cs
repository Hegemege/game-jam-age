using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour 
{
    // Handles game over

    public Image Fade;
    public Button PlayAgainButton;
    public Image Text;

    private TreeScript tree;

    private bool dead;

    public float FadeInTime;
    private float fadeTimer;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start() 
    {
        tree = GameObject.Find("Tree").GetComponent<TreeScript>();

        PlayAgainButton.onClick.AddListener(PlayAgain);
        PlayAgainButton.gameObject.SetActive(false);
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene("Main");
    }
    
    void Update() 
    {
        if (tree.Dead && !dead)
        {
            dead = true;
        }

        if (dead)
        {
            float dt = Time.deltaTime;

            fadeTimer += dt;

            var ratio = Mathf.Clamp(fadeTimer / FadeInTime, 0f, 1f);

            Fade.color = new Color(Fade.color.r, Fade.color.g, Fade.color.b, ratio / 2f);
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, ratio);

            if (fadeTimer > FadeInTime)
            {
                Time.timeScale = 0f;
                PlayAgainButton.gameObject.SetActive(true);
            }
        }
    }
}

// Written by Jay Gunderson
// 07/05/2024
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Text highScoreUI; //
    string newGameScene = "SampleScene";
    
    public AudioSource menuChannel;
    public AudioClip backgroundMusic;

    // Start is called before the first frame update
    void Start()
    {
        menuChannel.PlayOneShot(backgroundMusic);
        int highScore = SaveLoadManager.Instance.LoadHighScore();
        highScoreUI.text = $"Top Wave Surrvived:  {highScore}";
    }

    public void StartNewGame()
    {
        menuChannel.Stop();
        SceneManager.LoadScene(newGameScene);
    }
   
    public void ExitApplication()
    {
#if UNITY_EDITOR // If we are in the Unity editor
        UnityEditor.EditorApplication.isPlaying = false; // We will not exit the application, we will instead just close and stop playing
#else // Will close the application if it is being played outside the editor
        Application.Quit(); 
#endif
    }

}

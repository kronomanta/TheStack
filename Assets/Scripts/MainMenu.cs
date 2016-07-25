using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Score text gameobject 
    /// </summary>
    public Text scoreText;

    public void ToGame()
    {
        SceneManager.LoadScene("Game");
    }

    void Start()
    {
        //show the previous high score
        scoreText.text = PlayerPrefs.GetInt("score").ToString();
    }
}

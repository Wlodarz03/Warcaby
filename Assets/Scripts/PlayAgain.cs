using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAgain : MonoBehaviour
{
    public void playagain(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}

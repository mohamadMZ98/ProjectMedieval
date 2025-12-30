using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{
     public void OnPlayClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnOptionsClicked()
    {
        SceneManager.LoadScene("OptionsScene");
    }

    public void OnShopClicked()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("You have exited the game.");
    }
}

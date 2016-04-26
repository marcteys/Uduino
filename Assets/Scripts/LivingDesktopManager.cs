using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class LivingDesktopManager : MonoBehaviour {

    public static LivingDesktopManager Instance;
    public GameObject mainMenu = null;
    string currentScene;

	void Start ()
	{
        DontDestroyOnLoad(this.gameObject);
        if (LivingDesktopManager.Instance == null)
        {
            LivingDesktopManager.Instance = this;
        }
        else if (LivingDesktopManager.Instance != null)
        {
            Debug.Log("LivingDescktopManager already here : Destroy...");
            Destroy(this.gameObject);
            return;
        }
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            if (mainMenu.activeSelf && Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("Application.Quit();");
                Application.Quit();
            }
            mainMenu.SetActive(true);
        }
    }

    public void ToggleMenu()
    {
        mainMenu.SetActive(!mainMenu.activeSelf);
    }

    public void LoadPrefabScene(string targetScene)
    {
        SceneManager.UnloadScene(currentScene);
        SceneManager.LoadScene(targetScene);
        currentScene = targetScene;
        mainMenu.SetActive(false);
    }
}

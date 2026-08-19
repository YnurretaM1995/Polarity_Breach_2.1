using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace PolarityBreach.Menus
{
    public class MenuController : MonoBehaviour
    {
        [Header("Levels To Load")]
        public string _newGameLevel;
        private string levelToLoad;
        [SerializeField] private GameObject noSaveGameDialog = null;

        public void NewGameDialogYes()
        {
            SceneManager.LoadScene(_newGameLevel);
        }
        
        public void LoadGameDialogYes()
        {
            if (PlayerPrefs.HasKey("SavedLevel"))
            {
                levelToLoad = PlayerPrefs.GetString("SavedLevel");
                SceneManager.LoadScene(levelToLoad);
            }
            else
            {
                noSaveGameDialog.SetActive(true);
            }
        }

        public void QuitGame()
        {
            SceneManager.LoadScene(0);
        }

        public void ExitButton()
        {
            Application.Quit();
        }

    }
}

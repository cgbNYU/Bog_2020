using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dev_QuitandReload : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            Scene loadedLevel = SceneManager.GetActiveScene ();
            SceneManager.LoadScene (loadedLevel.buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}

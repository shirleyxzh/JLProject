using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public string SplashScene;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SplashScene);
    }
}

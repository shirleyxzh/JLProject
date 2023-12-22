using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SplashMain : MonoBehaviour
{
    public string TetririaScene;
    public GameObject StartButton;
    public GameObject LoadBar;
    public GameObject MainPanel;
    public Image FillBar;

    private AsyncOperation tetririaScene;

    private bool firstLoad = true;

    private void OnEnable()
    {
        StartButton.SetActive(firstLoad);
        MainPanel.SetActive(!firstLoad);
        LoadBar.SetActive(false);
    }

    public void StartPressed()
    {
        firstLoad = false;
        StartButton.SetActive(false);
        StartCoroutine(LoadAssets());
    }

    public void NewGame()
    {
        tetririaScene.allowSceneActivation = true;
    }

    IEnumerator LoadAssets()
    {
        LoadBar.SetActive(true);
        FillBar.fillAmount = 0;
        yield return null;

        tetririaScene = SceneManager.LoadSceneAsync(TetririaScene, LoadSceneMode.Additive);
        tetririaScene.allowSceneActivation = false;

        while (!tetririaScene.isDone)
        {
            var fill = Mathf.Clamp01(tetririaScene.progress / 0.9f);
            FillBar.fillAmount = fill;
            yield return null;
        }

        LoadBar.SetActive(false);
        MainPanel.SetActive(true);
    }
}

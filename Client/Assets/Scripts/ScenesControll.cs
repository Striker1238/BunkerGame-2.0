using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesControll : MonoBehaviour
{
    public void ChangeScene(int index) => StartCoroutine(LoadYourAsyncScene(index));
    public void ChangeScene(string nameScene) => StartCoroutine(LoadYourAsyncScene(nameScene));

    public IEnumerator LoadYourAsyncScene(int index)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

    }
    public IEnumerator LoadYourAsyncScene(string nameScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nameScene, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

    }
}

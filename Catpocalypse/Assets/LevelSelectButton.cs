using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    // Start is called before the first frame update
    public void LoadScene(string sceneToLoad)
    {
        SceneLoader_Async.LoadSceneAsync(sceneToLoad);
    }
}

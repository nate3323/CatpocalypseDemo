using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{
   public void OnMainMenuButton()
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("TowerBase");
        foreach(GameObject tb in bases)
        {
            if(tb.GetComponent<TowerBase>().hasTower == true)
            {
                tb.GetComponent<TowerBase>().DestroyTower();
            }
        }

        //SceneManager.LoadScene("MainMenu");
        SceneLoader_Async.LoadSceneAsync("MainMenu");
    }
}

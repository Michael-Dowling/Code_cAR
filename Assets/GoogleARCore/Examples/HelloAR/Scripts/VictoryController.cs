using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleARCore;
using GoogleARCore.Examples.Common;

public class VictoryController : MonoBehaviour
{
    public string NextLevel;
    public string MainMenu;
    public GameObject VictoryMenu;
    public static bool victory;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartNextLevel()
    {
        Debug.Log("would return to code");
        DataScript.level = (DataScript.level + 1) % DataScript.totalLevels;
        SceneManager.LoadScene("HelloAR");
    }


}

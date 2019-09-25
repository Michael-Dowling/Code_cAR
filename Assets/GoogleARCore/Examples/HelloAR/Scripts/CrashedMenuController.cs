using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashedMenuController : MonoBehaviour
{
    public string CodeScene;
    public GameObject CrashedMenu;
    public GameObject OutOfMovesMenu;
    public static bool crash;
    public static bool codeFinished;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (crash)
        {
            crash = false;
            Pause();
        }
        if (codeFinished)
        {
            codeFinished = false;
            OutOfMoves();
        }
    }

    public void ReturnToCode(bool crashed)
    {
        crash = false;
        codeFinished = false;
        DataScript.isRunning = false;
        MoveCreation.showControls = true;
        CrashedMenu.SetActive(false);
        OutOfMovesMenu.SetActive(false);
        DataScript.resetCar = true;
        DataScript.moveCtr = 0;
    }

    public void Pause()
    {
        MoveCreation.showControls = false;
        CrashedMenu.SetActive(true);
    }

    public void OutOfMoves()
    {
        MoveCreation.showControls = false;
        OutOfMovesMenu.SetActive(true);
    }
}

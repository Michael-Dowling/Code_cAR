using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//Character representations:
//L - left rotation, R - right Rotation, F - Drive Forward

public class MoveCreation : MonoBehaviour
{
    ArrayList moveList;
    ArrayList loopList;
    string moveString;
    string loopString;
    public Dropdown iterationsSelect;
    public TextMeshProUGUI moves;
    public GameObject controls;
    public static bool showControls;

    private void Start()
    {
        iterationsSelect.ClearOptions();
        List<string> nums = new List<string>() {"2", "3", "4", "5", "6", "7", "8", "9", "10","11","12","13","14","15" };
        iterationsSelect.AddOptions(nums);
        moveList = new ArrayList();
        //moves = GetComponent<TextMeshProUGUI>();
        moveString = "";
        showControls = false;
    }

    public void Update()
    {
        if (showControls)
        {
            controls.SetActive(true);
        }
        else
        {
            controls.SetActive(false);
        }
    }

    public void OnRightClick()
    {
        moveList.Add('R');
        int count = moveList.Count;
        Debug.Log("Current length of Move Queue: " + count);
        moveString += 'R';
        moves.text = moveString;
    }

    public void OnLeftClick()
    {
        moveList.Add('L');
        int count = moveList.Count;
        Debug.Log("Current length of Move Queue: " + count);
        moveString += 'L';
        moves.text = moveString;
    }

    public void OnForwardClick()
    {
        moveList.Add('F');
        int count = moveList.Count;
        Debug.Log("Current length of Move Queue: " + count);
        moveString += 'F';
        moves.text = moveString;
    }

    public string GetMoveList()
    {
        string temp = "";
        for (int i = 0; i < moveList.Count; i++)
            temp += moveList[i];
        return temp;
        Debug.Log(temp);
    }

    public void OnLoopStart()
    {
        loopList = new ArrayList();
        loopString = "";
        moves.text = "";
        //Change View on screen
    }
    public void OnLoopLeft()
    {
        loopList.Add('L');
        loopString += 'L';
        moves.text = loopString;

    }
    public void OnLoopForward()
    {
        loopList.Add('F');
        loopString += 'F';
        moves.text = loopString;
    }
    public void OnLoopRight()
    {
        loopList.Add('R');
        loopString += 'R';
        moves.text = loopString;
    }
    public void OnLoopEnd()//get user selected number of iterations
    {
        int iterations = 0;
        switch (iterationsSelect.value)
        {
            case 0:
                iterations = 2;
                break;
            case 1:
                iterations = 3;
                break;
            case 2:
                iterations = 4;
                break;
            case 3:
                iterations = 5;
                break;
            case 4:
                iterations = 6;
                break;
            case 5:
                iterations = 7;
                break;
            case 6:
                iterations = 8;
                break;
            case 7:
                iterations = 9;
                break;
            case 8:
                iterations = 10;
                break;
            case 9:
                iterations = 11;
                break;
            case 10:
                iterations = 12;
                break;
            case 11:
                iterations = 13;
                break;
            case 12:
                iterations = 14;
                break;
            case 13:
                iterations = 15;
                break;
        }
        Debug.Log(iterationsSelect.value + " " + iterations);
        string loopHold = loopString;
        for (int i = 0; i < iterations - 1; i++)
            loopString += loopString;
        moveList.Add(loopString);
        moveString += " " + iterations + "(" + loopHold + ") ";
        moves.text = moveString;
    }

    public void onDelete()
    {
        if (moveString[moveString.Length-1] == ' ')//its a loop
        {
            int start = moveString.LastIndexOf('(');
            if(char.IsDigit(moveString[start-2]))
                moveString = moveString.Substring(0, start - 3);
            else
                moveString = moveString.Substring(0, start - 2);
        }
        else{
               moveString = moveString.Substring(0, moveString.Length - 1);
        }
        moveList.RemoveAt(moveList.Count - 1);
        moves.text = moveString;
    }

    public void runCode()
    {
        showControls = false;
        DataScript.moves = GetMoveList();
        Debug.Log(DataScript.moves);
        DataScript.isRunning = true;
        //SceneManager.LoadScene("HelloAR");
    }
}
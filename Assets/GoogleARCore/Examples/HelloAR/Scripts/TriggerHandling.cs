using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerHandling : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        Debug.Log(this.gameObject.name);
        if (this.gameObject.name == "Wall(Clone)")
        {
            if(other.tag!="Player")
                Destroy(other.gameObject);

            Destroy(other.gameObject);

            
            CrashedMenuController.crash = true;
        }
        else if(this.gameObject.name == "Goal(Clone)")
        {
            Debug.Log("Victory!");
            SceneManager.LoadScene("Victory");
            DataScript.isRunning = false;
        }
    }
}

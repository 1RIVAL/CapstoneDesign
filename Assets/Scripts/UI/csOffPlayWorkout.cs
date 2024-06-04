using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csOffPlayWorkout : MonoBehaviour
{
    public Button back;
    public GameObject playWorkout;
    

    // Start is called before the first frame update
    void Start()
    {
        back.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnButtonClick()
    {
        Debug.Log("go back");
        

        Transform parent = playWorkout.transform.parent;
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (child.gameObject != playWorkout)
                {
                    child.gameObject.SetActive(true);
                }
            }
            playWorkout.SetActive(false);
        }

    }
}

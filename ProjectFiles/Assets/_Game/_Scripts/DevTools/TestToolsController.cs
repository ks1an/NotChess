using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestToolsController : MonoBehaviour
{
    [SerializeField] GameObject testToolGameobject, canvasTestTools;
    private void OnEnable()
    {
        if (Application.isEditor == false)
        {
            Destroy(canvasTestTools);
            Destroy(testToolGameobject);
        }
        else
            DontDestroyOnLoad(this);
    }
}

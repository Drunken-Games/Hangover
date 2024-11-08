using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    private BuildManager buildManager;
    
    private void Start()
    {
        buildManager = FindObjectOfType<BuildManager>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        if (buildManager != null)
        {
            buildManager.ResetContainer();
        }
    }
}
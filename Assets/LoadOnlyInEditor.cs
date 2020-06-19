using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOnlyInEditor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        {

        }
#else
        GetComponent<Canvas>().enabled = true;
#endif    
        }

    // Update is called once per frame
    void Update()
    {
        
    }
}

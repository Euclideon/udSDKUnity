using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using udSDK;

/*
 * Loads all UDS files in a local path and places them in the world space relative to the first
 * Intended for multi part uds files
 */
 
public class loadAllUDSInDirectory : MonoBehaviour
{
    public string path = "";
    public bool reload = false;
    void Start()
    {
        string[] files = Directory.GetFiles(path);
        double[] rootBaseOffset = new double[3];

        Vector3 rootBaseTranslation = Vector3.zero;
        int baseInd = 0; //index in the list to which all models will be placed relative to
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i];
            //skip non uds files
            if (!file.Substring(file.Length - 4).Equals(".uds"))
            {
                if (i == baseInd)
                    ++baseInd;
                continue;
            }

            GameObject modelGameObject = new GameObject(file);
            modelGameObject.transform.SetParent(this.transform);
            modelGameObject.AddComponent<UDSModel>();
            UDSModel model = modelGameObject.GetComponent<UDSModel>();
            model.path = file;
            try
            {
                model.LoadModel();
            }
            catch
            {
                Debug.LogError("load model failed: " + file);
                if (i == baseInd)
                    ++baseInd;
                continue;
            }
            if (i == baseInd) //reference all models to the first 
            {
                rootBaseOffset = model.header.baseOffset;
            }
            model.geolocationOffset =
               UDUtilities.UDtoGL * -new Vector3(
                    (float)rootBaseOffset[0],
                    (float)rootBaseOffset[1],
                    (float)rootBaseOffset[2]
               );
            modelGameObject.tag = "UDSModel";
            model.geolocate = true;
        }
    }

    private void Update()
    {
        if (this.reload)
        {
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
            Start();
            reload = false;
        }
    }
}

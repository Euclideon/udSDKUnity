using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    bool showRay = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        if (Input.GetKey(KeyCode.Space)) {
            showRay = !showRay;
        }
        if (showRay && Physics.Raycast(ray, out hit)){
            Debug.DrawLine(transform.position, hit.point);
            Debug.DrawRay(hit.point, hit.normal * 50);
            Debug.Log("Distance is: " + hit.distance.ToString());
        }
    }
}

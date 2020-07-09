using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMover : MonoBehaviour
{
    public float UpAndDownAmount = 0.0F;
    public Vector3 YPRPerSecond = new Vector3(0.0F, 0.0F, 0.0F);

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.timeSinceLevelLoad;
        float previousTime = currentTime - Time.deltaTime;

        transform.Translate(0.0F, (Mathf.Sin(currentTime) - Mathf.Sin(previousTime)) * UpAndDownAmount, 0.0F, Space.World);
        transform.Rotate(YPRPerSecond * Time.deltaTime, Space.Self);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    public float speed = 1f;
    
    void Update()
    {
        Time.timeScale = speed;
    }

    public void ChangeSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}

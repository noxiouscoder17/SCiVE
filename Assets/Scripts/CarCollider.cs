using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollider : MonoBehaviour
{
    public CarController Reset;
    public void OnCollisionEnter(Collision collision)
    {
        Reset.Death();
    }
}

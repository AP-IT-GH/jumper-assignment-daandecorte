using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public float speed = 5f;
    int side;
    
    void Start()
    {
        speed = Random.Range(3, 8);
        if (Mathf.Approximately(transform.localPosition.x, 0))
        {
            side = 1;
        }
        else if (Mathf.Approximately(transform.localPosition.z, 0))
        {
            side = 2;
        }
    }

    void Update()
    {
        if (side==1)
        {
            transform.localPosition += new Vector3(0, 0, speed * Time.deltaTime);
        }
        else if (side==2)
        {
            transform.localPosition += new Vector3(speed * Time.deltaTime, 0, 0);
        }
    }
}

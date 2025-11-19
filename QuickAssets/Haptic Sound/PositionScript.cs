using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionScript : MonoBehaviour
{

    // Update is called once per frame
    private void Update()
    {
        if (transform.position.y < -2)
        {
            transform.position = new Vector3(1.722f, 2, 1.854f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScrollUp : MonoBehaviour
{
    public float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime * Vector3.up;
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

}

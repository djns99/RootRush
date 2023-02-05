using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScrollUp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Spawner.spawnedObjectSpeed * Time.deltaTime * Vector3.up;
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

}

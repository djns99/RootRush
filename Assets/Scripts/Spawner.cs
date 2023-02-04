using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float spawnsPerSecond = 0.5f; // Num items per second
    public float spawnedObjectSpeed = 4.0f;
    private float coolDown = 0;
    public float borderPercent = 0.1f;
    
    public GameObject goodGuyPrefab; 
    public GameObject badGuyPrefab;
    public Camera cameraView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 rollRandomPos(Vector3 scale)
    {
        Vector3 bottomLeft = cameraView.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
        float screenLeft = bottomLeft.x;
        float width = scale.x;
        float screenRight = cameraView.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, 0.0f)).x;
        float border = (screenRight - screenLeft) * borderPercent / 2;
        float xCoord = Random.Range(screenLeft + border + width / 2, screenRight - width / 2 - border);
        return new Vector3(xCoord, bottomLeft.y - scale.y, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        float rollsPerSecond = 1.0f / Time.deltaTime;
        float succeedEvery = rollsPerSecond / spawnsPerSecond;
        
        //float successChance = 1.0 / succeedEvery;
        //float successChance = Time.deltaTime * spawnsPerSecond;
        float roll = Random.Range(0.0f, succeedEvery);
        coolDown -= Time.deltaTime;

        if ( roll < 1.0f && coolDown <= 0.0f )
        {
            GameObject obj;
            bool goodGuy = Random.Range(0, 2) == 0;
            if (goodGuy)
                obj = Instantiate(goodGuyPrefab);
            else
                obj = Instantiate(badGuyPrefab);
            obj.GetComponent<ObjectScrollUp>().speed = spawnedObjectSpeed;
            Vector3 pos = rollRandomPos(obj.GetComponent<SpriteRenderer>().bounds.size);
            obj.transform.position = obj.transform.worldToLocalMatrix * pos;
            
            coolDown = obj.GetComponent<SpriteRenderer>().bounds.size.y * 1.5f / spawnedObjectSpeed;
        }

    }
}

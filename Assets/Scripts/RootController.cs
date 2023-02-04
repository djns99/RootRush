using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootController : MonoBehaviour
{
    float accelerometerUpdateInterval = 1.0f / 60.0f;
    float lowPassKernelWidthInSeconds = 1.0f;
    float tiltAdjust = 1.5f;
    float responsiveness = 2.0f;
    private float lowPassFilterFactor;
    private float lastTilt = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        lastTilt = Input.acceleration.x;
    }

    // Update is called once per frame
    void Update()
    {
        float tilt;

        if (SystemInfo.supportsAccelerometer)
        {
            tilt = Input.acceleration.x * tiltAdjust;
            tilt = Mathf.Lerp(lastTilt, tilt, lowPassFilterFactor);
            lastTilt = tilt;
        }
        else
        {
            tilt = Input.GetAxis("Horizontal");
        }
        tilt *= responsiveness;
        float newX = transform.position.x + tilt * Time.deltaTime;
        float left = cameraView.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float right = cameraView.ViewportToWorldPoint(new Vector3(1.0f, 0, 0)).x;
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        //float width = transform.localScale.y;
        newX = Mathf.Clamp(newX, left + width / 2, right - width / 2);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);  
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Hit");
        Debug.Log(collider.gameObject.tag);
        if (collider.gameObject.tag == "Loot")
        {
            transform.Rotate(Vector3.forward * 30);
        }
        Destroy(collider.gameObject);
    }

    public Camera cameraView;
}

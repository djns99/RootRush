using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScrollUp : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material material;
    public float speedOverride = -1.0f;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        float height = meshRenderer.bounds.size.y / material.mainTextureScale.y;
        
        float shift = Spawner.spawnedObjectSpeed * Time.deltaTime;
        if (speedOverride >= 0.0f)
        {
            shift = speedOverride * Time.deltaTime;
        }
        material.mainTextureOffset += new Vector2(0.0f, shift / height);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class RootController : MonoBehaviour
{
    public float accelerometerUpdateInterval = 1.0f / 60.0f;
    public float lowPassKernelWidthInSeconds = 1.0f;
    public float tiltAdjust = 1.5f;
    public float responsiveness = 2.0f;
    public float widthVariation = 0.05f;
    public float stepVariation = 0.001f;
    public int numPositions = 120;
    public float startHeight = 3.0f;
    public Camera cameraView;
    public float speed = 0.1f;
    public float colliderHeight = 0.1f;

    public HealthBar healthBar;

    private float finalWidth;
    private float[] targetPercentage;
    private float lowPassFilterFactor;
    private float lastTilt = 0.0f;
    private float totalRootLength;
    private float stepSize;
    private int newestIndex = 0;



    [Serializable]
    public struct Point
    {
        public Point(int idx, float x, float w) { this.index = idx; this.x = x; this.width = w; }
        public int index;
        public float x;
        public float width;
    }
    public List<Point> points;



    // Start is called before the first frame update
    void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        lastTilt = Input.acceleration.x;

        LineRenderer renderer = GetComponent<LineRenderer>();

        float screenTop = cameraView.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, 0.0f)).y;
        points = new List<Point>();

        // Stretch the space slightly so we have a bunch of extra points off the top
        stepSize = (screenTop - startHeight) / (numPositions * 0.9f);
        totalRootLength = (numPositions - 1) * stepSize;

        // Initialise the start positions
        Vector3[] startPos = new Vector3[numPositions];
        for(int i = 0; i < numPositions; i++)
        {
            startPos[i] = Vector3.zero;
            startPos[i].y = startHeight + i * (screenTop - startHeight) / (numPositions - 2);
        }

        targetPercentage = new float[numPositions];
        finalWidth = renderer.widthCurve.Evaluate(1.0f);
        // Override the default curve with a point for each element
        AnimationCurve curve = new AnimationCurve();
        for(int i = numPositions - 1; i >= 0; i--)
        {
            float time = (float)i / (numPositions-1);
            float value = renderer.widthCurve.Evaluate(time);
            curve.AddKey(time, value);
            points.Add(new Point(i, 0, value));
            targetPercentage[i] = value / finalWidth;
        }
        newestIndex = numPositions - 1;

        float colliderWidth = renderer.widthCurve.Evaluate(colliderHeight / totalRootLength);
        GetComponent<PolygonCollider2D>().points = new Vector2[] { new Vector2(-colliderWidth/2, -colliderHeight), Vector2.zero, new Vector2(colliderWidth/2, -colliderHeight) };
        renderer.positionCount = numPositions;
        renderer.SetPositions(startPos);
        //renderer.widthCurve = curve;
        transform.position = new Vector3(points[points.Count - 1].x, startHeight, 0.0f);  
    }

    // Update is called once per frame
    void Update()
    {
        float tilt;

        if (SystemInfo.supportsAccelerometer)
        {
            tilt = Input.acceleration.x * tiltAdjust;
        }
        else
        {
            tilt = Input.GetAxis("Horizontal");
        }
        tilt = Mathf.Lerp(lastTilt, tilt, lowPassFilterFactor);
        lastTilt = tilt;
        tilt *= responsiveness;


        Debug.Assert(points.Count > 0);
        float oldX = points[points.Count - 1].x;
        float newX = oldX + tilt * Time.deltaTime;
        float left = cameraView.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float right = cameraView.ViewportToWorldPoint(new Vector3(1.0f, 0, 0)).x;

        float maxWidth = finalWidth + widthVariation;
        newX = Mathf.Clamp(newX, left + maxWidth / 2, right - maxWidth / 2);


        AnimationCurve curve = new AnimationCurve();

        int shift = (int)Mathf.Round((totalRootLength * Time.deltaTime * speed) / stepSize);
        shift = Math.Max(1, shift);

        newestIndex += shift;

        // Generate a random number which will be the final size this particular piece of root will go to

        Point lastPoint = points[points.Count -1];
        float upperBound = Mathf.Min(lastPoint.width + stepVariation * Time.deltaTime, finalWidth + widthVariation);
        float lowerBound = Mathf.Max(lastPoint.width - stepVariation * Time.deltaTime, finalWidth - widthVariation);

        float newWidth = UnityEngine.Random.Range(lowerBound, upperBound);
        points.Add(new Point(newestIndex, newX, newWidth));

        Vector3[] positions = new Vector3[numPositions];
        for (int pIdx = 1; pIdx < points.Count; pIdx++)
        {
            var older = points[pIdx-1]; 
            var newer = points[pIdx];

            int invertedOldIndex = newestIndex - older.index;
            int invertedNewIndex = newestIndex - newer.index;
            if (invertedNewIndex >= numPositions - 1)
            {
                // Both are invalid. Remove the older one
                points.RemoveAt(pIdx-1);
                pIdx--;
                continue;
            }

            if(invertedOldIndex >= numPositions)
            {
                float t = (numPositions - 1 - invertedNewIndex) / (invertedOldIndex - invertedNewIndex);
                invertedOldIndex = numPositions - 1;
                float midX = Mathf.Lerp(newer.x, newer.x, t);
                float midW = Mathf.Lerp(newer.width, newer.width, t);
                points[pIdx - 1] = new Point(newestIndex - numPositions - 1, midX, midW);
                curve.AddKey(1.0f, midW * targetPercentage[numPositions - 1]);
            }

            for (int outIdx = invertedNewIndex; outIdx <= invertedOldIndex; outIdx++)
            {
                float t = (outIdx - invertedNewIndex) / (invertedOldIndex - invertedNewIndex);
                positions[outIdx] = new Vector3(Mathf.Lerp(newer.x, older.x, (float)outIdx / (invertedOldIndex - invertedNewIndex)), startHeight + outIdx * stepSize, 0.0f);
            }

            curve.AddKey((float)invertedNewIndex / (numPositions - 1), newer.width * targetPercentage[invertedNewIndex]);
        }



        LineRenderer renderer = GetComponent<LineRenderer>();
        renderer.SetPositions(positions);
        var newPos = new Vector3(newX, transform.position.y, transform.position.z);
        Vector2 direction = (newPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        int steps = (int)(colliderHeight / stepSize);
        float colliderWidth = renderer.widthCurve.Evaluate((float)steps / (numPositions-1));

        Vector3 tipPos = positions[positions.Length - 1];
        Vector3 backPos = positions[positions.Length - 1 - steps];
        Vector3 pointToBack = tipPos - backPos;

        Vector3 perp = Vector3.Cross(pointToBack, Vector3.forward).normalized;
        Vector3 rightTri = pointToBack + perp * colliderWidth / 2;
        Vector3 leftTri = pointToBack - perp * colliderWidth / 2;


        GetComponent<PolygonCollider2D>().points = new Vector2[] { 
            leftTri,
            Vector2.zero, 
            rightTri, 
        };

        transform.position = newPos;
        //renderer.widthCurve = curve;



        /*









        for (int i = 0; i < positions.Length; i++)
        {
            float percentage = targetPercentage[i];
            if (i >= 1) {
                // Shift these back to front
                int idxOld = positions.Length - i - 1;
                int idxNew = idxOld + 1;
                positions[idxNew].x = positions[idxOld].x;

                // An adjustment since the value is scaled down by the last amount
                percentage /= targetPercentage[i - 1];
            }
            Keyframe key = renderer.widthCurve.keys[i];
            renderer.widthCurve.MoveKey(i, new Keyframe(iToTime(i), saved * percentage));
            saved = key.value;
        }
        positions[0].x = newX;
        renderer.SetPositions(positions);

        var newPos = new Vector3(newX, transform.position.y, transform.position.z);
        transform.LookAt(newPos); // Look first so we are looking at the location we are going
        transform.position = newPos;  
        */
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Hit");
        Debug.Log(collider.gameObject.tag);
        if (collider.gameObject.tag == "Loot")
        {
            healthBar.Fertilize();
        }
        else if (collider.gameObject.tag == "Bug")
        {
            healthBar.Damage();
        }
        Destroy(collider.gameObject);
    }
}

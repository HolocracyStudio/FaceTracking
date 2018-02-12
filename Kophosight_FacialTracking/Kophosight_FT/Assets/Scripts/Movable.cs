using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Windows.Graphics.Imaging;

public class Movable : MonoBehaviour {

    public static bool isDetectingFace;
    public static float x, y, z;
    //public static BitmapBounds faceBox;
    private Camera RGBcamera;
    public static Vector3 worldPosition;

    //private GameObject cube = gameObject.GetComponent();
    // Use this for initialization
    void Start () {

        RGBcamera = Camera.main;
        worldPosition = new Vector3(0, 0, 0);
        isDetectingFace = false;
        x = 0;
        y = 0;
        z = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        // placer le cube dans les coordonnées caméra ????
        if (isDetectingFace == true)
        {
            // this.enabled = true;
            y = RGBcamera.pixelHeight - y;
            // y = y / RGBcamera.pixelHeight;
            // x = x / RGBcamera.pixelWidth ;
            // this.gameObject.SetActive(true);
           
            worldPosition = RGBcamera.ScreenToWorldPoint(new Vector3( x, y, z));
            float deltaTime = Time.unscaledDeltaTime;

            worldPosition.x = Mathf.Round(worldPosition.x * 100.0f) / 100.0f;
            worldPosition.y = Mathf.Round(worldPosition.y * 100.0f) / 100.0f;
            worldPosition.z = Mathf.Round(worldPosition.z * 100.0f) / 100.0f;            

            //gameObject.transform.position = new Vector3(x, y, z);
            //this.gameObject.transform.position = Vector3.Lerp(this.transform.position, worldPosition, 2 * deltaTime);
            
            this.transform.position = worldPosition;
        }
        else
        {
            /*
              x = 0;
              y = 0;
              z = 0;
            */
            //this.enabled = false;
            
            //this.gameObject.SetActive(false);
        }
        
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceCoord : MonoBehaviour {
    public static string x = "_";
    public static string y = "_";
   
    public Text Coords;
	// Use this for initialization
	void Start () {
       // Coords = GameObject.FindObjectOfType<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        Coords.text = "X: " + x + "\nY: " + y;
	}
}

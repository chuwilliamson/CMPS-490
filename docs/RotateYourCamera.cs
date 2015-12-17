using UnityEngine;
using System.Collections;

public class RotateYourCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
        _root = GameObject.FindGameObjectWithTag("root");
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(_root.transform.position, Vector3.up, 20 * Time.deltaTime);
	
	}
    private GameObject _root;
}

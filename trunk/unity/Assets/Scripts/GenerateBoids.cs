using UnityEngine;
using System.Collections;

public class GenerateBoids : MonoBehaviour {

	private void Awake ()
	{
		//if (_predator) 
		//	_boidsArray = new GameObject[3];
		//else

		_boidsArray = new GameObject[_numBoids];
		SpawnBoids ();
	}
	private void SpawnBoids ()
	{
		//if (_predator) {
		//	_boidsArray [0] = (GameObject.FindWithTag ("Predator1"));
		//	_boidsArray [1] = (GameObject.FindWithTag ("Predator2"));
		//	_boidsArray [2] = (GameObject.FindWithTag ("Predator3"));
		//} else {
			GameObject boidsParent = new GameObject ("Boids Parent");
			boidsParent.transform.parent = gameObject.transform;
			for (int i = 0; i < _boidsArray.Length; i++) {
				
				GameObject clone = Instantiate (_boidPrefab, transform.localPosition, transform.localRotation) as GameObject;
				clone.GetComponent<BoidInfo> ().Position = new Vector3 (Random.Range (-35f, 35f), Random.Range (-35f, 35f), Random.Range (-10f, 10f));
				clone.GetComponent<BoidInfo> ().Velocity = new Vector3 (Random.Range (-10f, 10f), Random.Range (-10f, 10f), Random.Range (-10f, 10f));
				clone.name = "Boid " + i;
				clone.transform.parent = boidsParent.transform;
				_boidsArray [i] = clone;
				
			}	


	}

	private GameObject [] _boidsArray;
	public int _numBoids;
	public GameObject _boidPrefab;
}

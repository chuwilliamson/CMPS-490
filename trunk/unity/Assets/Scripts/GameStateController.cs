using UnityEngine;
using System.Collections;

public class GameStateController : MonoBehaviour
{
		public void QuitLevel ()
		{
				Debug.Log ("Quitting Application");
				Application.Quit ();
		}
}

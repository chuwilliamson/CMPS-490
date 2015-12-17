using UnityEngine;
using System.Collections;

public class DragToMove : MonoBehaviour
{



		private void Start ()
		{
				GetComponent<Renderer>().material.color = Color.gray; //default color for the ball handle
		}

		private void OnMouseDown ()
		{
				screenPoint = Camera.main.WorldToScreenPoint (gameObject.transform.position);//whenever we mousedown grab the screen position
				GetComponent<Renderer>().material.color = Color.red;
				offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		
		}

		private void OnMouseUp ()
		{

				GetComponent<Renderer>().material.color = Color.gray;
		}
	
		private void OnMouseDrag ()
		{
				Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);		
				Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
				transform.position = curPosition;
		
		}

		public void EnableAnimation ()
		{
				if (!animated) {
            gameObject.GetComponent<Animator>().SetTrigger("move");
						_animationValue.text = "Manual Movement Disabled";
						animated = true;
				} else {
            gameObject.GetComponent<Animator>().SetTrigger("move");
            
						_animationValue.text = "Manual Movement Enabled";
						animated = false;
				}
		}
		public GUIText _animationValue;
		private Vector3 screenPoint;
		private Vector3 offset;
		private bool animated;
}

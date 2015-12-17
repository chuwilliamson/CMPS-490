using UnityEngine;
using System.Collections;

public class BoidInfo : MonoBehaviour
{
		public Vector3 Position {
				get { return gameObject.transform.position;}
				set { gameObject.transform.position = value;}
				
		}
		public Vector3 Velocity {
				get { return _velocity;}
				set {
						_velocity = value;
						_speed = _velocity.magnitude;
				}
		}
		
		public float Speed {
				get{ return _speed;}
				

		}

		public bool Perch {
				get{ return _perch;}
				set{ _perch = value;}
		}
		

		private Vector3 _velocity;
		private float _speed;
		private bool _perch;

		
}

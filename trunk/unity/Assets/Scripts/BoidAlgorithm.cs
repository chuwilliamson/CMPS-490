//notes: 3/3/2014 boids keep flying to flock even though they should be ignoring them
//written by Matthew Williamson
//adapted from Conrad Parker's pseudocode of Craig W. Reynold's Boid's algorithm
//for Unity3D 
//http://www.red3d.com/cwr
using UnityEngine;
using System.Collections;

//adaptation of Boids pseudocode into Unity taken from http://www.kfish.org/boids/pseudocode.html
public class BoidAlgorithm : MonoBehaviour
{
		//make boids
		private void Awake ()
		{
				if (_predator) 
						_boidsArray = new GameObject[3];
				else
						_boidsArray = new GameObject[_numBoids];
				SpawnBoids ();
		}
		private void SpawnBoids ()
		{
				if (_predator) {
						_boidsArray [0] = (GameObject.FindWithTag ("Predator1"));
						_boidsArray [1] = (GameObject.FindWithTag ("Predator2"));
						_boidsArray [2] = (GameObject.FindWithTag ("Predator3"));
				} else {
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
						//StartCoroutine (DatBoids ());
				}
		}

		
		void FixedUpdate ()
		{
				//Debug.Log ("FixedUpdate time :" + Time.deltaTime);
				//This is so bad... This should be in another gameobject to seperate the boids from any type of GUI elements
				//Initially I wanted to have an external library so that nothing would be coupled, then I just wanted to use 
				//Modal View Controller design pattern, then I just wanted to get it working. This still needs to be pulled out and 
				//seperated.
				//Issues with the seperation were figuring out how to apply the algorithm and only the algorithm on an array of objects
				//without passing that array around. Asking people for advice made it worse so I just did it this way for now.
				//*fixed* Going to a GUI handler to get our slider values.
				_rule1Strength = GuiController.GetComponent<GUIController> ().Slider1;
				_rule2Strength = GuiController.GetComponent<GUIController> ().Slider2;
				_rule3Strength = GuiController.GetComponent<GUIController> ().Slider3;
				if (_predator)
						_velocityLimit = 15; //static velocity for the predators so that the user can see the variation of boids and predators
				else                         //if the predators and the boids are the same velocity it does not look very realistic.
						_velocityLimit = GuiController.GetComponent<GUIController> ().Slider4;
				MoveBoidsToNewPosition ();
		}

		
	
	
		

		private void MoveBoidsToNewPosition ()
		{
				Vector3 v1 = Vector3.zero, 
				v2 = Vector3.zero, 
				v3 = Vector3.zero, 
				v4 = Vector3.zero,
				v5 = Vector3.zero,
				v6 = Vector3.zero;
				

				foreach (GameObject boid in _boidsArray) {		

						BoidInfo boidInfo = boid.GetComponent<BoidInfo> ();
		

						//normalizing these vectors will give the direction the boid should move to
						//the magnitude of these vectors will give how fast the boid should move there
						//relative to the timestep which is time.deltaTime;
						v1 = _rule1Strength * Rule1 (boid);  //Flock Centering (Cohesion)											
						v2 = _rule2Strength * Rule2 (boid);  //Collision Avoidance (Seperation)
						v3 = _rule3Strength * Rule3 (boid);  //Velocity Matching (Alignment)
						v4 = CheckBounds (boid);
						v5 = _tendToStrength * TendToPlace (_target, boid);
						v6 = (-1) * _avoidStrength * AvoidPlace (_avoid, boid);
						
						
						//the boidInfo.Velocity is the amount of positional change 
						//resulting in the offset vectors
						boidInfo.Velocity = (boidInfo.Velocity + v1 + v2 + v3 + v4 + v5 + v6);
						
						boid.transform.rotation = Quaternion.LookRotation (boidInfo.Velocity);

						LimitSpeed (boid);
						//Interpret the velocity as how far the boid moves per time step we add it to the current position
						boidInfo.Position = boidInfo.Position + (boidInfo.Velocity * Time.deltaTime);
						//the new position of the boid is calculated by adding the offset vectors (v1,v2...vn) to the position
				}
		}

	#region Rule 1: Boids try to fly towards the centre of mass of neighbouring boids.
		/*Rule 1: Boids try to fly towards the centre of mass of neighbouring boids.
		 The 'centre of mass' is simply the average position of all the boids. I use the term centre of mass by analogy with the 
		 corresponding physical formula (however we ignore individual masses here and treat all boids having the same mass).

		Assume we have N boids, called b1, b2, ..., bN. Also, the position of a boid b is denoted b.position. Then the 'centre of mass' c 
		of all N boids is given by:

		c = (b1.position + b2.position + ... + bN.position) / N
		Remember that the positions here are vectors, and N is a scalar.

		However, the 'centre of mass' is a property of the entire flock; it is not something that would be considered by an individual boid.
		I prefer to move the boid toward its 'perceived centre', which is the centre of all the other boids, not including itself. 
		Thus, for boidJ (1 <= J <= N), the perceived centre pcJ is given by:

		pcJ = (b1.position + b2.position + ... + bJ-1.position +
			bJ+1.position + ... + bN.position) / (N-1)
		Having calculated the perceived centre, we need to work out how to move the boid towards it. To move it 1% of the way
		towards the centre (this is about the factor I use) this is given by (pcJ - bJ.position) / 100.

		Summarising this in pseudocode:

		PROCEDURE rule1(boid bJ)

			Vector pcJ

			FOR EACH BOID b
				IF b != bJ THEN
					pcJ = pcJ + b.position
				END IF
			END

			pcJ = pcJ / N-1

			RETURN (pcJ - bJ.position) / 100

		END PROCEDURE
		Thus we have calculated the first vector offset, v1, for the boid.*/
		private Vector3 Rule1 (GameObject boid) //(cohesion
		{
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> (); //current boid info

				Vector3 perceivedCenter = Vector3.zero;

				foreach (GameObject b in _boidsArray) {
						BoidInfo bInfo = b.GetComponent<BoidInfo> (); //neighbors


						if (b != boid) {	
								//doing another calculation to see if I can get some of the boids in the same system to split up at times
								//for randomness
								if (Vector3.Distance (bInfo.Position, boidInfo.Position) < _flockRadius) //neighborhood
										perceivedCenter += bInfo.Position;
						}
				}

				perceivedCenter /= (_boidsArray.Length - 1); //dividing by the size of the array -1
				//gives the average perceived center of mass
				
				perceivedCenter = (perceivedCenter - boidInfo.Position) / 100;
				//how strong the boid will move to the center
				//higher means less strength
				return perceivedCenter;
		}
	#endregion

	#region Rule 2: Boids try to keep a small distance away from other objects (including other boids).
		/*Rule 2: Boids try to keep a small distance away from other objects (including other boids).
		The purpose of this rule is to for boids to make sure they don't collide into each other. I simply look at each boid, and if
		it's within a defined small distance (say 100 units) of another boid, move it as far away again as it already is. 
		This is done by subtracting from a vector c the displacement of each boid which is near by. 
		We initialise c to zero as we want this rule to give us a vector which when added to the current position moves 
		a boid away from those near it.

		In pseudocode:

			PROCEDURE rule2(boid bJ)

				Vector c = 0;

				FOR EACH BOID b
					IF b != bJ THEN
						IF |b.position - bJ.position| < 100 THEN
							c = c - (b.position - bJ.position)
						END IF
					END IF
				END

				RETURN c

			END PROCEDURE
		It may seem odd that we choose to simply double the distance from nearby boids, as it means that boids which are very close 
		are not immediately "repelled". Remember that if two boids are near each other, this rule will be applied to both of them. 
		They will be slightly steered away from each other, and at the next time step if they are still near each other they will be pushed 
		further apart. Hence, the resultant repulsion takes the form of a smooth acceleration. It is a good idea to maintain a principle of 
		ensuring smooth motion. If two boids are very close to each other it's probably because they have been flying very quickly towards 
		each other, considering that their previous motion has also been restrained by this rule. Suddenly jerking them away from each other, 
		such that they each have their motion reversed, would appear unnatural, as if they bounced off each other's invisible force fields. 
		Instead, we have them slow down and accelerate away from each other until they are far enough apart for our liking.

	 	*/
		private Vector3 Rule2 (GameObject boid)
		{
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> (); //current boid info

				Vector3 displacement = Vector3.zero;

				foreach (GameObject b in _boidsArray) {

						BoidInfo bInfo = b.GetComponent<BoidInfo> (); //neighbor

						if (b != boid) {
								//if the distance between the current boid and his neighbor
								//is less than 10 they are too close and must be seperated
								
								if (Vector3.Distance (bInfo.Position, boidInfo.Position) < _rule2Radius) {
									
										//calculate a displacement to move them apart
										//the displacement will result in a vector
										//that when added to the original velocity vector will
										//move them away from each other
										displacement -= (bInfo.Position - boidInfo.Position);
										
								}
						}
				}

				return displacement;
		}
	#endregion
		
	#region Rule 3: Boids try to match velocity with near boids.
		/*This is similar to Rule 1, however instead of averaging the positions of the other boids we average the velocities. 
		 * We calculate a 'perceived velocity', pvJ, then add a small portion (about an eighth) to the boid's current velocity.


			PROCEDURE rule3(boid bJ)

				Vector pvJ

				FOR EACH BOID b
					IF b != bJ THEN
						pvJ = pvJ + b.velocity
					END IF
				END

				pvJ = pvJ / N-1

				RETURN (pvJ - bJ.velocity) / 8

			END PROCEDURE
		That's all there is to it :) The three rules are fairly simple to implement.
	*/
		private Vector3 Rule3 (GameObject boid)
		{
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> (); //current boid info

				Vector3 perceivedVelocity = Vector3.zero;

				foreach (GameObject b in _boidsArray) {
						BoidInfo bInfo = b.GetComponent<BoidInfo> ();
						if (b != boid) {
								if (Vector3.Distance (boidInfo.Position, bInfo.Position) < _flockRadius) {//if the distance from this boid and another boid is less than a set amount they
										perceivedVelocity += bInfo.Velocity;//are in the same neighborhood
										
								}
						}
						
				}

				perceivedVelocity /= (_boidsArray.Length - 1);
				perceivedVelocity = (perceivedVelocity - boidInfo.Velocity) / 8; //using conrad's magic /8 till i get a better handle on what the vectors are doing
				//how strong the boid will try to match velocity
				//higher means less strength

				
				
				return perceivedVelocity;
		}

	#endregion

	#region Limiting the bounds
		/*In order to keep the flock within a certain area (eg. to keep them on-screen) Rather than unrealistically placing them within 
		some confines and thus bouncing off invisible walls, we implement a rule which encourages them to stay within rough boundaries.
		That way they can fly out of them, but then slowly turn back, avoiding any harsh motions.
		
		PROCEDURE bound_position(Boid b)
			Integer Xmin, Xmax, Ymin, Ymax, Zmin, Zmax
			Vector v
			
			IF b.position.x < Xmin THEN
			v.x = 10
			ELSE IF b.position.x > Xmax THEN
			v.x = -10
			END IF
			IF b.position.y < Ymin THEN
			v.y = 10
			ELSE IF b.position.y > Ymax THEN
			v.y = -10
			END IF
			IF b.position.z < Zmin THEN
			v.z = 10
			ELSE IF b.position.z > Zmax THEN
			v.z = -10
			END IF
			
			RETURN v
			END PROCEDURE
			Here of course the value 10 is an arbitrary amount to encourage them to fly in a particular direction.
			*/

		private Vector3 CheckBounds (GameObject boid)
		{	
				int Xmin = -(_boundary), 
				Xmax = _boundary, 
				Ymin = -(_boundary), 
				Ymax = _boundary, 
				Zmin = -(_boundary / 2), 
				Zmax = 5;

				int ground = -5;


				Vector3 v = new Vector3 ();
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> ();
				if (boidInfo.Position.x < Xmin) {						
						v.x = _boundsStr;
				} else if (boidInfo.Position.x > Xmax) {
						v.x = -(_boundsStr);
				}
				if (boidInfo.Position.y < Ymin) {
						v.y = _boundsStr;
				} else if (boidInfo.Position.y > Ymax) {
						//print ("boid out of y neg");
						v.y = -(_boundsStr);
				}
				if (boidInfo.Position.z < Zmin) {
						v.z = _boundsStr;
				} else if (boidInfo.Position.z > Zmax) {
						v.z = -(_boundsStr);
				}
				/*if (boidInfo.Position.y < ground) {
						boidInfo.Position = new Vector3 (boidInfo.Position.x, ground, boidInfo.Position.z);
						boidInfo.Perch = true;
				}*/
					
				return v;
				
				

		}
	#endregion	

	#region Limiting the speed
		/*I find it a good idea to limit the magnitude of the boids' velocities, this way they don't go too fast.
		 * Without such limitations, their speed will actually fluctuate with a flocking-like tendency, 
		 * and it is possible for them to momentarily go very fast. We assume that real animals can't go arbitrarily fast, 
		 * and so we limit the boids' speed. (Note that I am using the physical definitions of velocity and speed here; 
		 * velocity is a vector and thus has both magnitude and direction, whereas speed is a scalar and is equal to the magnitude 
		 * of the velocity).
			
			For a limiting speed vlim:
				
				PROCEDURE limit_velocity(Boid b)
				Integer vlim
				Vector v
				
				IF |b.velocity| > vlim THEN
				b.velocity = (b.velocity / |b.velocity|) * vlim
				END IF
				END PROCEDURE
				This procedure creates a unit vector by dividing b.velocity by its magnitude, then multiplies this unit vector by vlim. 
				The resulting velocity vector has the same direction as the original velocity but with magnitude vlim.
				
				Note that this procedure operates directly on b.velocity, rather than returning an offset vector. It is not used like the
				other rules; rather, this procedure is called after all the other rules have been applied and before calculating the new 
				position, ie. within the procedure move_all_boids_to_new_positions:
		
				b.velocity = b.velocity + v1 + v2 + v3 + ...
				limit_velocity(b)
				b.position = b.position + b.velocity
		*/

		private void LimitSpeed (GameObject boid)
		{				
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> ();

				if (boidInfo.Velocity.magnitude > _velocityLimit) //if the size of the velocity is greater than the limit set
						//normalize it and scale it by the limit
						boidInfo.Velocity = boidInfo.Velocity.normalized * _velocityLimit;
				//magnitude is the length of a vector
				//dat mag is given by dat pythag
				//a^2 + b^2 = c^2
				//or a length c is given by the sqrt(a^2 + b^2)
		}
	#endregion

	#region Tend to place
		private Vector3 TendToPlace (GameObject place, GameObject boid)
		{
				Vector3 tendTo;// = Vector3.zero;
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> ();

				tendTo = place.transform.position;
				
				tendTo = tendTo - (boidInfo.Position);
				tendTo = tendTo / 10;
				return tendTo;

		}

		private Vector3 AvoidPlace (GameObject place, GameObject boid)
		{
				Vector3 tendTo;// = Vector3.zero;
				BoidInfo boidInfo = boid.GetComponent<BoidInfo> ();
				tendTo = place.transform.position;
				if (Vector3.Distance (boidInfo.Position, tendTo) < (_flockRadius / 2)) {
						tendTo = tendTo - (boidInfo.Position);
						tendTo = tendTo / 10;
						return tendTo;
			
				} else
						return Vector3.zero;
		
		
		
		}
	
	
	#endregion
	
		private IEnumerator Wait (float time)
		{

				yield return new WaitForSeconds (time);
		}

			
		
		private GameObject[] _boidsArray;
		public GameObject _boidPrefab;
		public int _numBoids;
		public float _boundsStr;		
		public float _velocityLimit;
		public int _boundary;
		//_rule1Strength: this is the coefficient that will determine the strength of never talking about fight club... or cohesion.
		//HIGH VALUES: High values will cause the boids to pack really tight. 
		//LOW VALUES :Lower values will cause boids to disperse. Leaving the rule1Strength at it's lowest value will cause the boids to hug the boundaries from the boundary vector, 
		//emulating insect behavior**maybe something to look into later**.
		public float _rule1Strength; 
		//rule2Strength: this is the coefficient that will control the neighborhood.
		public float _rule2Strength;
		public int _rule2Radius;
		public float _rule3Strength;
		public int _flockRadius;
		public GameObject _target;
		public GameObject _avoid;
		public float _tendToStrength;
		public int _avoidStrength;
		public bool _predator;
		public GameObject GuiController;
		

		
























	
}

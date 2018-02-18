using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Attached to car

public class Agent : MonoBehaviour {
	private bool hasFailed = false;
	private float distanceDelta;

	private float headingAngle; //Degrees

	private NNet neuralnet;
	private Sensor sensor;

	public float MAX_ROTATION = 8; //max rotate speed
	public float ACCEL_FACTOR = 1f;

	public float MIN_SPEED = 2;
	public float MAX_SPEED = 5;
	private float currSpeed;
	private float totalTime;

	private Hit hit;
	public bool freezeMotion = false;

	// Use this for initialization
	void Start () {
		sensor = gameObject.GetComponent<Sensor> ();
		hit = gameObject.GetComponent<Hit> ();
		currSpeed = MIN_SPEED;
		totalTime = 0;
	}

	// Update is called once per frame
	void Update () {
		hasFailed = hit.HasCrashed();

		if (!freezeMotion && !hasFailed) {
			totalTime += Time.deltaTime;

			List<float> inputs = new List<float> ();
			inputs.Add (NormaliseSpeed (currSpeed));
			inputs.Add (Normalise (sensor.Getdis_fl()));
			inputs.Add (Normalise (sensor.Getdis_f()));
			inputs.Add (Normalise (sensor.Getdis_fr()));

			neuralnet.SetInput (inputs);
			neuralnet.refresh ();

			///////ROTATION//////////
			float thetaChange = neuralnet.GetOutput (0);
			//These values go from 0 to 1, so lets map them
			thetaChange += -0.5f; //shift down by 0.5, so it goes from -0.5 to 0.5
			thetaChange *= 2f; //double, so it goes from -1 to 1

			transform.Rotate (new Vector3 (0, MAX_ROTATION * thetaChange * Time.deltaTime, 0));

			///////TRANSLATION//////////
			float acceleration = neuralnet.GetOutput (1);
			//These values go from 0 to 1, so lets map them
			acceleration *= 2f; //double, so it goes from 0 to 2
			acceleration -= 1f; //shift down by 1, so it goes from -1 to 1

			//Keep a running total of the speed
			currSpeed += ACCEL_FACTOR * acceleration * Time.deltaTime;
			currSpeed = Clamp(currSpeed, MIN_SPEED, MAX_SPEED);

			float dir = (Mathf.PI * transform.rotation.eulerAngles.y) / 180; //convert to radians
			Vector3 newsp = new Vector3(currSpeed * Mathf.Cos (dir), 0, - currSpeed * Mathf.Sin (dir));

			transform.position = transform.position + (newsp * Time.deltaTime);
		}
	}

	public bool HasFailed()
	{
		return hasFailed;
	}

	public float Normalise(float i){
		float depth = i / sensor.Sensor_Length;
		return 1 - depth;
	}

	public float NormaliseSpeed(float speed){
		return (speed - MIN_SPEED) / (MAX_SPEED - MIN_SPEED);
	}

	public float FitnessFunction()
	{
		return hit.CheckPointsPassed ();
	}

	public void Attach(NNet net){
		neuralnet = net;
	}

	public void Reset(){
		hasFailed = false;
		hit.Reset();
		currSpeed = MIN_SPEED;
		totalTime = 0;
	}

	public float Clamp (float val, float min, float max){
		if (val < min) {
			return min;
		}
		if (val > max) {
			return max;
		}
		return val;
	}

}
using UnityEngine;
using System.Collections;

//Attached to car

public class Sensor : MonoBehaviour {
	public float Sensor_Length;
	private float dis_l, dis_fl, dis_f, dis_fr, dis_r;
	public bool drawRays = true;

	// Use this for initialization
	void Start () {
		dis_l = 0.0f;
		dis_fl = 0.0f;
		dis_f = 0.0f;
		dis_fr = 0.0f;
		dis_r = 0.0f;
	}

	// Update is called once per frame
	void Update () {
		Vector3 origin = transform.position + Vector3.up * 0.2f;

		float heading = transform.rotation.eulerAngles.y;
		float angle = (heading / 180) * Mathf.PI;

		//Move to front of car
		origin = origin + new Vector3 (0.5f*Mathf.Cos (angle), 0, -0.5f*Mathf.Sin (angle));

		Vector3 left = new Vector3 (origin.x + Sensor_Length * Mathf.Cos (angle - Mathf.PI / 4), origin.y, origin.z - Sensor_Length * Mathf.Sin (angle - Mathf.PI / 4));
		Vector3 frontleft = new Vector3 (origin.x + Sensor_Length * Mathf.Cos (angle - Mathf.PI / 4), origin.y, origin.z - Sensor_Length * Mathf.Sin (angle - Mathf.PI / 4));
		Vector3 front = new Vector3 (origin.x + Sensor_Length * Mathf.Cos (angle), origin.y, origin.z - Sensor_Length * Mathf.Sin (angle));
		Vector3 frontright = new Vector3 (origin.x - Sensor_Length * Mathf.Sin (angle - Mathf.PI / 4), origin.y, origin.z - Sensor_Length * Mathf.Cos (angle - Mathf.PI / 4));
		Vector3 right = new Vector3 (origin.x - Sensor_Length * Mathf.Sin (angle - Mathf.PI / 4), origin.y, origin.z - Sensor_Length * Mathf.Cos (angle - Mathf.PI / 4));

		//Cast the rays
		RaycastHit hit_l, hit_fl, hit_f, hit_fr, hit_r;

		//frontleft
		//Physics.Linecast (origin, left, out hit_l);
		//if (drawRays) Debug.DrawLine (origin, left, Color.yellow);
		//frontleft
		Physics.Linecast (origin, frontleft, out hit_fl);
		if (drawRays) Debug.DrawLine (origin, frontleft, Color.red);
		//front
		Physics.Linecast (origin, front, out hit_f);
		if (drawRays) Debug.DrawLine (origin, front, Color.red);
		//frontright
		Physics.Linecast (origin, frontright, out hit_fr);
		if (drawRays) Debug.DrawLine (origin, frontright, Color.red);
		//right
		//Physics.Linecast (origin, right, out hit_r);
		//if (drawRays) Debug.DrawLine (origin, right, Color.blue);


		//dis_l = hit_l.distance;
		dis_fl = hit_fl.distance;
		dis_f = hit_f.distance;
		dis_fr = hit_fr.distance;
		//dis_r = hit_r.distance;
	}
		

	public float Getdis_fl()
	{
		return dis_fl;
	}

	public float Getdis_f()
	{
		return dis_f;
	}

	public float Getdis_fr()
	{
		return dis_fr;
	}

}
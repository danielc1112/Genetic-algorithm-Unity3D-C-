using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attached to main camera

public class CameraMovement : MonoBehaviour {
	public Transform Target;

	void Update () {
		//TODO: Make y position depend on speed
		transform.position = new Vector3 (Target.position.x, transform.position.y, Target.position.z);
	}
}

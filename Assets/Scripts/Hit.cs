using UnityEngine;
using System.Collections;

//Attached to car

public class Hit : MonoBehaviour {
	public Material Passed;

	private int checkpoints;
	private bool crashed;
	private Manager man;
	private Agent agent;
	private Sensor sensor;
	public GameObject explosion;

	void Start () {
		crashed = false;
		checkpoints = 0;
		man = GameObject.Find ("SceneManager").GetComponent<Manager> ();
		agent = gameObject.GetComponent<Agent> ();
		sensor = gameObject.GetComponent<Sensor> ();
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Checkpoint") {
			Checkpoint ch = other.gameObject.GetComponent<Checkpoint> ();
			if (!ch.passed) {
				ch.passed = true;
				Renderer tmp = other.gameObject.GetComponent<Renderer> ();
				tmp.material = Passed;
				checkpoints++;
				man.BuildNextRoadSegment ();
			}
			//Destroy (other.gameObject);
		} else {
			//hit a wall considered fail
			crashed = true;
			//StartCoroutine (Explode());
		}
	}

	IEnumerator Explode()
	{
		sensor.drawRays = false;
		agent.freezeMotion = true;
		agent.gameObject.GetComponent<Renderer>().enabled = false;
		Renderer[] rens = agent.gameObject.GetComponent<Renderer> ().GetComponentsInChildren<Renderer> ();
		foreach (Renderer r in rens)
			r.enabled = false;
		GameObject expl = Instantiate (explosion, transform.position, Quaternion.identity);
		yield return new WaitForSeconds (1);
		Destroy(expl);
		crashed = true;
		agent.gameObject.GetComponent<Renderer>().enabled = true;
		rens = agent.gameObject.GetComponent<Renderer> ().GetComponentsInChildren<Renderer> ();
		foreach (Renderer r in rens)
			r.enabled = true;
		agent.freezeMotion = false;
		sensor.drawRays = true;
	}

	public bool HasCrashed()
	{
		return crashed;
	}

	public int CheckPointsPassed()
	{
		return checkpoints;
	}

	public void Reset()
	{
		crashed = false;
		checkpoints = 0;
	}
		
}
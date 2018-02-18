using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

	public int DieSeconds = 5;

	// Use this for initialization
	void Start () {
		StartCoroutine (DestroySoon());
	}

	private IEnumerator DestroySoon()
	{
		yield return new WaitForSeconds (DieSeconds);
		Destroy(gameObject);
	}
}

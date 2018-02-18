using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Attached to empty object

public class Manager : MonoBehaviour {

	public const int POPULATION_SIZE = 15;

	private const int TOTAL_WEIGHTS = 5 * 8 + 8 * 2 + 8 + 2;
	public GameObject car;
	private Agent testAgent;

	private float currentAgentFitness;
	private float bestFitness;

	//Track Generation
	public GameObject TrackStart;
	public GameObject wall;
	public GameObject sphere;
	private Vector3 currCentreRoad, currLeftRoad, currRightRoad;
	private float currTheta;

	private Vector3 prevCentreRoad, prevprevCentreRoad, prevprevprevCentreRoad;
	private float prevTheta, prevprevTheta;

	public GameObject checkpoint;
	public float ROAD_WIDTH = 5f;
	public float ROAD_LENGTH_MIN = 0.1f;
	public float ROAD_LENGTH_MAX = 3f;

	private const int NO_OF_INPUTS = 4;
	private const int NO_OF_HIDDEN = 6;
	private const int NO_OF_OUTPUTS = 2;

	public NNet neuralNet;

	private GameObject carObject;
	private GeneticAlgorithm genAlg;
	public Material normal;

	public GUIStyle textStyle;

	public void OnGUI(){
		int x = 550;
		int y = 500;
		GUI.Label (new Rect (x, y, 200, 20), "CurrentFitness: " + Mathf.Round(currentAgentFitness), textStyle);
		GUI.Label (new Rect (x, y+20, 200, 20), "BestFitness: " + Mathf.Round(bestFitness), textStyle);
		GUI.Label (new Rect (x+200, y, 200, 20), "Genome: " + genAlg.GetCurrentGenomeIndex() + " of " + genAlg.GetTotalPopulation(), textStyle);
		GUI.Label (new Rect (x+200, y + 20, 200, 20), "Generation: " + genAlg.GetCurrentGeneration(), textStyle);
	}

	// Use this for initialization
	void Start () {
		genAlg = new GeneticAlgorithm ();
		genAlg.GenerateNewPopulation (POPULATION_SIZE, TOTAL_WEIGHTS);
		currentAgentFitness = 0.0f;
		bestFitness = 0.0f;

		neuralNet = new NNet ();
		neuralNet.CreateNet (1, NO_OF_INPUTS, NO_OF_HIDDEN, NO_OF_OUTPUTS);
		Genome genome = genAlg.GetNextGenome ();
		neuralNet.FromGenome (genome, NO_OF_INPUTS, NO_OF_HIDDEN, NO_OF_OUTPUTS);

		//Road Generation
		currCentreRoad = TrackStart.transform.position;
		currLeftRoad = currCentreRoad + new Vector3(0, 0, ROAD_WIDTH/2);
		currRightRoad = currCentreRoad + (currCentreRoad - currLeftRoad);
		currTheta = TrackStart.transform.eulerAngles.y * Mathf.PI/180; //convert to radions;

		carObject = Instantiate (car, currLeftRoad + ((currRightRoad - currLeftRoad)/2), Quaternion.identity);
		carObject.transform.Rotate(new Vector3(0, currTheta*(180/Mathf.PI), 0), Space.World);

		testAgent = carObject.GetComponent<Agent> ();
		testAgent.Attach (neuralNet);

		GameObject cam = GameObject.Find ("Main Camera");
		CameraMovement camMov = cam.GetComponent<CameraMovement> ();
		camMov.Target = carObject.transform;

		BuildNextRoadSegment ();
		BuildNextRoadSegment ();
		BuildNextRoadSegment ();
		BuildNextRoadSegment ();
	}

	// Update is called once per frame
	void Update () {
		if (testAgent.HasFailed()) {
			if(genAlg.GetCurrentGenomeIndex() == POPULATION_SIZE-1){
				genAlg.BreedPopulation ();
				NextTestSubject ();
				return;
			}
			NextTestSubject();
		}
		currentAgentFitness = testAgent.FitnessFunction();
		if (currentAgentFitness > bestFitness) {
			bestFitness = currentAgentFitness;
		}
	}

	//should call this when passed a checkpoint
	public void BuildNextRoadSegment()
	{
		prevprevprevCentreRoad = prevprevCentreRoad;
		prevprevCentreRoad = prevCentreRoad;
		prevCentreRoad = currCentreRoad;

		prevprevTheta = prevTheta; 
		prevTheta = currTheta;

		//If this angle is zero, it continues straight. Angle is from centre of road
		float randomAngle = Random.Range (-30, 30);
		//This length goes down the centre of the road
		float randomLength = Random.Range (ROAD_LENGTH_MIN, ROAD_LENGTH_MAX);
		//float randomLength = Random.Range ((ROAD_WIDTH/2)*Mathf.Sin(randomAngle), ROAD_LENGTH_MAX);

		randomAngle *= Mathf.PI/180; //convert to radions

		float leftLength, rightLength; 
		Vector3 newCentreRoad, newLeftRoad, newRightRoad;
		float newAngle = (currTheta + randomAngle);

		//keep it in the domain
		if (newAngle > Mathf.PI) 
			newAngle = newAngle - 2*Mathf.PI;
		else if (currTheta < -Mathf.PI) 
			newAngle = newAngle + 2*Mathf.PI;

		//Vector3 points on the ground
		newCentreRoad = currCentreRoad + new Vector3(randomLength*Mathf.Cos(newAngle), 0, -randomLength*Mathf.Sin(newAngle));

		//work out left and right from centre of road
		Vector3 halfRoad = new Vector3((ROAD_WIDTH/2)*Mathf.Sin(newAngle), 0, (ROAD_WIDTH/2)*Mathf.Cos(newAngle));

		newLeftRoad = newCentreRoad + halfRoad;
		newRightRoad = newCentreRoad - halfRoad;

		leftLength = (newLeftRoad - currLeftRoad).magnitude;
		rightLength = (newRightRoad - currRightRoad).magnitude;

		//To create the walls, need to specify centre point
		Vector3 leftWallCentre = currLeftRoad + ((newLeftRoad - currLeftRoad) / 2);
		Vector3 rightWallCentre = currRightRoad + ((newRightRoad - currRightRoad) / 2);	

		newAngle *= (180/Mathf.PI); //back to degrees

		Vector3 straightAhead;
		if (prevCentreRoad == currCentreRoad)
			straightAhead = new Vector3(1, 0, 0);
		else			
			straightAhead = currCentreRoad - prevCentreRoad;

		//work out angle in World Space
		Vector3 leftWall = newLeftRoad - currLeftRoad;
		float leftWallAngle = Mathf.Atan(-leftWall.z/leftWall.x);

		//Instantiate left wall
		GameObject wallObject;
		wallObject = Instantiate(wall, leftWallCentre, Quaternion.identity);
		wallObject.transform.localScale = new Vector3(leftLength, wallObject.transform.localScale.y, wallObject.transform.localScale.z);
		wallObject.transform.Rotate(new Vector3 (0, leftWallAngle*(180/Mathf.PI), 0), Space.World);

		Vector3 rightWall = newRightRoad - currRightRoad;
		float rightWallAngle = Mathf.Atan(-rightWall.z/rightWall.x);

		//Instantiate right wall
		wallObject = Instantiate(wall, rightWallCentre, Quaternion.identity);
		wallObject.transform.localScale = new Vector3(rightLength, wallObject.transform.localScale.y, wallObject.transform.localScale.z);
		wallObject.transform.Rotate(new Vector3 (0, rightWallAngle*(180/Mathf.PI), 0), Space.World);

		//create checkpoint from currLeftRoad to currRightRoad
		Vector3 cpPos = newLeftRoad + ((newRightRoad - newLeftRoad)/2);
		GameObject checkpointObject = Instantiate(checkpoint, cpPos, Quaternion.identity);
		checkpointObject.transform.localScale = new Vector3(ROAD_WIDTH, 0.2f, 0.2f);
		checkpointObject.transform.Rotate(new Vector3 (0, newAngle - 90, 0), Space.World);

		//update points
		currCentreRoad = newCentreRoad;
		currLeftRoad = newLeftRoad;
		currRightRoad = newRightRoad;
		currTheta += randomAngle; //in radians
		if (currTheta > Mathf.PI) 
			currTheta = currTheta - 2*Mathf.PI;
		else if (currTheta < -Mathf.PI) 
			currTheta = currTheta + 2*Mathf.PI;
	}

	public void NextTestSubject(){
		genAlg.SetGenomeFitness (currentAgentFitness, genAlg.GetCurrentGenomeIndex ());
		currentAgentFitness = 0.0f;
		Genome genome = genAlg.GetNextGenome ();
		 
		neuralNet.FromGenome (genome, NO_OF_INPUTS, NO_OF_HIDDEN, NO_OF_OUTPUTS);

		carObject.transform.position = prevprevprevCentreRoad;
		carObject.transform.rotation = Quaternion.identity;
		carObject.transform.Rotate(new Vector3(0, prevprevTheta*(180/Mathf.PI), 0), Space.World);

		testAgent.Attach (neuralNet);
		testAgent.Reset();
	}

}

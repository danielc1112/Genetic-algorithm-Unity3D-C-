using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NNet {
	private int outputAmount;

	List<float> inputs = new List<float>();
	NLayer inputlayer = new NLayer();

	List<NLayer> hiddenLayers = new List<NLayer>();
	NLayer outputLayer = new NLayer();

	List<float> outputs = new List<float> ();

	public void refresh(){
		outputs.Clear ();

		for (int i=0; i < hiddenLayers.Count; i++) {
			if(i > 0){
				inputs = outputs;
			}
			hiddenLayers[i].Evaluate(inputs, ref outputs);

		}
		inputs = outputs;
		//Process the layeroutputs through the output layer to
		outputLayer.Evaluate (inputs, ref outputs);

	}

	public void SetInput(List<float> input){
		inputs = input;
	}

	public float GetOutput(int ID){
		if (ID >= outputAmount)
			return 0.0f;
		return outputs [ID];
	}

	public int GetTotalOutputs() {
		return outputAmount;
	}

	public void CreateNet(int numOfHIddenLayers, int numOfInputs, int NeuronsPerHidden, int numOfOutputs){
		outputAmount = numOfOutputs;

		for(int i=0; i<numOfHIddenLayers; i++){
			NLayer layer = new NLayer();
			layer.PopulateLayer(NeuronsPerHidden, numOfInputs);
			hiddenLayers.Add (layer);
		}

		outputLayer = new NLayer ();
		outputLayer.PopulateLayer (numOfOutputs, NeuronsPerHidden);
	}

	public void ReleaseNet(){
		if (inputlayer != null) {
			inputlayer = null;
			inputlayer = new NLayer();
		}
		if (outputLayer != null) {
			outputLayer = null;
			outputLayer = new NLayer();
		}
		for (int i=0; i<hiddenLayers.Count; i++) {
			if(hiddenLayers[i]!=null){
				hiddenLayers[i] = null;
			}
		}
		hiddenLayers.Clear ();
		hiddenLayers = new List<NLayer> ();
	}

	public int GetNumofHIddenLayers(){
		return hiddenLayers.Count;
	}

	public Genome ToGenome(){
		Genome genome = new Genome ();

		for (int i=0; i<this.hiddenLayers.Count; i++) {
			List<float> weights = new List<float> ();
			hiddenLayers[i].GetWeights(ref weights);
			for(int j=0; j<weights.Count;j++){
				genome.weights.Add (weights[j]);
			}
		}

		List<float> outweights = new List<float> ();
		outputLayer.GetWeights(ref outweights);
		for (int i=0; i<outweights.Count; i++) {
			genome.weights.Add (outweights[i]);
		}

		return genome;
	}

	public void FromGenome(Genome genome, int numofInputs, int neuronsPerHidden, int numOfOutputs){
		ReleaseNet ();

		outputAmount = numOfOutputs;

		NLayer hidden = new NLayer ();

		List<Neuron> neurons = new List<Neuron>();

		for(int i=0; i<neuronsPerHidden; i++){
			neurons.Add(new Neuron());
			List<float> weights = new List<float>();

			for(int j=0; j<numofInputs+1;j++){
				weights.Add(0.0f);
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			neurons[i].weights = new List<float>();
			neurons[i].Initilise(weights, numofInputs);
		}
		hidden.LoadLayer (neurons);
		this.hiddenLayers.Add (hidden);

		//Clear weights and reasign the weights to the output
		List<Neuron> outneurons = new List<Neuron> ();

		for(int i=0; i<numOfOutputs; i++){
			outneurons.Add(new Neuron());

			List<float> weights = new List<float>();

			for(int j=0; j<neuronsPerHidden + 1; j++){
				weights.Add (0.0f);
				weights[j] = genome.weights[i*neuronsPerHidden + j];
			}
			outneurons[i].weights = new List<float>();
			outneurons[i].Initilise(weights, neuronsPerHidden);
		}
		this.outputLayer = new NLayer ();
		this.outputLayer.LoadLayer (outneurons);
	}
}


//=================================================================================================================
public class NLayer {

	private int totalNeurons;


	List<Neuron> neurons = new List<Neuron>();

	public float Sigmoid(float a, float p) {
		float ap = (-a) / p;
		return (1 / (1 + Mathf.Exp (ap)));
	}

	public float BiPolarSigmoid(float a, float p){
		float ap = (-a) / p;
		return (2 / (1 + Mathf.Exp (ap)) - 1);
	}

	public void Evaluate(List<float> input, ref List<float> output){
		int inputIndex = 0;
		//cycle over all the neurons and sum their weights against the inputs
		for (int i=0; i< totalNeurons; i++) {
			float activation = 0.0f;

			//sum the weights to the activation value
			//we do the sizeof the weights - 1 so that we can add in the bias to the activation afterwards.
			for(int j=0; j< neurons[i].numInputs - 1; j++){

				activation += input[inputIndex] * neurons[i].weights[j];
				inputIndex++;
			}

			//add the bias
			//the bias will act as a threshold value to
			activation += neurons[i].weights[neurons[i].numInputs] * (-1.0f);//BIAS == -1.0f

			output.Add(Sigmoid(activation, 1.0f));
			inputIndex = 0;
		}
	}

	public void LoadLayer(List<Neuron> input){
		totalNeurons = input.Count;
		neurons = input;
	}

	public void PopulateLayer(int numOfNeurons, int numOfInputs){
		totalNeurons = numOfNeurons;

		if (neurons.Count < numOfNeurons) {
			for(int i=0; i<numOfNeurons; i++){
				neurons.Add(new Neuron());
			}
		}

		for(int i=0; i<numOfNeurons; i++){
			neurons[i].Populate(numOfInputs);
		}
	}

	public void SetWeights(List<float> weights, int numOfNeurons, int numOfInputs){
		int index = 0;
		totalNeurons = numOfNeurons;

		if (neurons.Count < numOfNeurons) {
			for (int i=0; i<numOfNeurons - neurons.Count; i++){
				neurons.Add(new Neuron());
			}
		}
		//Copy the weights into the neurons.
		for (int i=0; i<numOfNeurons; i++) {
			if(neurons[i].weights.Count < numOfInputs){
				for(int k=0; k<numOfInputs-neurons[i].weights.Count; k++){
					neurons[i].weights.Add (0.0f);
				}
			}
			for(int j=0; j<numOfInputs; j++){
				neurons[i].weights[j] = weights[index];
				index++;
			}
		}
	}

	public void GetWeights(ref List<float> output){
		//Calculate the size of the output list by calculating the amount of weights in each neurons.
		output.Clear ();

		for (int i=0; i<this.totalNeurons; i++) {
			for(int j=0; j<neurons[i].weights.Count; j++){
				output[totalNeurons*i + j] = neurons[i].weights[j];
			}
		}
	}

	public void SetNeurons(List<Neuron> neurons, int numOfNeurons, int numOfInputs){
		totalNeurons = numOfNeurons;
		this.neurons = neurons;
	}
}


//=============================================================
public class Neuron {
	public int numInputs;
	public List<float> weights = new List<float>();


	public float RandomFloat()
	{
		float rand = (float)Random.Range (0.0f, 32767.0f);
		return rand / 32767.0f/*32767*/ + 1.0f;
	}

	public float RandomClamped()
	{
		return RandomFloat() - RandomFloat();
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

	public void Populate(int num){
		this.numInputs = num;

		//Initilise the weights
		for (int i=0; i < num; i++){
			weights.Add(RandomClamped());
		}

		//add an extra weight as the bias (the value that acts as a threshold in a step activation).
		weights.Add (RandomClamped ());
	}

	public void Initilise(List<float> weightsIn, int num){
		this.numInputs = num;
		weights = weightsIn;
	}
}
	
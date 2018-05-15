using UnityEngine.UI;
using UnityEngine;
using System;

[Serializable]
public struct FPSColor
{
	public Color color;
	public int minimumFPS;
}

public class FPSUtility : MonoBehaviour
{
	public int frameRange = 60;
	public Text highestFPSLabel, averageFPSLabel, lowestFPSLabel;
	public FPSColor[] coloring = new FPSColor[0];

	int[] fpsBuffer;
	int fpsBufferIndex;

	public int AverageFPS { get; private set; }

	public int HighestFPS { get; private set; }

	public int LowestFPS { get; private set; }

	void Start ()
	{
		InitializeBuffer ();
		DontDestroyOnLoad (gameObject);
	}

	void Update ()
	{
		UpdateBuffer ();
		CalculateFPS ();

		Display (highestFPSLabel, HighestFPS);
		Display (averageFPSLabel, AverageFPS);
		Display (lowestFPSLabel, LowestFPS);
	}

	void InitializeBuffer ()
	{
		if (frameRange <= 0) {
			frameRange = 1;
		}
		fpsBuffer = new int[frameRange];
		fpsBufferIndex = 0;
	}

	void UpdateBuffer ()
	{
		fpsBuffer [fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
		if (fpsBufferIndex >= frameRange) {
			fpsBufferIndex = 0;
		}
	}

	void CalculateFPS ()
	{
		int sum = 0;
		int highest = 0;
		int lowest = int.MaxValue;
		for (int i = 0; i < frameRange; i++) {
			int fps = fpsBuffer [i];
			sum += fps;
			if (fps > highest) {
				highest = fps;
			}
			if (fps < lowest) {
				lowest = fps;
			}
		}
		AverageFPS = (int)((float)sum / frameRange);
		HighestFPS = highest;
		LowestFPS = lowest;
	}

	void Display (Text label, int fps)
	{
		label.text = fps.ToString ();
		for (int i = 0; i < coloring.Length; i++) {
			if (fps >= coloring [i].minimumFPS) {
				label.color = coloring [i].color;
				break;
			}
		}
	}
}

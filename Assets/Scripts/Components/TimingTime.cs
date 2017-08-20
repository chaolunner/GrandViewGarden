using UnityEngine;
using UnityEngine.UI;
using UniECS;

public class TimingTime : ComponentBehaviour
{
	public  int hour;
	public  int minute;
	public  int second;
	public  int millisecond;

	public  float timeSpeed = 0.0f;
	public  Text text_timeSpeed;

//	void Start ()
//	{
//		text_timeSpeed = GetComponent<Text> ();
//	}
//
//	void Update ()
//	{
//		timeSpeed += Time.deltaTime;	
//
//		hour = (int)timeSpeed / 3600;
//		minute = ((int)timeSpeed - hour * 3600) / 60;
//		second = (int)timeSpeed - hour * 3600 - minute * 60;
//		millisecond = (int)((timeSpeed - (int)timeSpeed) * 100);
//
//		text_timeSpeed.text = string.Format ("{0:D2}.{1:D2}", second, millisecond);
//	}
}

using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		_slider1 = Mathf.Lerp (-10, 10f, _rule1Slider.value);
		_slider2 = Mathf.Lerp (-5, 5f, _rule2Slider.value);
		_slider3 = Mathf.Lerp (-2f, 2f, _rule3Slider.value);
		_slider4 = Mathf.Lerp (0, 100f, _velocitySlider.value);
	}

	public UISlider _rule1Slider;
	public UISlider _rule2Slider;
	public UISlider _rule3Slider;
	public UISlider _velocitySlider;
	private float _slider1;
	private float _slider2;
	private float _slider3;
	private float _slider4;

	public float Slider1
	{
		get
		{
			return _slider1;
		}
	}

	public float Slider2
	{
		get
		{
			return _slider2;
		}
	}

	public float Slider3
	{
		get
		{
			return _slider3;
		}
	}

	public float Slider4
	{
		get
		{
			return _slider4;
		}
	}
}

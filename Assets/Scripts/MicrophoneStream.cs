using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public sealed class MicrophoneStream : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private float _smoothing = 0.5f;
	[SerializeField] private int _bandwidth = 256;

	[SerializeField] private Transform _feedback;

	[SerializeField] private TMP_Text _avgText;
	[SerializeField] private TMP_Text _peakText;

	public float RunningAverage => _runningAverage;

	private float[] _sampleValues;
	private float _runningAverage;
	private float _peak;

	private void Awake()
	{
		_sampleValues = new float[_bandwidth];
	}

	private IEnumerator Start()
	{
		Application.HasUserAuthorization(UserAuthorization.Microphone);
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

		if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
		{
			Permission.RequestUserPermission(Permission.Microphone);
		}

		var device = Microphone.devices.FirstOrDefault();

		_audioSource.clip = Microphone.Start(device, true, 1, AudioSettings.outputSampleRate);
		_audioSource.loop = true;

		yield return new WaitUntil(() => Microphone.GetPosition(device) > 0);

		Debug.Log($"Started recording: {device} {AudioSettings.outputSampleRate}");
		_audioSource.Play();
	}

	private void Update()
	{
		var avg = 0f;
		_audioSource.GetOutputData(_sampleValues, 0);
		for (var i = 0; i < _sampleValues.Length - 1; i++)
		{
			avg += Mathf.Abs(_sampleValues[i] * 1f) / _sampleValues.Length;
		}

		_runningAverage = Mathf.Lerp(_runningAverage, avg, 1 - Mathf.Pow(_smoothing, Time.deltaTime));
		_peak = Mathf.Max(_peak, _runningAverage);

		if (_feedback != null) _feedback.localScale = new Vector3(_runningAverage, _runningAverage, _runningAverage);
		if (_avgText != null) _avgText.text = $"Avg: {_runningAverage:0.000}";
		if (_peakText != null) _peakText.text = $"Peak: {_peak:0.000}";
	}
}

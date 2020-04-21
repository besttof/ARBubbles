using System.Collections;
using System.Linq;
using UnityEngine;

public sealed class MicrophoneInput : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private int _samplesSize = 128;

	public float Value { get; private set; }
	public string Device { get; private set; }
	public float Peak { get; private set; }

	public bool IsListening => _audioSource.isPlaying;

	private float[] _sampleValues;

	private void Awake()
	{
		_sampleValues = new float[_samplesSize];
	}

	private void OnEnable()
	{
		StartMic();
	}

	private void OnDisable()
	{
		StopMic();
	}

	private void Update()
	{
		if (_audioSource.clip == null) return;
		var pos = Microphone.GetPosition(Device) - _sampleValues.Length + 1;

		// Brute force, but instead of managing wrapped buffers (due to the looping). Just skip updating for the couple
		// of invalid frames. This is not pretty, but hardly noticeable for users.
		if (pos < 0) return;

		Value = 0;
		_audioSource.clip.GetData(_sampleValues, pos);

		for (var i = 0; i < _sampleValues.Length - 1; i++)
		{
			//Value = Mathf.Max(Value, _sampleValues[i]);
			Value += _sampleValues[i] * _sampleValues[i];
		}

		Value /= _sampleValues.Length;

#if UNITY_EDITOR
		Value = Input.GetKey(KeyCode.B) ? 0.5f : Value;
#endif
		Peak = Mathf.Max(Peak, Value);
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (isPaused)
		{
			StopMic();
		}
		else
		{
			StartMic();
		}
	}

	public void StartMic()
	{
		StartCoroutine(StartMicRoutine());
	}

	public void StopMic()
	{
		Value = 0f;
		Microphone.End(Device);
		_audioSource.Stop();
	}

	private IEnumerator StartMicRoutine()
	{
		Device = Microphone.devices.FirstOrDefault();

		_audioSource.clip = Microphone.Start(Device, true, 1, AudioSettings.outputSampleRate);
		_audioSource.loop = true;

		yield return new WaitUntil(() => Microphone.GetPosition(Device) > 0);

		Debug.Log($"Started recording: {Device} {AudioSettings.outputSampleRate}");

		_audioSource.Play();
	}
}

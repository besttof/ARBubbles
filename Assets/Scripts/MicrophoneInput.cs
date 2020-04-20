using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public sealed class MicrophoneInput : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private int _samplesSize = 128;

	[SerializeField] private TMP_Text _peakText;
	[SerializeField] private TMP_Text _maxText;
	[SerializeField] private TMP_Text _positionText;

	public float Value { get; private set; }
	public string Device { get; private set; }
	public float Peak { get; private set; }

	private float[] _sampleValues;

	private void Awake()
	{
		_sampleValues = new float[_samplesSize];
	}

	private IEnumerator Start()
	{
		// TODO proper permissions
		Application.HasUserAuthorization(UserAuthorization.Microphone);
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

		if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
		{
			Permission.RequestUserPermission(Permission.Microphone);
		}

		yield return StartCoroutine(StartMicRoutine());
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

	private void StopMic()
	{
		Value = 0f;
		Microphone.End(Device);
		_audioSource.clip = null;
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
			Value = Mathf.Max(Value, _sampleValues[i]);
		}

#if UNITY_EDITOR
		Value = Input.GetKey(KeyCode.B) ? 0.5f : Value;
#endif
		Peak = Mathf.Max(Peak, Value);

		if (_peakText != null) _peakText.text = $"Peak: {Peak:0.000}";
		if (_positionText != null) _positionText.text = $"Position: {Microphone.GetPosition(Device)}";
		if (_maxText != null) _maxText.text = $"Peak: {Value:0.000}";
	}
}

using TMPro;
using UnityEngine;

public sealed class MicrophoneInputInfo : MonoBehaviour
{
	[SerializeField] private MicrophoneInput _microphone;
	
	[SerializeField] private TMP_Text _peakText;
	[SerializeField] private TMP_Text _valueText;
	[SerializeField] private TMP_Text _deviceText;

	private void Update()
	{
		_deviceText.text = $"Device: {_microphone.Device}";
		_peakText.text = $"Peak: {_microphone.Peak:0.000}";
		_valueText.text = $"Value: {_microphone.Value:0.000}";
	}
}
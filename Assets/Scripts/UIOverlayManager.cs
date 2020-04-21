using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public sealed class UIOverlayManager : MonoBehaviour
{
	[SerializeField] private GameObject _waitingOverlay;
	[SerializeField] private GameObject _micCTAOverlay;

	[SerializeField] private MicrophoneInput _mic;

	private void OnEnable()
	{
		StartCoroutine(StateMachine());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private IEnumerator StateMachine()
	{
		_waitingOverlay.SetActive(false);
		_micCTAOverlay.SetActive(false);
		_mic.enabled = false;

		while (enabled)
		{
			yield return WaitForReadyState();
			yield return WaitMicReadyState();
			yield return ActiveMicState();
		}
	}

	private IEnumerator WaitForReadyState()
	{
		_waitingOverlay.SetActive(true);

		while (ARSession.state != ARSessionState.Ready)
		{
			yield return null;
		}

		_waitingOverlay.SetActive(false);
	}

	private IEnumerator WaitMicReadyState()
	{
		_mic.enabled = true;

		while (_mic.IsListening == false)
		{
			yield return null;
		}
	}

	private IEnumerator ActiveMicState()
	{
		_micCTAOverlay.SetActive(true);

		while (_mic.IsListening)
		{
			yield return null;
		}

		_micCTAOverlay.SetActive(false);
	}
}
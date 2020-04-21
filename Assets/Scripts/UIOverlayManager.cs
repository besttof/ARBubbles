using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public sealed class UIOverlayManager : MonoBehaviour
{
	[SerializeField] private GameObject _waitingOverlay;
	[SerializeField] private GameObject _micCTAOverlay;

	[SerializeField] private TMP_Text _status;

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

		while (ARSession.state.IsUpAndRunning() == false)
		{
			_status.text = $"ARSession: {ARSession.state}";
			yield return null;
		}
		_status.text = "";
		_waitingOverlay.SetActive(false);
	}

	private IEnumerator WaitMicReadyState()
	{
		_mic.enabled = true;

		while (_mic.IsListening == false)
		{
			_status.text = $"Mic: {_mic.IsListening}";
			yield return null;
		}

		_status.text = "";
	}

	private IEnumerator ActiveMicState()
	{
		_micCTAOverlay.SetActive(true);

		while (_mic.IsListening && ARSession.state.IsUpAndRunning())
		{
			yield return null;
		}

		_micCTAOverlay.SetActive(false);
	}
}


public static class ARSessionStateExt
{
	public static bool IsUpAndRunning(this ARSessionState state)
	{
		#if UNITY_EDITOR
		return Input.GetKey(KeyCode.A);
		#endif
		return state == ARSessionState.Ready || state == ARSessionState.SessionTracking || state == ARSessionState.SessionInitializing;
	}
}

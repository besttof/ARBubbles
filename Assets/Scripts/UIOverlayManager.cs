using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

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
			_status.text = $"Waiting for AR to kick in\nCurrent state: {ARSession.state}";
			yield return null;
		}
		_status.text = "";
		_waitingOverlay.SetActive(false);
	}

	private IEnumerator WaitMicReadyState()
	{
		// _Very_ rudimentary permission checking.
		_waitingOverlay.SetActive(true);
		_status.text = $"Waiting microphone permission.";

#if UNITY_ANDROID
		while (Permission.HasUserAuthorizedPermission(Permission.Microphone) == false)
		{
			_status.text = $"Waiting microphone permission.";
			// this call is blocking, apparently, so we can ask for this in a loop 😈
			Permission.RequestUserPermission(Permission.Microphone);
			yield return null;
		}
#elif UNITY_IOS
		// Apparently, the web permission api is supposed to work on iOS.
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

		while (Application.HasUserAuthorization(UserAuthorization.Microphone) == false)
		{
			_status.text = $"Microphone permission denied, go to the settings to enable it.";
			yield return null;
		}
#endif

		_mic.enabled = true;

		while (_mic.IsListening == false)
		{
			_status.text = $"Waiting for microphone to come alive";
			yield return null;
		}

		_status.text = "";
		_waitingOverlay.SetActive(false);
	}

	private IEnumerator ActiveMicState()
	{
		_micCTAOverlay.SetActive(true);
		_status.text = $"";

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

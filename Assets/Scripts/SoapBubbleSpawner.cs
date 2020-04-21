using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SoapBubbleSpawner : MonoBehaviour
{
	[SerializeField] private ARSessionOrigin _origin;
	[SerializeField] private Transform _arRoot;

	[SerializeField] private MicrophoneInput _microphone;
	[SerializeField] private SoapBubble _bubble;
	[SerializeField] private Transform _bubbleSpawnHolder;

	[SerializeField] private float _spawnThreshold = 0.4f;
	[SerializeField] private float _releaseForce = 0.1f;
	[SerializeField] private Vector3 _releaseDir = new Vector3(0, 1f, 0.3f);
	[SerializeField] private float _growSpeed = 0.01f;
	[SerializeField] private float _maxSize = 0.1f;
	[SerializeField] private float _maxTime = 2f;
	[SerializeField] private float _intervalTime = 0.2f;

	private SoapBubble _bubbleInProgress;
	private Coroutine _spawnRoutine;

	private void Reset()
	{
		_microphone = GetComponent<MicrophoneInput>();
	}

	private void OnEnable()
	{
		_spawnRoutine = StartCoroutine(SpawnRoutine());
		_origin.MakeContentAppearAt(_arRoot, Vector3.zero);
	}

	private void OnDisable()
	{
		if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
	}

	private IEnumerator SpawnRoutine()
	{
		while (enabled)
		{
			while (_microphone.Value < _spawnThreshold) yield return null;

			_bubbleInProgress = Instantiate(_bubble, _bubbleSpawnHolder, false);

			{
				var size = 0.01f;
				var startTime = Time.realtimeSinceStartup;

				while (_microphone.Value > _spawnThreshold && size < _maxSize &&
				       Time.realtimeSinceStartup - _maxTime < startTime)
				{
					size += _microphone.Value * _growSpeed;

					_bubbleInProgress.SetSize(size);
					yield return null;
				}
			}

			_bubbleInProgress.Release((_bubbleSpawnHolder.TransformDirection(_releaseDir).normalized) * _releaseForce, _origin.trackablesParent);
			_bubbleInProgress = null;

			{
				var startTime = Time.realtimeSinceStartup;
				while (_microphone.Value > _spawnThreshold && Time.realtimeSinceStartup - _intervalTime < startTime) yield return null;
			}
		}
	}
}

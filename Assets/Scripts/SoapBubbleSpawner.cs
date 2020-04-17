using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SoapBubbleSpawner : MonoBehaviour
{
	[SerializeField] private MicrophoneStream _microphone;
	[SerializeField] private SoapBubble _bubble;
	[SerializeField] private Transform _bubbleSpawnHolder;

	[SerializeField] private float _spawnThreshold = 0.4f;
	[SerializeField] private float _releaseForce = 0.1f;
	[SerializeField] private Vector3 _releaseDir = new Vector3(0,1f,0.3f);
	[SerializeField] private float _growSpeed = 0.01f;
	[SerializeField] private float _maxSize = 0.1f;

	private SoapBubble _bubbleInProgress;
	private Coroutine _spawnRoutine;

	private void Reset()
	{
		_microphone = GetComponent<MicrophoneStream>();
	}

	private void OnEnable()
	{
		_spawnRoutine = StartCoroutine(SpawnRoutine());
	}

	private void OnDisable()
	{
		if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
	}

	private IEnumerator SpawnRoutine()
	{
		while (enabled)
		{
			while (_microphone.RunningAverage < _spawnThreshold) yield return null;

			_bubbleInProgress = Instantiate(_bubble, _bubbleSpawnHolder, false);
			var size = 0.01f;
			while (_microphone.RunningAverage > _spawnThreshold && size < _maxSize)
			{
				size += _microphone.RunningAverage * _growSpeed;

				_bubbleInProgress.SetSize(size);
				yield return null;
			}

			_bubbleInProgress.Release((_bubbleSpawnHolder.TransformDirection(_releaseDir).normalized)* _releaseForce);
			_bubbleInProgress = null;

			while (_microphone.RunningAverage > _spawnThreshold) yield return null;
		}
	}
}

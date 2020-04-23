using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class SoapBubbleSpawner : MonoBehaviour
{
	[SerializeField] private ARSessionOrigin _origin;
	[SerializeField] private Transform _arRoot;

	[SerializeField] private MicrophoneInput _microphone;

	[SerializeField] private Bubble[] _bubbles;
	[SerializeField] private Transform _bubbleSpawnHolder;
	[SerializeField] private float _spawnThreshold = 0.4f;
	[SerializeField] private float _releaseForce = 0.1f;
	[SerializeField] private Vector3 _releaseDir = new Vector3(0, 1f, 0.3f);
	[SerializeField] private float _growSpeed = 0.01f;
	[SerializeField] private float _maxSize = 0.1f;
	[SerializeField] private float _maxTime = 2f;
	[SerializeField] private float _intervalTime = 0.2f;

	[Serializable]
	private class Bubble
	{
		[SerializeField] private int _weight;
		[SerializeField] private SoapBubble _prefab;

		public int Weight => _weight;
		public SoapBubble Prefab => _prefab;
	}

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

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void OnDisable()
	{
		if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}

	private IEnumerator SpawnRoutine()
	{
		while (enabled)
		{
			while (_microphone.Value < _spawnThreshold) yield return null;

			_bubbleInProgress = SpawnBubble();

			{
				var size = 0.01f;
				var startTime = Time.realtimeSinceStartup;

				while (_microphone.Value > _spawnThreshold &&
				       size < _maxSize &&
				       Time.realtimeSinceStartup - _maxTime < startTime)
				{
					size += _microphone.Value * _growSpeed;

					_bubbleInProgress.SetSize(size);
					yield return null;
				}
			}

			_bubbleInProgress.Release((_bubbleSpawnHolder.TransformDirection(_releaseDir).normalized) * _releaseForce);
			_bubbleInProgress = null;

			{
				var startTime = Time.realtimeSinceStartup;
				while (_microphone.Value > _spawnThreshold && Time.realtimeSinceStartup - _intervalTime < startTime) yield return null;
			}
		}
	}

	private SoapBubble SpawnBubble()
	{
		var totalWeight = _bubbles.Sum(b => b.Weight);
		var pick = Random.Range(0, totalWeight);
		var acc = 0;

		foreach (var b in _bubbles)
		{
			acc += b.Weight;
			if (pick < acc) return Instantiate(b.Prefab, _bubbleSpawnHolder, false);
		}

		Debug.LogAssertion("Somehow the weighted random didn't work out, this shouldn't happen but for now we'll fall back to the first bubble");
		return Instantiate(_bubbles[0].Prefab, _bubbleSpawnHolder, false);
	}
}

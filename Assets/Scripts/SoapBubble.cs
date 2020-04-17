using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoapBubble : MonoBehaviour
{
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private SphereCollider _rangeTrigger;

	[Range(0f, 0.1f)]
	[SerializeField] private float _noiseAmplitude = 0.1f;
	[Range(0f, 50f)]
	[SerializeField] private float _noiseScale = 0.1f;

	[SerializeField] private float _repelForce = 0.1f;
	[SerializeField] private float _attractForce = 0.1f;
	[SerializeField] private ForceMode _attractForceMode;
	[SerializeField] private ForceMode _repelForceMode;

	public float Radius => transform.localScale.x;

	private readonly HashSet<SoapBubble> _bubbles = new HashSet<SoapBubble>();
	private Vector3 _noiseOffset;

	private void Reset()
	{
		_rigidbody = GetComponentInChildren<Rigidbody>();
		_noiseOffset = Random.insideUnitSphere;
	}

	private void OnValidate()
	{
		if (_rangeTrigger != null) _rangeTrigger.isTrigger = true;
	}

	private void Start()
	{
		_rigidbody.isKinematic = true;
	}

	private void FixedUpdate()
	{
		_rigidbody.AddForce(-Physics.gravity);

		_bubbles.RemoveWhere(b => b == null);
		foreach (var bubble in _bubbles)
		{
			ApplyBubbleForces(bubble);
		}
	}

	public void OnMouseDown() => Pop();

	private void OnCollisionEnter(Collision other) => Pop();

	private void OnTriggerEnter(Collider other)
	{
		var bubble = other.GetComponent<SoapBubble>();
		if (bubble != null)
		{
			_bubbles.Add(bubble);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var bubble = other.GetComponent<SoapBubble>();
		if (bubble != null)
		{
			_bubbles.Remove(bubble);
		}
	}

	public void SetSize(float size)
	{
		var s = Mathf.Max(0.01f, size);
		transform.localScale = new Vector3(s, s, s);
	}

	public void Release(Vector3 forward = default)
	{
		transform.SetParent(null, true);

		_rigidbody.isKinematic = false;
		_rigidbody.WakeUp();
		_rigidbody.AddForce(forward, ForceMode.VelocityChange);

		StartCoroutine(StartPopRoutine());
	}

	private void ApplyBubbleForces(SoapBubble other)
	{
		if (other == null) return;

		var delta = transform.position - other.transform.position + Random.onUnitSphere * 0.01f;
		var distance = delta.magnitude;

		var totalRadius = (Radius + other.Radius);
		if (distance < totalRadius)
		{
			_rigidbody.AddForce(delta.normalized * _repelForce / distance, _repelForceMode);
		}
		else
		{
			var surfaceDistance = Mathf.Max(0f, distance - Radius);
			_rigidbody.AddForce(-delta.normalized * _attractForce / surfaceDistance, _attractForceMode);
		}
	}

	private void Pop()
	{
		Destroy(gameObject);
	}

	private float GetNoise(Vector3 position)
	{
		var p = (position + _noiseOffset) * _noiseScale;
		var noise = (Mathf.PerlinNoise(p.x, p.z) - 0.5f) * _noiseAmplitude;
		return noise;
	}

	private IEnumerator StartPopRoutine()
	{
		yield return new WaitForSeconds(Random.Range(5f, 12f));

		Pop();
	}
}

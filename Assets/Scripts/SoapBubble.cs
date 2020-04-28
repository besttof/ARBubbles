using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoapBubble : MonoBehaviour
{
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private SphereCollider _rangeTrigger;

	[SerializeField] private GameObject _popEffectPrefab;

	[SerializeField] private float _repelForce = 0.1f;
	[SerializeField] private float _attractForce = 0.1f;
	[SerializeField] private ForceMode _attractForceMode;
	[SerializeField] private ForceMode _repelForceMode;

	[SerializeField] private float _mergeDistanceThreshold = 0.1f;

	public float Radius => transform.localScale.x;
	public bool IsFree => _rigidbody.isKinematic == false;

	private readonly HashSet<SoapBubble> _bubbles = new HashSet<SoapBubble>();

	private void Reset()
	{
		_rigidbody = GetComponentInChildren<Rigidbody>();
	}

	private void OnValidate()
	{
		if (_rangeTrigger != null) _rangeTrigger.isTrigger = true;
	}

	private void Start()
	{
		_rigidbody.isKinematic = true;
		_rigidbody.rotation = Random.rotation;
	}

	private void FixedUpdate()
	{
		_rigidbody.AddForce(-Physics.gravity);

		// because destruction doesn't trigger physics events, clean up manually
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

	public void Release(Vector3 forward = default, Transform origin = null)
	{
		transform.SetParent(origin, true);

		_rigidbody.isKinematic = false;
		_rigidbody.WakeUp();
		_rigidbody.AddForce(forward, ForceMode.VelocityChange);
		_rigidbody.AddTorque(Random.onUnitSphere * 100f, ForceMode.Acceleration);

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

			if (other.IsFree && distance < _mergeDistanceThreshold) MergeWithBubble(other);
		}
		else
		{
			var surfaceDistance = Mathf.Max(0f, distance - Radius);
			_rigidbody.AddForce(-delta.normalized * _attractForce / surfaceDistance, _attractForceMode);
		}
	}

	private void MergeWithBubble(SoapBubble other)
	{
		DOTween.Kill(this);
		transform.DOScale(Radius + other.Radius, 0.1f)
		         .SetSpeedBased()
		         .SetEase(Ease.OutElastic)
		         .SetId(this);

		other.Pop();
	}

	private void Pop()
	{
		var t = transform;
		var popEffect = Instantiate(_popEffectPrefab, t.position, t.rotation, t.parent);
		popEffect.transform.localScale = t.localScale;
		Destroy(gameObject);
	}

	private IEnumerator StartPopRoutine()
	{
		yield return new WaitForSeconds(Random.Range(5f, 12f));

		Pop();
	}
}

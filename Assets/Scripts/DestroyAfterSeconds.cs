using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
	[SerializeField] private float _destroyTime;

	private void Start()
	{
		Destroy(gameObject, _destroyTime);
	}
}
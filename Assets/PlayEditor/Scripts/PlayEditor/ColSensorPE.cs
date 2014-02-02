using UnityEngine;

public class ColSensorPE : MonoBehaviour 
{
	void OnCollisionEnter(Collision c)
	{
		if (transform.rigidbody != null)
		{
			Destroy(transform.rigidbody);
			Destroy(this);
		}
	}
}

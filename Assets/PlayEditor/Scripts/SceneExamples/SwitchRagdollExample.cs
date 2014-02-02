using UnityEngine;
using System.Collections.Generic;

public class SwitchRagdollExample : MonoBehaviour 
{
	public KeyCode switchKey = KeyCode.O;
	List<Rigidbody> rigs = new List<Rigidbody>();

	// Use this for initialization
	void Start () 
	{
		GetT(GetComponent<Transform>());
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(switchKey) && rigs != null)
		{
			for(int i=0; i<rigs.Count; i++)
			{
				rigs[i].isKinematic = !rigs[i].isKinematic;
				rigs[i].WakeUp();
			}
		}
	}
	
	void GetT (Transform tf)
	{
		foreach(Transform t in tf)
		{
			GetT(t);
			if(t.rigidbody)
			{
				rigs.Add(t.rigidbody);
				t.rigidbody.isKinematic = true;
			}
		}
	}
}

using UnityEngine;
using System.Collections;

public class DestructionExample : MonoBehaviour 
{
	public GameObject goIntact = null;
	public GameObject goPieces = null;
	
	public KeyCode triggerButton = KeyCode.O;
	public bool useLightProbesForPieces = true;
	Animation a;
	bool isReady = false;
	
	// Use this for initialization
	void Start () 
	{
		if(goIntact != null && goPieces != null)
		{
			isReady = true;
			SetActiveAll(goIntact,true);
			SetActiveAll(goPieces,false);
			if(goPieces.animation != null)
				a = goPieces.animation;
			if(useLightProbesForPieces)
			{
				foreach(Transform t in goPieces.transform)
					t.GetComponent<MeshRenderer>().useLightProbes = true;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(isReady && Input.GetKeyDown(triggerButton)) 
		{
			SetActiveAll(goIntact,false);
			SetActiveAll(goPieces,true);
			if(a != null)
			{
				a.Play();
			}
			isReady = false;
		}
	}
	
	static void SetActiveAll (GameObject g, bool active)
	{
		if(g != null)
		{
			if(g.transform.childCount != 0)
			{
				foreach(Transform t in g.transform)
					t.gameObject.active = active;
			}
			g.active = active;
		}
	}
}

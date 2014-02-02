#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public delegate void RecordDeleg (bool rec, bool del, float pulse, bool pos, bool rot, bool scale);

[RequireComponent(typeof(Animation))]
public class RecorderPE : MonoBehaviour 
{
	
	public static RecordDeleg doRecDel = null;
	public float keyTimeInterval = 0.5f;
	public bool recPosition = true;
	public bool recRotation = true;
	public bool recScale = true;
	
	static string pathAnim = "Assets/PlayEditor/Animations/";
	
	static List<Transform> childs = new List<Transform>();
	static AnimationClip a;
	static GameObject thisG;
	static int index = 0;
	
	void Awake () 
	{
		thisG = this.gameObject;
		a = new AnimationClip();
		childs = new List<Transform>(GetComponentsInChildren<Transform>());
		
		if (!Directory.Exists (Application.dataPath + "/PlayEditor/Animations")) 
		{
			Directory.CreateDirectory (Application.dataPath + "/PlayEditor/Animations");
		}
		
		for (int i=1; i < childs.Count; i++)
		{
			childs[i].gameObject.AddComponent<KeyframerPE>();
		}
		
		Invoke("SetValuesStart",0.01f);
	}
	
	void SetValuesStart()
	{
		if (doRecDel != null) { doRecDel(true, false, keyTimeInterval, recPosition, recRotation, recScale); };
	}
	
	public void StopRecording ()
	{
		StartCoroutine(CombineCurves());
		Destroy(this, 1.5f);
	}
	
	static IEnumerator CombineCurves ()
	{
		int count = childs.Count;
		int i;
		string path;
		KeyframerPE s;
		
		for (i=1; i < count; i++)
		{
			s = childs[i].gameObject.GetComponentInChildren<KeyframerPE>();
			path = GetTPath(childs[i]);
	
			ApplyCurves (path, s.cPX, s.cPY, s.cPZ, s.cRX, s.cRY, s.cRZ, s.cRW, s.cSX, s.cSY, s.cSZ);
			
			if (i == count-1)
			{
				CreateAnim();
				yield return null;
			}
		}
	}
	
	static void ApplyCurves (string path, AnimationCurve pX, AnimationCurve pY, AnimationCurve pZ, 
		AnimationCurve rX, AnimationCurve rY, AnimationCurve rZ, AnimationCurve rW, 
		AnimationCurve sX, AnimationCurve sY, AnimationCurve sZ)
	{
		a.SetCurve (path, typeof(Transform), "localPosition.x", pX);
		a.SetCurve (path, typeof(Transform), "localPosition.y", pY);
		a.SetCurve (path, typeof(Transform), "localPosition.z", pZ);
		
		a.SetCurve (path, typeof(Transform), "localRotation.x", rX);
		a.SetCurve (path, typeof(Transform), "localRotation.y", rY);
		a.SetCurve (path, typeof(Transform), "localRotation.z", rZ);
		a.SetCurve (path, typeof(Transform), "localRotation.w", rW);
		
		a.SetCurve (path, typeof(Transform), "localScale.x", sX);
		a.SetCurve (path, typeof(Transform), "localScale.y", sY);
		a.SetCurve (path, typeof(Transform), "localScale.z", sZ);
	}
	
	
	
	static void CreateAnim ()
	{
		if (doRecDel != null) { doRecDel(false, true, 0, false, false, false); }
		a.wrapMode = WrapMode.ClampForever;
		string animName = thisG.transform.name+ "@Action";
		
		string newPath = pathAnim + animName;
		
		while (File.Exists(newPath+".anim")) //Create a unique filepath
		{
			index++;
			if (File.Exists(newPath+"_"+ index.ToString()+".anim"))
				continue;
			newPath += "_"+ index.ToString();
		}
		
		AssetDatabase.CreateAsset (a, newPath +".anim");
		
		Debug.Log("Animation clip saved : "+ newPath);
	}
	
	static string GetTPath(Transform t)
	{
		string path = "/" +t.name;
		
	    while (t.parent != null && t.parent != t.root)
	    {
			t = t.parent;
	        path = "/" +t.name+ path;
	    }
	    return path.Substring(1);
	}
}
#endif
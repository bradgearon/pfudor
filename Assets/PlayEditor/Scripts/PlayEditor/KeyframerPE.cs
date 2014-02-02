#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class KeyframerPE : MonoBehaviour 
{
	public AnimationCurve cPX;
	public AnimationCurve cPY;
	public AnimationCurve cPZ;
	
	public AnimationCurve cRX;
	public AnimationCurve cRY;
	public AnimationCurve cRZ;
	public AnimationCurve cRW;
	
	public AnimationCurve cSX;
	public AnimationCurve cSY;
	public AnimationCurve cSZ;
	
	float t = 0.0f;
	Transform tf;
	bool pos = false;
	bool rot = false;
	bool scale = false;
	
	void Awake() 
	{
		tf = transform;
		RecorderPE.doRecDel += DoRec;
		
		cPX = new AnimationCurve();
		cPY = new AnimationCurve();
		cPZ = new AnimationCurve();
		
		cRX = new AnimationCurve();
		cRY = new AnimationCurve();
		cRZ = new AnimationCurve();
		cRW = new AnimationCurve();
		
		cSX = new AnimationCurve();
		cSY = new AnimationCurve();
		cSZ = new AnimationCurve();
	}
	
	void DoRec (bool rec, bool del, float pulse, bool p, bool r, bool s)
	{
		if (rec)
		{
			if (t < 0.5f)
				t = -pulse; // Start at frame zero, independent of the pulse
			
			StartCoroutine (SetKeys (rec, pulse));
		}
		else if (del)
		{
			RecorderPE.doRecDel -= DoRec;
			Destroy(this);
		}
		pos = p;
		rot = r;
		scale = s;
	}
	
	IEnumerator SetKeys (bool rec, float sec)
	{
		while (rec)
		{
			if (pos)
			{
				Vector3 lP = tf.localPosition;
				cPX.AddKey (t, lP.x);
				cPY.AddKey (t, lP.y);
				cPZ.AddKey (t, lP.z);
			}
			if (rot)
			{
				Quaternion lR = tf.localRotation;
				cRX.AddKey (t, lR.x);
				cRY.AddKey (t, lR.y);
				cRZ.AddKey (t, lR.z);
				cRW.AddKey (t, lR.w);
			}
			if (scale)
			{
				Vector3 lS = tf.localScale;
				cSX.AddKey (t, lS.x);
				cSY.AddKey (t, lS.y);
				cSZ.AddKey (t, lS.z);
			}
			
			yield return new WaitForSeconds(sec);
			
			t += sec;
		}
	}
}
#endif
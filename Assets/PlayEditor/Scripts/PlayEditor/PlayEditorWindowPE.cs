#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
 
public class PlayEditorWindowPE : EditorWindow 
{
	public GameObject curBrush;
	public List<GameObject> BrushObjs = new List<GameObject>();
	
	bool useParent = true;
	bool useNewGroup = true;
	bool showList = false;
	bool showLoadOpts = false;
	bool randomBrush = false;
	
    int _index = 0;
	GameObject cam;
	GameObject master;
	 
	Vector3 cpPos;
	Quaternion cpRot;
	Vector3 cpScale;
	
	bool showSavedProps = false;
	bool savePos = true;
	bool saveRot = true;
	bool saveScale = true;
	
	Vector2 scrollP;
	
	static int number = 0;
	static string pathOBJ = "Assets/PlayEditor/Meshes/";
	
	bool showGrpOpt = false;
	bool uniqueNames = false;
	
	bool showMergeOpt = false;
	static bool asOBJ = true;
	static bool asMeshAsset = false;
	
	bool showAnimOpt = false;
	
	[MenuItem ("Window/Open PlayEditor")]
	static void Init () 
	{
		PlayEditorWindowPE windowPE = (PlayEditorWindowPE)EditorWindow.GetWindow (typeof (PlayEditorWindowPE));
		windowPE.Show();
		
		// Create save folder if not existing
		if (!Directory.Exists (Application.dataPath + "/PlayEditorSave")) 
		{
			Directory.CreateDirectory (Application.dataPath + "/PlayEditorSave");
		}
		if (!Directory.Exists (Application.dataPath + "/PlayEditor/Meshes")) 
		{
			Directory.CreateDirectory (Application.dataPath + "/PlayEditor/Meshes");
		}
	}
	//=================================================================================================================o
	public void RandomIndex ()
	{
		if (randomBrush) {
			_index = UnityEngine.Random.Range(0, BrushObjs.Count);
		}
	}
	//=================================================================================================================o
	static void MergeMeshes (Transform parent_)
	{
		if (parent_ != null)
		{
			GameObject master = new GameObject();
			master.transform.position = parent_.position;
			parent_.parent = master.transform;
		
			MeshFilter[] meshFilters = master.GetComponentsInChildren<MeshFilter>();
	        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
			
			foreach(Transform t in parent_) // integrate subgroups
			{
				while (t.childCount != 0)
					foreach(Transform c in t)
						c.parent = parent_;
			}
			
			Material baseMat = null;
	        int i = 0; 
			
	        while (i < meshFilters.Length) 
			{
				if (baseMat == null) // material the first child is using
					baseMat = meshFilters[i].transform.GetComponent<MeshRenderer>().sharedMaterial;
	            combine[i].mesh = meshFilters[i].sharedMesh;
	            combine[i].transform = Matrix4x4.TRS(meshFilters[i].transform.localPosition, meshFilters[i].transform.localRotation, meshFilters[i].transform.localScale);//meshFilters[i].transform.localToWorldMatrix;
	            meshFilters[i].gameObject.active = false; // Unity 3.5.x
				//meshFilters[i].gameObject.SetActive(false); // Unity 4
	            i++;
	        }
			
			MeshFilter mf = master.AddComponent<MeshFilter>();
			MeshRenderer mr = master.AddComponent<MeshRenderer>();
			mr.sharedMaterial = baseMat;
			mf.sharedMesh = new Mesh();
			mf.sharedMesh.CombineMeshes(combine);
			mf.sharedMesh.Optimize();
			int trisCount = mf.sharedMesh.triangles.Length/3;
			master.name = "NewMesh_tris_" +trisCount.ToString();
			master.active = true; // Unity 3.5.x
			//master.SetActive(true); // Unity 4
			string name_ = master.name;
			// destroy unneeded game objects in hierarchy
			for (int c=0; c < meshFilters.Length; c++)
			{
				if (meshFilters[c] != null)
				{
					if(c == 1) 
						name_ = meshFilters[c].gameObject.name +"_Tris"+ trisCount.ToString() +"_N";
					DestroyImmediate(meshFilters[c].gameObject);
				}
			}
			if (parent_ != null)
				DestroyImmediate(parent_.gameObject);
			
			Debug.Log(meshFilters.Length + " Merged");
			
			string newPathOBJ = pathOBJ + name_;
			
			while (File.Exists(newPathOBJ+".obj")) //Create a unique filepath
			{
				number++;
				if (File.Exists(newPathOBJ+"_"+ number.ToString()+".obj"))
					continue;
				newPathOBJ += "_"+ number.ToString();
			}
			
			if (asOBJ)
			{
				MeshToFile(mf,newPathOBJ+".obj");
				//AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				
				GameObject g = (GameObject)AssetDatabase.LoadAssetAtPath(newPathOBJ+".obj",typeof(GameObject));
				foreach(Transform t in g.transform) {
					if (t.parent == t.root) {
						t.renderer.sharedMaterial = baseMat;
					}
				}
				g = (GameObject)Instantiate(g, master.transform.position, Quaternion.identity);
				
				
				if (master != null)
					DestroyImmediate(master);
				
				Selection.activeGameObject = g;
			}
			else if (asMeshAsset)
			{
				Mesh m = mf.sharedMesh;
				AssetDatabase.CreateAsset(m, newPathOBJ+".asset");
				AssetDatabase.Refresh();
				GameObject prefab = PrefabUtility.CreatePrefab(newPathOBJ+".prefab", mf.gameObject, ReplacePrefabOptions.ReplaceNameBased);
				MeshFilter mfp = prefab.GetComponent<MeshFilter>();
				mfp.mesh = m;
				
				Selection.activeGameObject = master;
			}
		}
		else
		{
			Debug.Log("Select a group parent in the Herachy window");
		}
	}
	//=================================================================================================================o
	// MeshToString algorithm by Keli Hlodversson(Unity employee) http://wiki.unity3d.com/index.php/ObjExporter
	static string MeshToString(MeshFilter mf) 
	{
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.renderer.sharedMaterials;
 
        StringBuilder sb = new StringBuilder();
 
        sb.Append("g ").Append(mf.name).Append("\n");
        foreach(Vector3 v in m.vertices) {
            sb.Append(string.Format("v {0} {1} {2}\n",-v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector3 v in m.normals) {
            sb.Append(string.Format("vn {0} {1} {2}\n",-v.x,v.y,v.z));
        }
        sb.Append("\n");
        foreach(Vector2 v in m.uv) {
            sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
        }
        sb.Append("\n");
        foreach(Vector2 v in m.uv1) {
            sb.Append(string.Format("vt1 {0} {1}\n",v.x,v.y));
        }
        sb.Append("\n");
        foreach(Vector2 v in m.uv2) {
            sb.Append(string.Format("vt2 {0} {1}\n",v.x,v.y));
        }
        sb.Append("\n");
        foreach(Color c in m.colors) {
            sb.Append(string.Format("vc {0} {1} {2} {3}\n",c.r,c.g,c.b,c.a));
        }
        for (int material=0; material < m.subMeshCount; material ++) {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");
 
            int[] triangles = m.GetTriangles(material);
            for (int i=0;i<triangles.Length;i+=3) {
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", 
                    triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
            }
        }
        return sb.ToString();
    }
	//=================================================================================================================o
	static void MeshToFile(MeshFilter mf, string filename) 
	{
        try
		{
	        using (StreamWriter sw = new StreamWriter(filename)) 
	        {
	            sw.WriteLine(MeshToString(mf));
			}
		}
		catch (System.Exception){ }
    }
	//=================================================================================================================o
	void GroupSelected ()
	{
		List<GameObject> selected_ = new List<GameObject> (Selection.gameObjects);
		int count = selected_.Count;
		if (count > 0)
		{
			GameObject master = new GameObject();
			master.transform.position = selected_[0].transform.position;
			master.name = "_newGroup" +count;
			int i;
			string sName ="";
			int iNumber = 0;
			for (i=0; i<count; i++)
			{
				selected_[i].transform.parent = master.transform;
				
				
				if (uniqueNames)
				{
					sName = selected_[i].name;	
					while(selected_[i].name == sName)
					{
						iNumber++;
						if (selected_[i].name == sName + iNumber.ToString())
							continue;
						selected_[i].name += "_" + iNumber.ToString();
					}
				}
			}
			Selection.activeGameObject = master;
		}
		else Debug.Log("Nothing to group selected");
	}
	//=================================================================================================================o
	void CopyTransfProp ()
	{
		if (Selection.activeTransform == null)
			return;
		Transform curT = Selection.activeTransform;
		cpPos = curT.position;
		cpRot = curT.rotation;
		cpScale = curT.localScale;
		Debug.Log("Transform propertie(s) captured");
	}
	//=================================================================================================================o
	void PasteTransfProp ()
	{
		if (Selection.transforms == null)
			return;
		Transform[] ts = Selection.transforms;
		foreach (Transform t in ts)
		{
			if(savePos)
				t.position = cpPos;
			if(saveRot)
				t.rotation = cpRot;
			if(saveScale)
				t.localScale = cpScale;
		}
		Debug.Log("Transform propertie(s) assigned");
	}
	//=================================================================================================================o
	static void Legacy2Mecanim () // Only for generic rigs
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;

        if(clip)
        {
            SerializedObject soClip = new SerializedObject(clip);

            SerializedProperty propAnimType = soClip.FindProperty("m_AnimationType");
            SerializedProperty propStopTime = soClip.FindProperty("m_MuscleClipInfo.m_StopTime");

            if (propAnimType != null && propStopTime != null)
            {            
                propAnimType.intValue = 2;
                propStopTime.floatValue = clip.length;
                soClip.ApplyModifiedProperties();
            }
        }
    }
	//=================================================================================================================o
	static void Mecanim2Legacy ()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;

        if (clip)
        {
            SerializedObject soClip = new SerializedObject(clip);

            SerializedProperty propAnimType = soClip.FindProperty("m_AnimationType");

            if (propAnimType != null)
            {
                propAnimType.intValue = 1;
                soClip.ApplyModifiedProperties();
            }
        }
    }
	//=================================================================================================================o
	void OnInspectorUpdate ()
	{
		Repaint();
	}
	//=================================================================================================================o
	void OnGUI () 
	{	
		EditorGUILayout.BeginVertical();
		scrollP = EditorGUILayout.BeginScrollView(scrollP, false, true);
		
		
		GUILayout.Label ("- Brush Options :", EditorStyles.whiteBoldLabel);
		
		randomBrush = GUILayout.Toggle(randomBrush,"Random Brush");
		
	
		if (GUILayout.Button ("Cycle / Brushes"))
		{
			if ( _index < BrushObjs.Count-1)
				_index++;
			else
				_index = 0;
		}
		if (BrushObjs.Count > 0)
		{
			_index = Mathf.Clamp(_index,0,BrushObjs.Count-1);
			EditorGUILayout.ObjectField(BrushObjs[_index], typeof(GameObject),true);
			curBrush = BrushObjs[_index];
		}
		
		showList = EditorGUILayout.Foldout(showList, " Brush collection");
		if (showList)
		{
			if (GUILayout.Button ("+Add+"))
			{
				BrushObjs.Insert(BrushObjs.Count, (GameObject)Selection.activeObject);
				_index = BrushObjs.Count-1;
			}
			if (GUILayout.Button ("-Remove-"))
			{
				BrushObjs.Remove((GameObject)Selection.activeObject);
				_index = BrushObjs.Count-1;
			}
			if (GUILayout.Button ("Clear"))
			{
				BrushObjs.Clear();
				_index = 0;
			}
			for (int i=0; i<BrushObjs.Count; i++)
			{
				if (BrushObjs[i] == null)
				{
					BrushObjs.Remove(BrushObjs[i]);
				}
				EditorGUILayout.ObjectField(BrushObjs[i], typeof(GameObject),true);
			}
		}
		
		if (!EditorApplication.isPlaying)
		{
			GUILayout.Label ("- Load Options :", EditorStyles.whiteBoldLabel);
			showLoadOpts = EditorGUILayout.Foldout(showLoadOpts, " Show options");
			if (showLoadOpts)
			{
				useParent = GUILayout.Toggle (useParent,"Load as group");
				if (useParent)
					useNewGroup = GUILayout.Toggle (useNewGroup,"New group");
			}
			if (GUILayout.Button ("Load"))
			{
				int i;
				int k;
				List <GameObject> keys = new List<GameObject> (BrushObjs);
				ContainerPE c = ContainerPE.Load(Application.dataPath + "/PlayEditorSave/LastSave.xml");
				int iCount = c.Items.Count;
				int kCount = keys.Count;
				GameObject g;
				
				for (i=0; i < iCount; i++)
				{
					for (k=0; k < kCount; k++)
					{
						if (keys[k].name == c.Items[i].Name)
						{
							g = (GameObject)Instantiate(keys[k], new Vector3(c.Items[i].pX, c.Items[i].pY, c.Items[i].pZ), Quaternion.identity);
							g.name = c.Items[i].Name;
							g.transform.rotation = new Quaternion(c.Items[i].rX, c.Items[i].rY, c.Items[i].rZ, c.Items[i].rW);
							g.transform.localScale = new Vector3(c.Items[i].sX, c.Items[i].sY, c.Items[i].sZ);
							if (useParent)
							{
								if (master == null || (useNewGroup && i == 0))
								{
									master = new GameObject ();
									master.transform.position = g.transform.position;
									master.name = "_Group "+ DateTime.Now;
								}
								g.transform.parent = master.transform;
							}
						}
					}
				}
				Selection.activeGameObject = master;
			}
			
			GUILayout.Label ("- Merge Selected :", EditorStyles.whiteBoldLabel);
			showMergeOpt = EditorGUILayout.Foldout(showMergeOpt, " Show options");
			if(showMergeOpt)
			{
				asOBJ = GUILayout.Toggle (asOBJ,"Create .obj file");
				if(asOBJ) asMeshAsset = false;
				
				asMeshAsset = GUILayout.Toggle (asMeshAsset,"Create .asset file");
				if(asMeshAsset) asOBJ = false;
			}
			if(GUILayout.Button ("Merge Meshes"))
			{
				MergeMeshes(Selection.activeTransform);
			}
			
			GUILayout.Label ("- Group Options :", EditorStyles.whiteBoldLabel);
			showGrpOpt = EditorGUILayout.Foldout(showGrpOpt, " Show options");
			if(showGrpOpt)
			{
				uniqueNames = GUILayout.Toggle (uniqueNames,"Create unique names");
			}
			if(GUILayout.Button ("Group Selected")) 
			{
				GroupSelected();
			}
			
			GUILayout.Label ("- Property Options :", EditorStyles.whiteBoldLabel);
			showSavedProps = EditorGUILayout.Foldout(showSavedProps, " Show options");
			if (showSavedProps)
			{
				savePos = GUILayout.Toggle (savePos,"Position");
				saveRot = GUILayout.Toggle (saveRot,"Rotation");
				saveScale = GUILayout.Toggle (saveScale,"Scale");
			}
			if(GUILayout.Button ("Copy Transf. Props.")) 
			{
				CopyTransfProp();
			}
			else if(GUILayout.Button ("Paste Transf. Props.")) 
			{
				PasteTransfProp();
			}
			
			GUILayout.Label ("- Animation Options :", EditorStyles.whiteBoldLabel);
			showAnimOpt = EditorGUILayout.Foldout(showAnimOpt, " .anim file options");
			if(showAnimOpt)
			{
				if(GUILayout.Button ("Legacy to Mecanim")) // Only for Generic rigs
				{
					Legacy2Mecanim();
				}
				else if(GUILayout.Button ("Mecanim to Legacy")) 
				{
					Mecanim2Legacy();
				}
			}
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}
}
#endif
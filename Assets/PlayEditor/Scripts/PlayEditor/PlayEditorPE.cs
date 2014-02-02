#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[RequireComponent(typeof(KeyMappingPE))]
public class PlayEditorPE : MonoBehaviour
{
	public float cameraRotateSpeed = 5.0f;
	public float cameraMoveSpeed = 5.0f;
	
	public enum EditMode{
		Brush,
		Select,
		Move,
		Rotate,
		Scale,
		RandomRotation,
		RandomScale
	}
	public enum AxisMode{
		Default,
		OnlyX,
		OnlyY,
		OnlyZ
	}
	public enum SelectMode{
		Single,
		Multi,
		Tagged
	}
	public enum SpaceMode{
		Local,
		World
	}
	public enum BrushMode{
		Default,
		AlignToSurf,
		RandomRot,
		PhysX
	}
	
	[HideInInspector]
	public BrushMode brushMode = BrushMode.Default;
	[HideInInspector]
	public EditMode editMode = EditMode.Move;
	[HideInInspector]
	public AxisMode axisMode = AxisMode.Default;
	[HideInInspector]
	public SelectMode selectMode = SelectMode.Single;
	[HideInInspector]
	public SpaceMode spaceMode = SpaceMode.World;
	
	
	public GameObject selectorPref = null;
	public GameObject cursorPref = null;
	
	public static List<GameObject> SelectedObjs;
	public Dictionary<GameObject, int> EditedObjs;
	public static List<GameObject> GrabObjs;
	public static List<GameObject> AnimObjs;
	
	public float physXSpawnOffset = 22.0f;
	public float brushOffsetRange = 30.0f;
	
	public bool showGizmoLines = true;
	
	public string selectorName = "selector";
	
	public float maxBrushDist = 10000.0f;
	
	public bool lockAnimTaggedObjs = true; // Lock AnimTag root items, to make use of GrabTag and move childs
	public string grabableTag = "GrabTag";
	public string animatableTag = "AnimTag";
	
	public bool rdmScaleRotToChilds = false;
	
	public float rotateSpeed = 300f;
	public float scaleSpeed = 2f;
	
	float dist;
	GameObject curObj;
	GameObject master;
	Transform cam;
	ContainerPE cont;
	PlayEditorWindowPE pew;
	KeyMappingPE kM;
	
	float mX = 0.0f;
	float mY = 0.0f;
	Vector3 camClickVec;
	Vector2 startPos;
	Vector2 curPos;
	
	
	Vector3 cachePos;
	Vector3 cacheScale;
	Quaternion cacheRot;

	
	float minScale = 0.2f;
	float maxScale = 10.0f;
	float brushScale = 1.0f;
	float singleSelectionSize = 60.0f;
	float delAfterSec = 10.0f;
	float physBrushForce = 50.0f;
	
	
	int curAxis = 0;
	int curMode = 0;
	int curSelectM = 0;
	int curSpaceM = 0;
	int curBrushM = 0;
	string[] axisGrid = {"All","X","Y","Z"};
	string[] selectModeGrid = {"Single","Multi","Tagged"};
	string[] spaceModeGrid = {"Local","World"};
	string[] toolGridS = {"Brush","Select","Move","Rotate","Scale"};
	string[] brushModeGrid = {"Standard","Align To Surf","Random Rot.","PhysX"};
	Rect _SingleSelectSliderRect,_SelectionBox,_LeftPanel,_TopPanel,_Save,_Load,_BrushModeRect,_ModeRect,_SelModeRect,
	_AxisModeRect,_SpaceModeRect,_RdomRot,_RdomScale,_MinScale,_MaxScale,_BrushScale,_SnapRect,_VisibleRect,_MinScaleLabel,
	_MaxScaleLabel,_singleSelectLabel,_brushScaleLabel,_PhysXDrop,_DeleteSelect,_delColRect,_delRigRect,_remTimeRect,_ForceRect,
	_remTimeLabel,_ForceLabel,_ShowSeleRect,_SeleAllRect,_DeloImpRect,_RdmScaleBrushRect,_EditCounterRect,
	_RecAnimFoldTog,_RecAnimRect,_RecTimeLabel,_RecKeyTimer,_RecPosTog,_RecRotTog,_RecScaleTog,
	_ContBrushTog,_ContBrushSlider,_ContBSLabel,_OffSetBrushSlider,_OffsetBSLabel;
	
	static bool showSelected = false;
	bool dragging = false;
	bool isReset = false;
	bool isGUIVisible = true;
	bool useRigDelOnImp = false;
	bool isSnapToSurf = true;
	bool removeAddedRigidBody = true;
	bool removeAddedCollider = true;
	bool useRdmScaleForBrush = false;
	bool isMultiAction = false;
	bool isSaving = false;
	bool isLoading = false;
	
	static bool doRecAnimFold = false;
	static float recAnimTimePulse = 0.5f;
	static bool doRecPos = true;
	static bool doRecRot = true;
	static bool doRecScale = true;
	static bool isRecording = false;
	static bool setupRec = false;
	static RecorderPE recPE;
	public GameObject animObj = null;
	
	float brushOffset = 0.0f;
	
	float brushSpacing = 0.3f;
	bool continuousBrush = true;
	bool isBSpace = true;
	bool isCBrushing = false;
	
	public bool loadLastSettings = true;
	
	//=================================================================================================================o
	void Start ()
	{
		cam = transform;
		EditedObjs = new Dictionary<GameObject, int>();
		SelectedObjs = new List<GameObject>();
		GrabObjs = new List<GameObject>();
		AnimObjs = new List<GameObject>();
		
		try 
		{
			GameObject[] tObjs = GameObject.FindGameObjectsWithTag(grabableTag);
			GameObject[] aObjs = GameObject.FindGameObjectsWithTag(animatableTag);
			
			
			for (int i=0; i < tObjs.Length; i++)
			{
				GrabObjs.Add(tObjs[i]);
			}
			for (int i=0; i < aObjs.Length; i++)
			{
				AnimObjs.Add(aObjs[i]);
			}
		}
		catch (System.Exception){ 
			Debug.Log("- Missing Tag(s) ! - Please make sure that the Tag Manager contains AnimTag & GrabTag"); 
		}
		
		
		if (pew == null) {
			pew = UnityEditor.EditorWindow.GetWindow(typeof(PlayEditorWindowPE)) as PlayEditorWindowPE;
		}
		if (cont == null) {
			cont = new ContainerPE();
		}
		if (kM == null) {
			kM = GetComponent<KeyMappingPE>() as KeyMappingPE;
		}
		
		if (selectorPref == null || cursorPref == null)
			Debug.Log ("Please assign a Selector and Cursor prefab to the PlayEditor found in Inspector");
		
		// GUI setup
		_Save = new Rect (13, 7, 50, 20);
		_Load = new Rect (64, 7, 50, 20);
		_LeftPanel = new Rect (0, 0, 130, Screen.height);
		_TopPanel = new Rect (131, 0, Screen.width-132, 35);
		_ModeRect = new Rect(15,60,100,140);
		_BrushModeRect = new Rect (Screen.width/3.0f,5,Screen.width/4,25);
		_SelModeRect = new Rect (Screen.width/2.5f,5,250,25);
		_AxisModeRect = new Rect (Screen.width/2.5f,5,330,25);
		_SpaceModeRect = new Rect (Screen.width/1.5f,5,250,25);
		_RdomRot = new Rect (15,230,100,20);
		_RdomScale = new Rect (15,255,100,20);
		_MinScale = new Rect (15,305,100,20);
		_MaxScale = new Rect (15,330,100,20);
		_BrushScale = new Rect(Screen.width/7.0f,20,Screen.width/14,20);
		_brushScaleLabel = new Rect(Screen.width/7.0f,5,Screen.width/10,30);
		_MinScaleLabel = new Rect(15,290,120,20);
		_MaxScaleLabel = new Rect(15,315,120,20);
		_singleSelectLabel = new Rect(Screen.width/7.0f,5,Screen.width/13,20);
		_SnapRect = new Rect (Screen.width/7.0f,5,100,30);
		_SingleSelectSliderRect = new Rect (Screen.width/7.0f,20,Screen.width/15,20);
		_VisibleRect = new Rect(15,Screen.height-25,90,20);
		_PhysXDrop = new Rect(Screen.width/1.3f,5,Screen.width/13,25);
		_DeleteSelect = new Rect(15,380,100,20);
		_delColRect = new Rect(Screen.width/1.45f,3,Screen.width/15,15);
		_delRigRect = new Rect(Screen.width/1.45f,16,Screen.width/15,15);
		_remTimeRect = new Rect(Screen.width/1.17f,4,Screen.width/15,10);
		_ForceRect = new Rect(Screen.width/1.17f,18,Screen.width/15,10);
		_remTimeLabel = new Rect(Screen.width/1.08f,3,Screen.width/15,20);
		_ForceLabel = new Rect(Screen.width/1.08f,16,Screen.width/15,20);
		_ShowSeleRect = new Rect(17,Screen.height-60,90,20);
		_SeleAllRect = new Rect(Screen.width/4.0f,5,Screen.width/16,25);
		_DeloImpRect = new Rect(Screen.width/4.0f,7,100,30);
		_RdmScaleBrushRect = new Rect(15,350,100,20);
		_EditCounterRect = new Rect(20,410,90,20);
		_RecAnimFoldTog = new Rect(15,440,100,20);
		_RecAnimRect = new Rect(20,470,90,20);
		_RecTimeLabel = new Rect(20,500,90,20);
		_RecKeyTimer = new Rect(20,520,90,20);
		_RecPosTog = new Rect(20,540,25,20);
		_RecRotTog = new Rect(50,540,25,20);
		_RecScaleTog = new Rect(80,540,25,20);
		_ContBrushTog = new Rect(Screen.width/1.68f,3,Screen.width/15,15);
		_ContBrushSlider = new Rect(Screen.width/1.68f,21,Screen.width/18,15);
		_ContBSLabel = new Rect(Screen.width/1.53f,18,Screen.width/18,15);
		_OffSetBrushSlider = new Rect(15,215,100,15);
		_OffsetBSLabel = new Rect(15,200,120,20);
		
		
		// Visible keymapping for the GUI
		toolGridS[0] += "     ("+kM.brushMode.ToString()+")";
		toolGridS[1] += "     ("+kM.selectMode.ToString()+")";
		toolGridS[2] += "     ("+kM.moveMode.ToString()+")";
		toolGridS[3] += "     ("+kM.rotateMode.ToString()+")";
		toolGridS[4] += "     ("+kM.scaleMode.ToString()+")";
		
		selectModeGrid[0] += "   ("+kM.selectModeToggle.ToString()+")";
		spaceModeGrid[0] += "   ("+kM.spaceModeToggle.ToString()+")";
		
		axisGrid[0] += " ("+kM.allAxis.ToString()+")";
		axisGrid[1] += " ("+kM.xAxis.ToString()+")";
		axisGrid[2] += " ("+kM.yAxis.ToString()+")";
		axisGrid[3] += " ("+kM.zAxis.ToString()+")";
		
		// Load last PlayEditor settings
		if(loadLastSettings)
			StartCoroutine(_LoadSettings());
		
		// Selector render camera 
		StartCoroutine(_SelectorCam());
		
		// Create selector pool
		Pool.CreatePoolWith(selectorPref, 200, selectorName);
	}
	//=================================================================================================================o
	
	IEnumerator _SelectorCam ()
	{
		yield return new WaitForSeconds(0.5f);
		
		GameObject selCam = new GameObject();
		Camera c = selCam.AddComponent<Camera>();
		
		selCam.name = "Selector Camera";
		selCam.transform.position = cam.position;
		selCam.transform.rotation = cam.rotation;
		selCam.transform.parent = cam;
		
		c.clearFlags = CameraClearFlags.Depth;
		c.farClipPlane = 2000f;
		c.depth = 0;
		c.renderingPath = RenderingPath.VertexLit;
		c.fieldOfView = cam.camera.fieldOfView;
		c.cullingMask = 1 << 1; // Only TransparentFX
		
		selectorPref.layer = 1;
		cursorPref.layer = 1;
		
		cam.camera.cullingMask &= ~(1 << 1); // Not TransparentFX
	}
	//=================================================================================================================o
	void Update ()
	{
		FreeCamera ();
		// if not controlling options due GUI (click secure area)
		if (!inGuiSpace(_LeftPanel,_TopPanel))
		{
			_Input ();
			
			switch (editMode) 
			{
			case EditMode.Brush:
				_BrushObj ();
				break;
				
			case EditMode.Select:
				_SelectObj ();
				break;
				
			case EditMode.Move:
				_MoveObj ();
				break;
				
			case EditMode.Rotate:
				_RotateObj ();
				break;
				
			case EditMode.Scale:
				_ScaleObj ();
				break;
			}
		}
	}
	//=================================================================================================================o
	void _Input ()
	{
		if (!Input.GetMouseButton(0)) // Do not switch Editmode during action
		{
			if (Input.GetKeyDown (kM.brushMode))
			{
				curMode = (int)EditMode.Brush;
			}
			else if (Input.GetKeyDown (kM.selectMode))
			{
				curMode = (int)EditMode.Select;
			}
			else if (Input.GetKeyDown (kM.moveMode))
			{
				curMode = (int)EditMode.Move;
			}
			else if (Input.GetKeyDown (kM.rotateMode))
			{
				curMode = (int)EditMode.Rotate;
			}
			else if (Input.GetKeyDown (kM.scaleMode))
			{
				curMode = (int)EditMode.Scale;
			}
			else if (Input.GetKeyDown (kM.recordToggle))
			{
				if (doRecAnimFold)
				{
					if (!setupRec)
					{
						StartCoroutine(_RecAnimation(animObj, animatableTag, isRecording));
					}
				}
				doRecAnimFold = true;
			}
		}
		// Top panel/secondary options
		if (curMode == (int)EditMode.Move || curMode == (int)EditMode.Rotate || curMode == (int)EditMode.Scale)
		{
			if (Input.GetKeyDown (kM.xAxis))
			{
				curAxis = (int)AxisMode.OnlyX;
			}
			else if (Input.GetKeyDown (kM.yAxis))
			{
				curAxis = (int)AxisMode.OnlyY;
			}
			else if (Input.GetKeyDown (kM.zAxis))
			{
				curAxis = (int)AxisMode.OnlyZ;
			}
			else if (Input.GetKeyDown (kM.allAxis))
			{
				curAxis = (int)AxisMode.Default;
			}
			// Move and Rotate mode
			if (curMode != (int)EditMode.Scale) 
			{
				if(Input.GetKeyDown (kM.spaceModeToggle))
				{
					if (curSpaceM == (int)SpaceMode.World)
						curSpaceM = (int)SpaceMode.Local;
					else
						curSpaceM = (int)SpaceMode.World;;
				}
			}
		}
		else if (curMode == (int)EditMode.Select)
		{
			if(Input.GetKeyDown (kM.selectModeToggle))
			{
				if (curSelectM == (int)SelectMode.Multi)
					curSelectM = (int)SelectMode.Single;
				else
					curSelectM = (int)SelectMode.Multi;
			}
		}
	}
	//=================================================================================================================o
	void FreeCamera ()
	{
		float v = Input.GetAxis ("Vertical");
		float h = Input.GetAxis ("Horizontal");
		float mW = Input.GetAxis ("Mouse ScrollWheel");
		
		// Right mouse button --> rotation
		if (Input.GetMouseButton (kM.mouseCamRot)) {
			mX += Input.GetAxis ("Mouse X") * cameraRotateSpeed;
			mY += Input.GetAxis ("Mouse Y") * cameraRotateSpeed;
			
			cam.localRotation = Quaternion.AngleAxis (mX, Vector3.up);
			cam.localRotation *= Quaternion.AngleAxis (mY, Vector3.left);
			Vector3 forwardVec = v == 0.0f ? cam.forward * mW * Time.deltaTime * 600 
				: cam.forward * cameraMoveSpeed * v;
			cam.position += forwardVec;
			cam.position += cam.right * cameraMoveSpeed * h;
		}
		else if (editMode != EditMode.Select) // No camera drag-move while de/selecting
		{
			// Middle Mouse camera drag-move 
			if (Input.GetMouseButtonDown(kM.mouseCamDrag))
			{
				camClickVec = Input.mousePosition;
			}
			else if (Input.GetMouseButton(kM.mouseCamDrag))
			{
				Vector3 dPos = cam.camera.ScreenToViewportPoint(Input.mousePosition - camClickVec);
				Vector3 moveVec = new Vector3(dPos.x, dPos.y, 0);
				cam.Translate(-moveVec * 250 * Time.deltaTime, Space.Self);
			}
		}
		else // Standard movement
		{
			Vector3 forwardVec = v == 0.0f ? cam.forward * mW * Time.deltaTime * 600 
				: cam.forward * cameraMoveSpeed * v;
			cam.position += forwardVec;
			cam.position += cam.right * cameraMoveSpeed * h;
		}
	}
	//=================================================================================================================o
	IEnumerator _BrushInterval (float time)
	{
		isBSpace = false;
		isCBrushing = false;
		yield return new WaitForSeconds (time);
		isCBrushing = Input.GetMouseButton (kM.mouseDownBrush);
		yield return null;
		isBSpace = true;
	}
	//=================================================================================================================o
	void _BrushObj ()
	{
		if (Input.GetMouseButton (kM.mouseDownBrush) && continuousBrush) 
		{
			if(isBSpace)
				StartCoroutine(_BrushInterval(brushSpacing));
		}
		else isCBrushing = false;
		
		if (Input.GetMouseButtonDown (kM.mouseDownBrush) || isCBrushing) 
		{
			if (pew.BrushObjs.Count > 0)
			{
				pew.RandomIndex();
				GameObject curGo = pew.curBrush;
				Ray r = cam.camera.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (r, out hit, maxBrushDist)) 
				{
					Vector3 goPos = hit.point + hit.normal * brushOffset;
					
					if (brushMode == BrushMode.Default)
					{
						GameObject gO = (GameObject)Instantiate (curGo, goPos, Quaternion.identity);
						gO.name = curGo.name;
						if (!useRdmScaleForBrush)
						{
							gO.transform.localScale = new Vector3(brushScale, brushScale, brushScale);
						}
						else 
						{
							float rScale = Random.Range(minScale, maxScale);
							gO.transform.localScale = new Vector3(rScale, rScale, rScale);
						}
						
						SelectObj(gO, selectorPref, selectorName, lockAnimTaggedObjs);
						EditedObjs.Add(gO, gO.GetInstanceID());
						curObj = gO;
					}
					else if (brushMode == BrushMode.AlignToSurf) // Y is normal-up axis
					{
						GameObject gO = (GameObject)Instantiate (curGo, goPos, Quaternion.identity);
						gO.name = curGo.name;
						gO.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
						if (!useRdmScaleForBrush)
						{
							gO.transform.localScale = new Vector3(brushScale, brushScale, brushScale);
						}
						else 
						{
							float rScale = Random.Range(minScale, maxScale);
							gO.transform.localScale = new Vector3(rScale, rScale, rScale);
						}
						SelectObj(gO, selectorPref,selectorName, lockAnimTaggedObjs);
						EditedObjs.Add(gO, gO.GetInstanceID());
						curObj = gO;
					}
					else if (brushMode == BrushMode.RandomRot)
					{
						GameObject gO = (GameObject)Instantiate (curGo, goPos, Random.rotation);
						gO.name = curGo.name;
						if (!useRdmScaleForBrush)
						{
							gO.transform.localScale = new Vector3(brushScale, brushScale, brushScale);
						}
						else 
						{
							float rScale = Random.Range(minScale, maxScale);
							gO.transform.localScale = new Vector3(rScale, rScale, rScale);
						}
						SelectObj(gO, selectorPref,selectorName, lockAnimTaggedObjs);
						EditedObjs.Add(gO, gO.GetInstanceID());
						curObj = gO;
					}
					// BrushMode PhysiX is in Fixed Update
					else if (brushMode == BrushMode.PhysX)
					{
						Vector3 spawnP = cam.position + cam.forward * physXSpawnOffset;
						GameObject go = (GameObject)Instantiate (curGo, spawnP, Quaternion.identity);
						go.name = curGo.name;
						if (!useRdmScaleForBrush)
						{
							go.transform.localScale = new Vector3(brushScale, brushScale, brushScale);
						}
						else 
						{
							float rScale = Random.Range(minScale, maxScale);
							go.transform.localScale = new Vector3(rScale, rScale, rScale);
						}
						SelectObj(go, selectorPref,selectorName, lockAnimTaggedObjs);
						EditedObjs.Add(go, go.GetInstanceID());
						if (go.collider == null)
						{
							go.AddComponent<BoxCollider>();
						}
						else if (go.GetComponent<MeshCollider>())
						{
							go.GetComponent<MeshCollider>().convex = true;
						}
						if (go.rigidbody == null)
						{
							go.AddComponent<Rigidbody>();
						}
						if (go.rigidbody && go.collider)
						{		
							Vector3 pos = cam.position;
							float x = Input.mousePosition.x;
							float y = Input.mousePosition.y;
							Vector3 dir = cam.camera.ScreenToWorldPoint(new Vector3(x,y,100)) - pos;
							go.rigidbody.velocity = dir.normalized * physBrushForce;
							if (removeAddedRigidBody)
							{
								StartCoroutine(DelRigidbody(go.rigidbody, delAfterSec));
							}
							if (removeAddedCollider && !go.GetComponent<MeshCollider>())
							{
								StartCoroutine(DelCollider(go.collider, delAfterSec));
							}
							else if (go.GetComponent<MeshCollider>())
							{
								StartCoroutine(ResetMeshCol(go, delAfterSec));
							}
							if (useRigDelOnImp)
							{
								AddCollisionSensor(go);
							}
						}
					}
				}
			}
			else
			{
				Debug.Log("No objects assigned. (Add+) a prefab to the Brushable object list.");
			}
		}
		
		// Delete Selected Brush -> right mouse button + shift
		else if (Input.GetMouseButtonDown (kM.mouseDownBrushDel) && Input.GetKey(kM.delCombo))
		{
			Vector3 mP = Input.mousePosition;
			curPos = new Vector2 (mP.x, -mP.y);
			_SelectionBox = new Rect (mP.x-25, Screen.height - mP.y-25, 50, 50);
			curObj = null;
		
			int i;
			List<GameObject> sels = new List<GameObject>(SelectedObjs);
			int selCount = sels.Count;

			for (i=0; i < selCount; i++)
			{
				if (_SelectionBox.Contains (WorldToScreen( sels[i]))) 
				{
					curObj = sels[i];

					EditedObjs.Remove(sels[i]);
					DeselectObj(sels[i], selectorName, lockAnimTaggedObjs);
					break;
				}
				// Delete rigidbody(s) Brush PhysX -> right mouse button + shift
				if (brushMode == BrushMode.PhysX)
				{
					if (sels[i].rigidbody != null)
					{
						StartCoroutine(DelRigidbody(sels[i].rigidbody, 0));
					}
				}
			}
			if (curObj != null)
			{
				Destroy(curObj);
			}
		}
		
		
	}
	//=================================================================================================================o
	// Select Gameobjects, even without collider
	void _SelectObj ()
	{
		Vector3 mP = Input.mousePosition;
		int sCount = SelectedObjs.Count;
		int eCount = EditedObjs.Count;
		List<GameObject> eObjs = new List<GameObject> (EditedObjs.Keys);
		
		if (selectMode == SelectMode.Single) // Single target selection
		{
			if (Input.GetMouseButtonDown (kM.mouseSelect))
			{
				curObj = null;
				DeselectObj(animObj, selectorName, lockAnimTaggedObjs);
				animObj = null;
				curPos = new Vector2 (mP.x, -mP.y);
				_SelectionBox = new Rect (mP.x-singleSelectionSize/2, Screen.height - mP.y-singleSelectionSize/2, singleSelectionSize, singleSelectionSize);
	
				if (sCount > 0) // Complete new selection
				{
					int i;
					for (i=0; i < eCount; i++)
					{
						DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
					}
				}
			}
			else if (Input.GetMouseButtonUp (kM.mouseSelect))
			{
				int i;
				for (i=0; i < eCount; i++)
				{
					if (_SelectionBox.Contains (WorldToScreen(eObjs[i]))) 
					{
						if (sCount == 0)
						{
							curObj = eObjs[i];
							SelectObj (eObjs[i], selectorPref, selectorName, lockAnimTaggedObjs);
							break;
						}
					}
					else  // Nothing in the box..
						DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
				}
			}
		}
		else if (selectMode == SelectMode.Multi) // Box Multi-Selection mode
		{
			// Select left mouse button
			if (Input.GetMouseButtonDown (kM.mouseSelect)) {
				DeselectObj(animObj, selectorName, lockAnimTaggedObjs);
				animObj = null;
				_SelectionBox = new Rect ();
				startPos = new Vector2 (mP.x, mP.y);
				curAxis = (int)AxisMode.Default; // Reset last Axis setting
				// Add to selection -> hold left shift while dragging 
				if (!Input.GetKey(kM.addToSelectionCombo))
				{
					if (sCount > 0) // Complete new selection
					{
						int i;
						for (i=0; i < eCount; i++)
						{
							DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
						}
					}
				}
			} 
			else if (Input.GetMouseButton (kM.mouseSelect)) {
				curPos = new Vector2 (mP.x, mP.y);
				_SelectionBox = new Rect (Mathf.Min (startPos.x, curPos.x), Screen.height - Mathf.Max (startPos.y, curPos.y), 
					Mathf.Abs (startPos.x - curPos.x), Mathf.Abs (startPos.y - curPos.y));
			}
			else if (Input.GetMouseButtonUp (kM.mouseSelect)) 
			{
				int i;
				for (i=0; i < eCount; i++)
				{
					if (_SelectionBox.Contains (WorldToScreen(eObjs[i]))) 
					{
						SelectObj (eObjs[i], selectorPref, selectorName, lockAnimTaggedObjs);
					}
					else if (!Input.GetKey(kM.addToSelectionCombo)) // Nothing in the box or no left shift
						DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
					
				}
			}
			// Deselect middle mouse button
			else if (Input.GetMouseButtonDown(kM.mouseDeselect))
			{
				_SelectionBox = new Rect ();
				startPos = new Vector2 (mP.x, mP.y);
			}
			else if (Input.GetMouseButton(kM.mouseDeselect))
			{
				curPos = new Vector2 (mP.x, mP.y);
				_SelectionBox = new Rect (Mathf.Min (startPos.x, curPos.x), Screen.height - Mathf.Max (startPos.y, curPos.y), 
					Mathf.Abs (startPos.x - curPos.x), Mathf.Abs (startPos.y - curPos.y));
			}
			else if (Input.GetMouseButtonUp(kM.mouseDeselect))
			{
				int i;
				for (i=0; i < eCount; i++)
				{
					if (_SelectionBox.Contains (WorldToScreen(eObjs[i])))
					{
						DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
					}
				}
			}
		}
		else if (selectMode == SelectMode.Tagged) // Select tagged objects (for animation)
		{
			int gCount = GrabObjs.Count;
			int aCount = AnimObjs.Count;
			
			if (Input.GetMouseButtonDown (kM.mouseSelect))
			{
				curObj = null;
				if (!isRecording)
				{
					DeselectObj(animObj, selectorName, lockAnimTaggedObjs);
					animObj = null;
				}
				curPos = new Vector2 (mP.x, -mP.y);
				_SelectionBox = new Rect (mP.x-singleSelectionSize/2, Screen.height - mP.y-singleSelectionSize/2, singleSelectionSize, singleSelectionSize);
	
				if (sCount > 0) // Complete new selection
				{
					int i;
					for (i=0; i < eCount; i++)
					{
						DeselectObj(eObjs[i], selectorName, lockAnimTaggedObjs);
					}
				}
			}
			else if (Input.GetMouseButtonUp (kM.mouseSelect))
			{
				if (aCount > 0) // Select animTag object
				{
					int i;
					for (i=0; i < aCount; i++)
					{
						if (_SelectionBox.Contains (WorldToScreen(AnimObjs[i])))
						{
							if (sCount == 0)
							{
								animObj = AnimObjs[i];
								Debug.Log("Animatable object selected");
								SelectObj (animObj, selectorPref, selectorName, lockAnimTaggedObjs);
								break;
							}
						}
						else  // Nothing in the box..
						{
							if (!isRecording)
							{
								DeselectObj(AnimObjs[i], selectorName, lockAnimTaggedObjs);
							}
						}
					}
				}
				if (gCount > 0) // Select grabTag object
				{
					int i;
					for (i=0; i < gCount; i++)
					{
						if (_SelectionBox.Contains (WorldToScreen(GrabObjs[i]))) 
						{
							if (sCount == 0)
							{
								curObj = GrabObjs[i];
								Debug.Log("Grabable object selected");
								SelectObj (GrabObjs[i], selectorPref, selectorName, lockAnimTaggedObjs);
								break;
							}
						}
						else  // Nothing in the box..
						{
							DeselectObj(GrabObjs[i], selectorName, lockAnimTaggedObjs);
						}
					}
				}
			}
		}
	}
	//=================================================================================================================o
	void _MoveObj ()
	{
		int count = SelectedObjs.Count;
		
		if (Input.GetMouseButtonDown (kM.mouseMove))
		{
			if (count > 1) // Multi selection move
			{
				isMultiAction = true;
				curObj = SelectedObjs[0];
				dist = Vector3.Distance(cam.position, curObj.transform.position);
				Vector3 mP = Input.mousePosition;
				Ray r = cam.camera.ScreenPointToRay(mP);
				Vector3 movePos = r.GetPoint(dist);
				if (master == null)
					master = (GameObject)Instantiate(cursorPref, movePos, Quaternion.identity);
				else 
					master.transform.position = movePos;
				master.name = "_master";
				master.layer = cursorPref.layer;
				cachePos = master.transform.position;
				isReset = false;
				
				int i;
				for (i=0; i < count; i++)
				{
					SelectedObjs[i].transform.parent = master.transform;
				}
			}
			else if (count == 1) // Single selection move
			{
				isMultiAction = false;
				curObj = SelectedObjs[0];
				cachePos = curObj.transform.position;
				isReset = false;
				dist = Vector3.Distance(cam.position, curObj.transform.position);
			}
			else 
			{
				Debug.Log ("No Object(s) selected");
			}
		}
		else if (Input.GetMouseButton(kM.mouseMove) && !isReset) 
		{
			Vector3 mP = Input.mousePosition;
			Ray r = cam.camera.ScreenPointToRay(mP);
			Vector3 movePos = r.GetPoint(dist);
			
			
			if (isMultiAction)
			{
				if (axisMode == AxisMode.OnlyX)
				{
					movePos = new Vector3(movePos.x, master.transform.position.y, master.transform.position.z);
					GizmoLine(movePos, new Vector3(movePos.x,0, 0), showGizmoLines);
				}
				else if (axisMode == AxisMode.OnlyY)
				{
					movePos = new Vector3(master.transform.position.x, movePos.y, master.transform.position.z);
					GizmoLine(movePos, new Vector3(0, movePos.y, 0), showGizmoLines);
				}
				else if (axisMode == AxisMode.OnlyZ)
				{
					movePos = new Vector3(master.transform.position.x, master.transform.position.y, movePos.z);
					GizmoLine(movePos, new Vector3(0, 0, movePos.z), showGizmoLines);
				}
				
				Vector3 sMovePos = Vector3.Lerp(master.transform.position, movePos, Time.deltaTime * 5);
				master.transform.position = sMovePos;
			
				// Move multiple objects on surface
				if (isSnapToSurf)
				{
					int i;
					for (i=0; i < count; i++)
					{
						SnapToSurf(SelectedObjs[i], 100);
					}
				}
				
				// Snap to start position reset -> right mouse
				if (Input.GetMouseButtonDown (kM.mouseMoveSnapBack))
				{
					master.transform.position = cachePos;
					isReset = true;
					master.transform.DetachChildren();
					Destroy(master);
					isMultiAction = false;
				}
			}
			// Single selection
			else if (curObj != null && !isMultiAction) 
			{
				if (spaceMode == SpaceMode.World)
				{
					if (axisMode == AxisMode.OnlyX)
					{
						movePos = new Vector3(movePos.x, curObj.transform.position.y, curObj.transform.position.z);
						GizmoLine(movePos, new Vector3(movePos.x,0, 0), showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyY)
					{
						movePos = new Vector3(curObj.transform.position.x, movePos.y, curObj.transform.position.z);
						GizmoLine(movePos, new Vector3(0, movePos.y, 0), showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyZ)
					{
						movePos = new Vector3(curObj.transform.position.x, curObj.transform.position.y, movePos.z);
						GizmoLine(movePos, new Vector3(0, 0, movePos.z), showGizmoLines);
					}
					
					Vector3 sMovePos = Vector3.Lerp(curObj.transform.position, movePos, Time.deltaTime * 7);
					curObj.transform.position = sMovePos;
				}
				else if (spaceMode == SpaceMode.Local)
				{
					float inpAxis = Input.GetAxis("Mouse X") * Time.deltaTime * 55;
					if (axisMode == AxisMode.OnlyX)
					{
						curObj.transform.Translate(inpAxis,0,0);
						GizmoLine(curObj.transform.position, curObj.transform.right, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyY)
					{
						curObj.transform.Translate(0,inpAxis,0);
						GizmoLine(curObj.transform.position, curObj.transform.up, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyZ)
					{
						curObj.transform.Translate(0,0,inpAxis);
						GizmoLine(curObj.transform.position, curObj.transform.forward, showGizmoLines);
					}
					else if (axisMode == AxisMode.Default)
					{
						Vector3 sMovePos = Vector3.Lerp(curObj.transform.position, movePos, Time.deltaTime * 2);
						curObj.transform.position = sMovePos;
					}
				}
				// Move object on surface
				if (isSnapToSurf)
				{
					SnapToSurf(curObj, 100);
				}
				// Snap to start position reset -> right mouse
				if (Input.GetMouseButtonDown (kM.mouseMoveSnapBack))
				{
					curObj.transform.position = cachePos;
					isReset = true;
					curObj = null;
				}
			}
		}
		else if (Input.GetMouseButtonUp (kM.mouseMove))
		{
			curObj = null;
			if (isMultiAction)
			{
				master.transform.DetachChildren();
				Destroy(master);
				isMultiAction = false;
			}
		}
	}
	
	//=================================================================================================================o
	void _RotateObj ()
	{
		float xRot = Input.GetAxis ("Mouse Y");
		float yRot = Input.GetAxis ("Mouse X");
		int count = SelectedObjs.Count;
		if (Input.GetMouseButtonDown (kM.mouseRotate))
		{
			if (count > 1)
			{
				isMultiAction = true;
				curObj = SelectedObjs[0];
				dist = Vector3.Distance(cam.position, curObj.transform.position);
				Vector3 mP = Input.mousePosition;
				Ray r = cam.camera.ScreenPointToRay(mP);
				Vector3 movePos = r.GetPoint(dist);
				if (master == null)
					master = (GameObject)Instantiate(cursorPref, movePos, Quaternion.identity);
				else
					master.transform.position = movePos;
				master.name = "_master";
				master.layer = cursorPref.layer;
				cacheRot = master.transform.rotation;
				isReset = false;
				
				int i;
				for (i=0; i < count; i++)
				{
					SelectedObjs[i].transform.parent = master.transform;
				}
			}
			else if (count == 1)
			{
				isMultiAction = false;
				curObj = SelectedObjs[0];
				cacheRot = curObj.transform.rotation;
				isReset = false;
			}
			else 
			{
				Debug.Log ("No Object(s) selected");
			}
		}
		if (Input.GetMouseButton (kM.mouseRotate) && !isReset) 
		{
			
			if (isMultiAction && master != null)
			{
				if (axisMode == AxisMode.OnlyX)
				{
					Vector3 rotVec = new Vector3(xRot,0,0);
					master.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed);
					GizmoLine(master.transform.position, rotVec, showGizmoLines);
				}
				else if (axisMode == AxisMode.OnlyY)
				{
					Vector3 rotVec = new Vector3(0,-yRot,0);
					master.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed);
					GizmoLine(master.transform.position, rotVec, showGizmoLines);
				}
				else if (axisMode == AxisMode.OnlyZ)
				{
					Vector3 rotVec = new Vector3(0,0,yRot);
					master.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed);
					GizmoLine(master.transform.position, rotVec, showGizmoLines);
				}
				else if (axisMode == AxisMode.Default)
				{
					Vector3 rotVec = new Vector3(-xRot,-yRot,Input.GetAxis("Mouse ScrollWheel"));//new Vector3(xRot,-yRot,0);
					master.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed);
				}
				
				
				// Snap to start Rotation reset -> right mouse
				if (Input.GetMouseButtonDown (kM.mouseRotateSnapBack))
				{
					master.transform.rotation = cacheRot;
					isReset = true;
					master.transform.DetachChildren();
					Destroy(master);
					isMultiAction = false;
				}
			}
			// Single object
			else if (!isMultiAction && curObj != null) 
			{
				if (axisMode == AxisMode.OnlyX)
				{
					Vector3 rotVec = new Vector3(yRot,0,0);
					if (spaceMode == SpaceMode.World)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.World);
						GizmoLine(curObj.transform.position, rotVec, showGizmoLines);
					}
					else if (spaceMode == SpaceMode.Local)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.Self);
						GizmoLine(curObj.transform.position, curObj.transform.right, showGizmoLines);
					}
				}
				else if (axisMode == AxisMode.OnlyY)
				{
					Vector3 rotVec = new Vector3(0,-yRot,0);
					if (spaceMode == SpaceMode.World)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.World);
						GizmoLine(curObj.transform.position, rotVec, showGizmoLines);
					}
					else if (spaceMode == SpaceMode.Local)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.Self);
						GizmoLine(curObj.transform.position, curObj.transform.up, showGizmoLines);
					}
				}
				else if (axisMode == AxisMode.OnlyZ)
				{
					Vector3 rotVec = new Vector3(0,0,yRot);
					if (spaceMode == SpaceMode.World)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.World);
						GizmoLine(curObj.transform.position, rotVec, showGizmoLines);
					}
					else if (spaceMode == SpaceMode.Local)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.Self);
						GizmoLine(curObj.transform.position, curObj.transform.forward, showGizmoLines);
					}
				}
				else if (axisMode == AxisMode.Default)
				{
					Vector3 rotVec = new Vector3(-xRot,-yRot,Input.GetAxis("Mouse ScrollWheel"));//new Vector3(xRot,-yRot,0);
					if (spaceMode == SpaceMode.World)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.World);
					}
					else if (spaceMode == SpaceMode.Local)
					{
						curObj.transform.Rotate(rotVec * Time.deltaTime * rotateSpeed,Space.Self);
					}
				}
				
				// Snap to start position reset -> right mouse
				if (Input.GetMouseButtonDown (kM.mouseRotateSnapBack))
				{
					curObj.transform.rotation = cacheRot;
					isReset = true;
					curObj = null;
				}
			}
		}
		if (Input.GetMouseButtonUp (kM.mouseRotate))
		{
			curObj = null;
			if (isMultiAction)
			{
				master.transform.DetachChildren();
				Destroy(master);
				isMultiAction = false;
			}
		}
	}
	//=================================================================================================================o
	void _ScaleObj ()
	{
		int count = SelectedObjs.Count;
		
		if (Input.GetMouseButtonDown(kM.mouseScale))
		{
			if (count > 1)
			{
				isMultiAction = true;
				curObj = SelectedObjs[0];
				dist = Vector3.Distance(cam.position, curObj.transform.position);
				Vector3 mP = Input.mousePosition;
				Ray r = cam.camera.ScreenPointToRay(mP);
				Vector3 movePos = r.GetPoint(dist);
				if (master == null)
					master = (GameObject)Instantiate(cursorPref, movePos, Quaternion.identity);
				else
					master.transform.position = movePos;
				master.name = "_master";
				master.layer = cursorPref.layer;
				cacheScale = curObj.transform.localScale;
				isReset = false;
				master.transform.localScale = curObj.renderer.bounds.size;
			}
			else if (count == 1)
			{
				isMultiAction = false;
				curObj = SelectedObjs[0];
				cacheScale = curObj.transform.localScale;
				isReset = false;
				dist = Vector3.Distance(cam.position, curObj.transform.position);
			}
			else 
			{
				Debug.Log ("No Object(s) selected");
			}
		}
		else if (Input.GetMouseButton(kM.mouseScale) && !isReset)
		{
			Vector3 mP = Input.mousePosition;
			Ray r = cam.camera.ScreenPointToRay(mP);
			Vector3 movePos = r.GetPoint(dist);
			
			if (curObj != null)
			{
				if (isMultiAction)
				{
					float d = Vector3.Distance(master.transform.position, movePos) - 10;
					Vector3 scaleMult = new Vector3(d,d,d);
					if (axisMode == AxisMode.OnlyX)
					{
						scaleMult = new Vector3(d, curObj.transform.localScale.y, curObj.transform.localScale.z);
						GizmoLine(curObj.transform.position, curObj.transform.right, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyY)
					{
						scaleMult = new Vector3(curObj.transform.localScale.x, d, curObj.transform.localScale.z);
						GizmoLine(curObj.transform.position, curObj.transform.up, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyZ)
					{
						scaleMult = new Vector3(curObj.transform.localScale.x, curObj.transform.localScale.y, d);
						GizmoLine(curObj.transform.position, curObj.transform.forward, showGizmoLines);
					}
					else if (axisMode == AxisMode.Default)
					{
						scaleMult = new Vector3(d,d,d);
					}
					if(showGizmoLines)
						Debug.DrawLine(curObj.transform.position, movePos,Color.green);
					
					float sTime = Mathf.Lerp(0,Time.deltaTime * scaleSpeed, Time.deltaTime);
					Vector3 sScale = Vector3.Lerp(curObj.transform.localScale, scaleMult, sTime);
					
					int i;
					for (i=0; i < count; i++)
					{
						SelectedObjs[i].transform.localScale = sScale;
						
						// Snap to start scale, reset -> right mouse
						if (Input.GetMouseButtonDown (kM.mouseScaleSnapBack))
						{
							SelectedObjs[i].transform.localScale = cacheScale;
							isReset = true;
						}
					}
				}
				else if (!isMultiAction) // One Object
				{
					float d = Vector3.Distance(curObj.transform.position, movePos) - 10;
					Vector3 scaleMult = new Vector3(d,d,d);
					if (axisMode == AxisMode.OnlyX)
					{
						scaleMult = new Vector3(d, curObj.transform.localScale.y, curObj.transform.localScale.z);
						GizmoLine(curObj.transform.position, curObj.transform.right, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyY)
					{
						scaleMult = new Vector3(curObj.transform.localScale.x, d, curObj.transform.localScale.z);
						GizmoLine(curObj.transform.position, curObj.transform.up, showGizmoLines);
					}
					else if (axisMode == AxisMode.OnlyZ)
					{
						scaleMult = new Vector3(curObj.transform.localScale.x, curObj.transform.localScale.y, d);
						GizmoLine(curObj.transform.position, curObj.transform.forward, showGizmoLines);
					}
					else if (axisMode == AxisMode.Default)
					{
						scaleMult = new Vector3(d,d,d);
					}
					if(showGizmoLines)
						Debug.DrawLine(curObj.transform.position, movePos,Color.green);
					
					float sTime = Mathf.Lerp(0,Time.deltaTime * scaleSpeed, Time.deltaTime);
					Vector3 sScale = Vector3.Lerp(curObj.transform.localScale, scaleMult, sTime);
					curObj.transform.localScale = sScale;
					
					
					// Snap to start scale reset -> right mouse
					if (Input.GetMouseButtonDown (kM.mouseScaleSnapBack))
					{
						curObj.transform.localScale = cacheScale;
						isReset = true;
						curObj = null;
					}
				}
			}
		}
		else if (Input.GetMouseButtonUp(kM.mouseScale))
		{
			curObj = null;
			if (isMultiAction)
			{
				Destroy(master);
				isMultiAction = false;
			}
		}
	}
	//=================================================================================================================o
	void _RandomRotation ()
	{
		if (SelectedObjs.Count > 0)
		{
			int i;
			int count = SelectedObjs.Count;
			Transform cur = null;
			for (i=0; i < count; i++)
			{
				if (axisMode == AxisMode.Default)
				{
					Quaternion q = Random.rotation;
					cur = SelectedObjs[i].transform;
					if(rdmScaleRotToChilds)
					{
						foreach(Transform t in cur)
							if(t.name != selectorName)
								t.rotation = Random.rotation;
					}
					else
						cur.rotation = q;
				}
				else if (axisMode == AxisMode.OnlyX)
				{
					Quaternion rdmRotX = Quaternion.Euler(Random.Range(0,360),0,0);
					cur = SelectedObjs[i].transform;
					if(rdmScaleRotToChilds)
					{
						foreach(Transform t in cur)
							if(t.name != selectorName)
								t.rotation *=  rdmRotX;
					}
					else
						cur.rotation *= rdmRotX;
				}
				else if (axisMode == AxisMode.OnlyY)
				{
					Quaternion rdmRotY = Quaternion.Euler(0,Random.Range(0,360),0);
					cur = SelectedObjs[i].transform;
					if(rdmScaleRotToChilds)
					{
						foreach(Transform t in cur)
							if(t.name != selectorName)
								t.rotation *= rdmRotY;
					}
					else
						cur.rotation *= rdmRotY;
				}
				else if (axisMode == AxisMode.OnlyZ)
				{
					Quaternion rdmRotZ = Quaternion.Euler(0,0,Random.Range(0,360));
					cur = SelectedObjs[i].transform;
					if(rdmScaleRotToChilds)
					{
						foreach(Transform t in cur)
							if(t.name != selectorName)
								t.rotation *= rdmRotZ;
					}
					else
						cur.rotation *= rdmRotZ;
				}
			}
		}
	}
	//=================================================================================================================o
	void _RandomScale ()
	{
		if (SelectedObjs.Count > 0)
		{
			int i;
			int count = SelectedObjs.Count;
			Vector3 newScale = Vector3.one;
			Transform cur = null;
			for (i=0; i < count; i++)
			{
				float rScale = Random.Range(minScale, maxScale);
				cur = SelectedObjs[i].transform;
				if (axisMode == AxisMode.Default)
				{
					newScale = new Vector3(rScale,rScale,rScale);
				}
				else if (axisMode == AxisMode.OnlyX)
				{
					newScale = new Vector3(rScale,  SelectedObjs[i].transform.localScale.y, SelectedObjs[i].transform.localScale.z);
				}
				else if (axisMode == AxisMode.OnlyY)
				{
					newScale = new Vector3(SelectedObjs[i].transform.localScale.x, rScale, SelectedObjs[i].transform.localScale.z);
				}
				else if (axisMode == AxisMode.OnlyZ)
				{
					newScale = new Vector3(SelectedObjs[i].transform.localScale.x, SelectedObjs[i].transform.localScale.y, rScale);
				}
				
				
				if(rdmScaleRotToChilds)
				{
					foreach(Transform t in cur)
					{
						float cScale = Random.Range(minScale, maxScale);
						if(t.name != selectorName)
							t.localScale = new Vector3(cScale,cScale,cScale);
					}
				}
				else
					cur.localScale = newScale;
			}
		}
	}
	//=================================================================================================================o
	void PhysXDrop ()
	{
		if (editMode == EditMode.Brush)
		{
			if (selectMode == SelectMode.Tagged)
			{
				if (animObj != null && animObj.transform.childCount > 0)
				{
					foreach (Transform t in animObj.transform)
					{
						if (!t.CompareTag(animatableTag) && t.name != selectorName)
						{
							if (t.collider == null)
							{
								t.gameObject.AddComponent<BoxCollider>();
							}
							if (t.rigidbody == null)
							{
								t.gameObject.AddComponent<Rigidbody>();
							}
							if (t.rigidbody && t.collider)
							{
								if (removeAddedRigidBody)
								{
									StartCoroutine(DelRigidbody(t.rigidbody, delAfterSec));
								}
								if (removeAddedCollider)
								{
									StartCoroutine(DelCollider(t.collider, delAfterSec));
								}
							}
						}
					}
				}
			}
			else
			{
				if (SelectedObjs.Count > 0)
				{
					int i;
					int count = SelectedObjs.Count;
					for (i=0; i<count; i++)
					{
						if (SelectedObjs[i].collider == null)
						{
							SelectedObjs[i].AddComponent<BoxCollider>();
						}
						if (SelectedObjs[i].rigidbody == null)
						{
							SelectedObjs[i].AddComponent<Rigidbody>();
						}
						if (SelectedObjs[i].rigidbody && SelectedObjs[i].collider)
						{
							if (removeAddedRigidBody)
							{
								StartCoroutine(DelRigidbody(SelectedObjs[i].rigidbody, delAfterSec));
							}
							if (removeAddedCollider)
							{
								StartCoroutine(DelCollider(SelectedObjs[i].collider, delAfterSec));
							}
						}
					}
				}
			}
		}
	}
	//=================================================================================================================o
	static IEnumerator DelRigidbody (Rigidbody r, float time)
	{
		yield return new WaitForSeconds(time);
			if (r != null)
				Destroy(r);
	}
	//=================================================================================================================o
	static IEnumerator DelCollider (Collider c, float time)
	{
		if (doRecAnimFold) // stabilizing for animation recording
		{
			BoxCollider bc = (BoxCollider)c;
			bc.size /= 1.8f;
		}
		
		yield return new WaitForSeconds(time);
		if (c != null)
			Destroy(c);
	}
	//=================================================================================================================o
	static IEnumerator ResetMeshCol (GameObject g, float time)
	{
		yield return new WaitForSeconds(time);
		if (g != null)
		{
			if (g.GetComponent<MeshCollider>())
				g.GetComponent<MeshCollider>().convex = false;
		}
	}
	//=================================================================================================================o
	static void AddCollisionSensor (GameObject target)
	{
		if (target != null)
			target.AddComponent<ColSensorPE>();
	}
	//=================================================================================================================o
	IEnumerator _SaveFile ()
	{
		isSaving = true;
		List <GameObject> keys = new List<GameObject> (EditedObjs.Keys);
		int i;
		int count = keys.Count;
		if (count != 0)
		{
			cont.Items.Clear(); 
			for (i=0; i < count; i++)
			{
				Item itm = new Item ();
				
				itm.Name = keys[i].name;
				itm.ID = keys[i].GetInstanceID();
				itm.pX = keys[i].transform.position.x;
				itm.pY = keys[i].transform.position.y;
				itm.pZ = keys[i].transform.position.z;
				
				itm.rX = keys[i].transform.rotation.x;
				itm.rY = keys[i].transform.rotation.y;
				itm.rZ = keys[i].transform.rotation.z;
				itm.rW = keys[i].transform.rotation.w;
				
				itm.sX = keys[i].transform.localScale.x;
				itm.sY = keys[i].transform.localScale.y;
				itm.sZ = keys[i].transform.localScale.z;
				
				cont.Items.Add (itm);
			}
			// Save some recent settings for the next session
			Setting stg = new Setting();
			
			stg.camPX = cam.position.x;
			stg.camPY = cam.position.y;
			stg.camPZ = cam.position.z;
			
			stg.camRX = cam.rotation.x;
			stg.camRY = cam.rotation.y;
			stg.camRZ = cam.rotation.z;
			stg.camRW = cam.rotation.w;
			
			stg.mX = mX;
			stg.mY = mY;
			
			stg.bSize = brushScale;
			stg.maxRScale = maxScale;
			stg.minRScale = minScale;
			stg.bForce = physBrushForce;
			stg.delSec = delAfterSec;
			
			stg.cMode = curMode;
			stg.cSelectM = curSelectM;
			stg.cAxis = curAxis;
			stg.cSpace = curSpaceM;
			stg.cBrushM = curBrushM;
			
			stg.useScaleToBrush = useRdmScaleForBrush;
			stg.useDelOnImP = useRigDelOnImp;
			stg.useShowSelected = showSelected;
			
			cont._Setting = stg;
			
			cont.Save (Application.dataPath + "/PlayEditorSave/LastSave.xml");
			Debug.Log (cont.Items.Count + " Item(s) saved");
			
			yield return new WaitForSeconds (2f);
			isSaving = false;
			Debug.Log ("..ready to save");
		}
		else 
			Debug.Log ("No Object(s) to save!");
		
	}
	//=================================================================================================================o
	IEnumerator _LoadFile ()
	{
		isLoading = true;
		ContainerPE c = ContainerPE.Load (Application.dataPath + "/PlayEditorSave/LastSave.xml");
		
		List <GameObject> keys = new List<GameObject> (EditedObjs.Keys);
		List <int> values = new List<int> (EditedObjs.Values);
		for (int i=0; i < c.Items.Count; i++)
		{
			for (int e=0; e < values.Count; e++)
			{
				if (values[e] == c.Items[i].ID)
				{
					keys[i].transform.position = new Vector3(c.Items[i].pX, c.Items[i].pY, c.Items[i].pZ);
					keys[i].transform.rotation = new Quaternion(c.Items[i].rX, c.Items[i].rY, c.Items[i].rZ, c.Items[i].rW);
					keys[i].transform.localScale = new Vector3(c.Items[i].sX, c.Items[i].sY, c.Items[i].sZ);
				}
			}
		}
		
		yield return new WaitForSeconds (2f);
		isLoading = false;
		Debug.Log ("..ready to load");
	}
	//=================================================================================================================o
	IEnumerator _LoadSettings ()
	{
		if (!File.Exists (Application.dataPath + "/PlayEditorSave/LastSave.xml")) 
		{
			Debug.Log("No save file to load from - " + Application.dataPath + "/PlayEditorSave/LastSave.xml");
			yield break;
		}
		
		// Load last settings
		ContainerPE c = ContainerPE.Load (Application.dataPath + "/PlayEditorSave/LastSave.xml");
		
		cam.position = new Vector3(c._Setting.camPX,c._Setting.camPY,c._Setting.camPZ);
		cam.rotation = new Quaternion(c._Setting.camRX,c._Setting.camRY,c._Setting.camRZ,c._Setting.camRW);
		
		mX = c._Setting.mX;
		mY = c._Setting.mY;
		
		brushScale = c._Setting.bSize;
		maxScale = c._Setting.maxRScale;
		minScale = c._Setting.minRScale;
		physBrushForce = c._Setting.bForce;
		delAfterSec = c._Setting.delSec;
			
		curMode = c._Setting.cMode;
		editMode = (EditMode)curMode; 
		curSelectM = c._Setting.cSelectM;
		selectMode = (SelectMode)curSelectM; 
		curAxis = c._Setting.cAxis;
		axisMode = (AxisMode)curAxis; 
		curSpaceM = c._Setting.cSpace;
		spaceMode = (SpaceMode)curSpaceM; 
		curBrushM = c._Setting.cBrushM;
		brushMode = (BrushMode)curBrushM; 
		
		useRdmScaleForBrush = c._Setting.useScaleToBrush;
		useRigDelOnImp = c._Setting.useDelOnImP;
		showSelected = c._Setting.useShowSelected;
		
		yield return null;
	}
	//=================================================================================================================o
	static void SnapToSurf (GameObject g, float snapDist)
	{
		Vector3 gPos = g.transform.position;
		float groundH = g.renderer ? g.renderer.bounds.extents.y : g.transform.localScale.y;
		RaycastHit hit;
		if (Physics.Raycast(gPos, Vector3.down, out hit, snapDist))
		{
			Vector3 groundPos = hit.point + Vector3.up * groundH;
			Vector3 sPos = Vector3.Lerp(gPos, groundPos, Time.deltaTime * 4);
			g.transform.position = sPos;
		}
		if (hit.distance < groundH)
		{
			g.transform.Translate(Vector3.up * Time.deltaTime * 8, Space.World);
		}
	}
	//=================================================================================================================o
	static bool inGuiSpace (Rect l, Rect t)
	{
		Vector3 mP = Input.mousePosition;
		Vector3 realMP = new Vector3(mP.x,Screen.height-mP.y,0);
		if (l.Contains(realMP) || t.Contains(realMP))
			return true;
		else
			return false;
	}
	//=================================================================================================================o
	static Vector2 WorldToScreen (GameObject g)
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint (g.transform.position);
		Vector2 point = new Vector2 (screenPos.x, Screen.height - screenPos.y);
		return point; 
	}
	//=================================================================================================================o
	static void SelectObj (GameObject target, GameObject selectorPrefab, string selectorName, bool lockSelection)
	{
		foreach (Transform t in target.transform) {
			if (t.name == selectorName) // if child has a selector
				return;
		}
		if (target.name == selectorName) // no selectors
			return;
		if (Pool.PoolObjs.Count == 0) // if the pool is empty
			return;
		if (target.renderer != null) 
		{
			Renderer r = target.GetComponent<Renderer> ();
			Vector3 boundsCenter = r.bounds.center;
			Vector3 boundsSize = r.bounds.size;
			Quaternion targetRot = target.transform.rotation;
			GameObject sel = Pool.Add(selectorPrefab, boundsCenter, targetRot, showSelected);
			sel.transform.localScale = boundsSize * 1.01f;
			sel.name = selectorName;
			sel.transform.parent = target.transform;
		}
		else // No renderer
		{
			Vector3 center = target.transform.position;
			Quaternion targetRot = target.transform.rotation;
			GameObject sel = Pool.Add(selectorPrefab, center, targetRot, showSelected);
			sel.transform.localScale = target.transform.localScale * 1.01f;
			sel.name = selectorName;
			sel.transform.parent = target.transform;
		}
		 // Do not select AnimTagged root items, GrabTagged needs to be moveable childs
		if (!target.CompareTag("AnimTag") && lockSelection)
		{
			SelectedObjs.Add(target);
		}
	}
	//=================================================================================================================o
	static void DeselectObj (GameObject target, string selectorName, bool lockSelection)
	{
		if (target != null)
		{
			Transform tf = target.transform;
			foreach (Transform t in tf)
			{
				if (t.name == selectorName)
				{
					Pool.Remove(t.gameObject);
					// Do not select AnimTagged root items, GrabTagged needs to be moveable childs
					if (!target.CompareTag("AnimTag") && lockSelection) 
					{
						SelectedObjs.Remove(target);
					}
				}
			}
		}
	}
	//=================================================================================================================o
	static void ShowHideSelection (string selName)
	{
		showSelected = !showSelected;
		
		int i;
		List<GameObject> sels = new List<GameObject>(SelectedObjs);
		for (i=0; i < sels.Count; i++)
		{
			foreach(Transform t in sels[i].transform)
			{
				if (t.name == selName)
				{
					if (showSelected)
					{
						t.renderer.enabled = true;
					}
					else
					{
						t.renderer.enabled = false;
					}
				}
			}
		}
	}
	//=================================================================================================================o
	void SelectAll ()
	{
		int i;
		List<GameObject> editGos = new List<GameObject>(EditedObjs.Keys);
		for (i=0; i < editGos.Count; i++)
		{
			SelectObj(editGos[i],selectorPref, selectorName, lockAnimTaggedObjs);
		}
	}
	//=================================================================================================================o
	void DeleteSelected ()
	{
		int i;
		List<GameObject> sels = new List<GameObject>(SelectedObjs);
		for (i=0; i < sels.Count; i++)
		{
			EditedObjs.Remove(sels[i]);
			DeselectObj(sels[i], selectorName, lockAnimTaggedObjs);
			if (!sels[i].CompareTag(animatableTag) && !sels[i].CompareTag(grabableTag)) // destroy if not grab and anim tag
				Destroy(sels[i]);
		}
	}
	//=================================================================================================================o
	static IEnumerator _RecStart (GameObject g, string tag)
	{
		if (g == null || g.transform.childCount == 0 || !g.CompareTag(tag))
		{
			Debug.Log("No valid animation object selected!");
			yield break;
		}
		
		recPE = g.AddComponent<RecorderPE>();
		recPE.keyTimeInterval = recAnimTimePulse;
		recPE.recPosition = doRecPos;
		recPE.recRotation = doRecRot;
		recPE.recScale = doRecScale;
		isRecording = true;
		Debug.Log("Recording started");
	}
	//=================================================================================================================o
	static IEnumerator _RecEnd (GameObject g, string tag)
	{
		if (g == null || g.transform.childCount == 0 || !g.CompareTag(tag))
		{
			Debug.Log("No valid animation object selected!");
			yield break;
		}
		recPE.StopRecording();
		isRecording = false;
		Debug.Log("Recording stopped");
	}
	//=================================================================================================================o
	IEnumerator _RecAnimation (GameObject g, string tag, bool status)
	{
		if (status)
		{
			yield return StartCoroutine(_RecEnd(g,tag));
			setupRec = true;
		}
		else
		{
			yield return StartCoroutine(_RecStart(g,tag));
			setupRec = true;
		}
		
		yield return new WaitForSeconds(2); // Cooldown
		setupRec = false;
	}
	//=================================================================================================================o
	static void GizmoLine (Vector3 pos, Vector3 dir, bool visible)
	{
		if (visible)
		{
			Debug.DrawLine(pos + dir * 400, pos - dir * 400, Color.green);
		}
	}
	//=================================================================================================================o
	void OnGUI ()
	{    
		if (isGUIVisible)
		{
			GUI.Box(_LeftPanel,"");
			GUI.Box(_TopPanel,"");
			if (!isMultiAction)
			{
				if (GUI.Button (_Save, "Save")) { 
					if (!isSaving)
					{
						StartCoroutine(_SaveFile());
					}
				} 
				if (GUI.Button (_Load, "Load")) { 
					if (!isLoading)
						StartCoroutine(_LoadFile());
				}  
				// Random Scale / Rotation
				if (GUI.Button (_RdomRot, "Rotate Random")) { 
					_RandomRotation();
				}  
				if (GUI.Button (_RdomScale, "Scale Random")) { 
					_RandomScale();
				}  
				GUI.Label(_MinScaleLabel,"Min. Scale : " + minScale.ToString("f1"),EditorStyles.whiteLabel);
				minScale = GUI.HorizontalSlider(_MinScale, minScale, 0.3f, 30.0f);
				GUI.Label(_MaxScaleLabel,"Max. Scale : " + maxScale.ToString("f1"),EditorStyles.whiteLabel);
				maxScale = GUI.HorizontalSlider(_MaxScale, maxScale, 0.3f, 30.0f);
				// Show hide Selected
				if (GUI.Button (_ShowSeleRect, "Show Select.")) { 
					ShowHideSelection(selectorName);
				}  
				// Delete Selected	
				if (GUI.Button (_DeleteSelect, "Delete Select.")) { 
					DeleteSelected();
				} 
				GUI.Box(_EditCounterRect, "Edited : "+EditedObjs.Count.ToString());
				// Recording
				doRecAnimFold = GUI.Toggle(_RecAnimFoldTog,doRecAnimFold," Rec Anim" +" ("+kM.recordToggle.ToString()+")");
				if (doRecAnimFold)
				{
					if(isRecording)
					{
						GUI.contentColor = Color.red;
					}
					if (GUI.Button (_RecAnimRect, "Rec / Stop")) { 
						
						if (!setupRec)
						{
							StartCoroutine(_RecAnimation(animObj, animatableTag, isRecording));
						}
					}
					if(!isRecording)
					{
						GUI.Label(_RecTimeLabel,"Key Time : " + recAnimTimePulse.ToString("f1"),EditorStyles.whiteLabel);
						recAnimTimePulse = GUI.HorizontalSlider(_RecKeyTimer, recAnimTimePulse, 0.1f, 10.0f);
						
						doRecPos = GUI.Toggle(_RecPosTog,doRecPos,"P");
						doRecRot = GUI.Toggle(_RecRotTog,doRecRot,"R");
						doRecScale = GUI.Toggle(_RecScaleTog,doRecScale,"S");
					}
					else
					{
						GUI.contentColor = Color.white;
					}
				}
				 // Mouse Drag
				if (Event.current.type == EventType.mouseDrag && !Input.GetMouseButton(1)){
					dragging = true;
				}
				if (Event.current.type == EventType.mouseUp){
					dragging = false;
				}
				
				curMode = GUI.SelectionGrid(_ModeRect,curMode,toolGridS,1);
				editMode = (EditMode)curMode;
			}
			// Brush
			if (curMode == (int)EditMode.Brush)
			{
				curBrushM = GUI.SelectionGrid(_BrushModeRect,curBrushM,brushModeGrid,4);
				brushMode = (BrushMode)curBrushM;
				GUI.Label(_brushScaleLabel,"Brush Scale : " + brushScale.ToString("f2"),EditorStyles.whiteLabel);
				brushScale = GUI.HorizontalSlider(_BrushScale, brushScale, 0.2f, 50.0f);
				useRdmScaleForBrush = GUI.Toggle(_RdmScaleBrushRect, useRdmScaleForBrush,"Scale to Brush");
				
				GUI.Label(_OffsetBSLabel,"Brush Offset : " + brushOffset.ToString("f1"),EditorStyles.whiteLabel);
				brushOffset = GUI.HorizontalSlider(_OffSetBrushSlider, brushOffset, -brushOffsetRange, brushOffsetRange);
				
				continuousBrush = GUI.Toggle(_ContBrushTog, continuousBrush,"cont. Brush.");
				if(continuousBrush)
				{
					brushSpacing = GUI.HorizontalSlider(_ContBrushSlider, brushSpacing, 0.0f, 2.0f);
					GUI.Label(_ContBSLabel,brushSpacing.ToString("f1"),EditorStyles.whiteLabel);
				}
				
				if (brushMode == BrushMode.PhysX)
				{
					if (GUI.Button (_PhysXDrop, "PhysX Drop")) { 
						PhysXDrop();
					}  
					removeAddedCollider = GUI.Toggle(_delColRect,removeAddedCollider," Del Col");
					removeAddedRigidBody = GUI.Toggle(_delRigRect,removeAddedRigidBody," Del Rig");
					delAfterSec = GUI.HorizontalSlider(_remTimeRect, delAfterSec, 1.0f, 100.0f);
					GUI.Label(_remTimeLabel,"Del Time: " +delAfterSec.ToString("f0"),EditorStyles.whiteLabel);
					physBrushForce = GUI.HorizontalSlider(_ForceRect, physBrushForce, 5.0f, 200.0f);
					GUI.Label(_ForceLabel,"Force: " + physBrushForce.ToString("f0"),EditorStyles.whiteLabel);
					useRigDelOnImp = GUI.Toggle(_DeloImpRect,useRigDelOnImp," Del on Imp");
				}
			}
			// Select
			else if (curMode == (int)EditMode.Select) 
			{
				if (selectMode == SelectMode.Multi)
				{
					if (dragging)
					{
						GUI.Box (_SelectionBox, "");
					}
				}
				else
				{
					GUI.Label(_singleSelectLabel,"Single Sel. Size: " +singleSelectionSize.ToString("f0")+"px", EditorStyles.whiteLabel);
					singleSelectionSize = GUI.HorizontalSlider(_SingleSelectSliderRect, singleSelectionSize, 10.0f, 500.0f);
				}
				
				if (GUI.Button (_SeleAllRect, "Select All")) { 
					SelectAll();
				}  
				curSelectM = GUI.SelectionGrid(_SelModeRect,curSelectM,selectModeGrid,3);
				selectMode = (SelectMode)curSelectM;
			}
			// Rotate
			else if (curMode == (int)EditMode.Rotate || curMode == (int)EditMode.Move || curMode == (int)EditMode.Scale) 
			{
				curAxis = GUI.SelectionGrid(_AxisModeRect,curAxis,axisGrid,4);
				axisMode = (AxisMode)curAxis;
				if (curMode == (int)EditMode.Move)
				{
					isSnapToSurf = GUI.Toggle(_SnapRect,isSnapToSurf," Snap to Surf");
					curSpaceM = GUI.SelectionGrid(_SpaceModeRect,curSpaceM,spaceModeGrid,2);
					spaceMode = (SpaceMode)curSpaceM;
				}
				else if (curMode == (int)EditMode.Rotate)
				{
					curSpaceM = GUI.SelectionGrid(_SpaceModeRect,curSpaceM,spaceModeGrid,2);
					spaceMode = (SpaceMode)curSpaceM;
				}
			}
		}
		// Toggle GUI visible
		isGUIVisible = GUI.Toggle(_VisibleRect,isGUIVisible," Show GUI");
	}
}

//=====================================================>POOL<==========================================================o
public class Pool : MonoBehaviour
{
	public static GameObject holder;
	public static List<GameObject> PoolObjs = new List<GameObject>();
	
	public static void CreatePoolWith (GameObject _prefab, int count, string name)
	{
		if (holder == null)
		{
			holder = new GameObject("_PoolHolder");
			holder.transform.position = Vector3.zero;
		}
		int i;
		GameObject clone;
		for (i=0; i < count; ++i)
		{
			clone = (GameObject)Instantiate (_prefab, holder.transform.position, Quaternion.identity);
			clone.transform.parent = holder.transform;
			clone.name = name;
			clone.layer = _prefab.layer;
			clone.renderer.enabled = false; 
			PoolObjs.Add(clone);
		}
	}
	public static GameObject Add (GameObject gO, Vector3 pos, Quaternion rot, bool isVisible)
	{
		if (PoolObjs.Count > 0)
		{
			gO = PoolObjs[PoolObjs.Count-1];
			PoolObjs.Remove(gO);
			gO.transform.parent = null;
			if (isVisible)
			{
				gO.renderer.enabled = true; 
			}
			gO.transform.position = pos;
			gO.transform.rotation = rot;
			return gO;
		}
		else
		{
			Debug.Log ("Out of Pool objects!");
			return gO;
		}
	}
	public static void Remove (GameObject gO)
	{
		PoolObjs.Add(gO);
		gO.renderer.enabled = false;
		gO.transform.position = holder.transform.position;
		gO.transform.parent = holder.transform;
	}
}
//=====================================================>POOL<==========================================================o
#endif
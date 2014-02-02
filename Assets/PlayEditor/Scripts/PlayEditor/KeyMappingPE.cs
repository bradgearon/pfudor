using UnityEngine;

public class KeyMappingPE : MonoBehaviour 
{
	// Camera
	public int mouseCamRot = 1;
	public int mouseCamDrag = 2;
	
	// Axis
	public KeyCode allAxis = KeyCode.Alpha1;
	public KeyCode xAxis = KeyCode.Alpha2;
	public KeyCode yAxis = KeyCode.Alpha3;
	public KeyCode zAxis = KeyCode.Alpha4;
	
	// Edit modi
	public KeyCode brushMode = KeyCode.X;
	public KeyCode selectMode = KeyCode.Q;
	public KeyCode moveMode = KeyCode.G;
	public KeyCode rotateMode = KeyCode.R;
	public KeyCode scaleMode = KeyCode.T;
	
	// Select/Space toggle
	public KeyCode selectModeToggle = KeyCode.B;
	public KeyCode spaceModeToggle = KeyCode.B;
	
	// Record Anim
	public KeyCode recordToggle = KeyCode.M;
	
	// Combo key 
	public KeyCode addToSelectionCombo = KeyCode.LeftShift;
	public KeyCode delCombo = KeyCode.LeftShift;
	
	// Brush
	public int mouseDownBrush = 0;
	public int mouseDownBrushDel = 1; // + delCombo key 
	
	// Select
	public int mouseSelect = 0; // + addToSelectionCombo key 
	public int mouseDeselect = 2; // Middle mouseb
	
	// Move
	public int mouseMove = 0;
	public int mouseMoveSnapBack = 1; // Right mouseb
	
	// Rotate
	public int mouseRotate = 0;
	public int mouseRotateSnapBack = 1; // Right mouseb
	
	// Scale
	public int mouseScale = 0;
	public int mouseScaleSnapBack = 1; // Right mouseb
}

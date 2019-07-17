using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MotionPath))]
[CanEditMultipleObjects]
public class MotionPathEditor : Editor 
{
	
	private SerializedObject path;
	private SerializedProperty controlPoints;
	private SerializedProperty samples;
    private SerializedProperty line;
    private SerializedProperty width;
    private SerializedProperty rounding;


    private static Vector3 textOffset = Vector3.down * 0.5f;
	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(30),
		indexWidth = GUILayout.MaxWidth(20);
	
	
	
	void OnSceneGUI()
	{
		MotionPath path = (MotionPath)target;
		Handles.matrix = path.transform.localToWorldMatrix;
        Undo.RecordObject(path, "MovePathPoints");
		
		GUIStyle controlPointText = new GUIStyle();
		controlPointText.normal.textColor = Color.green;
		controlPointText.fontSize = 20;
		
		GUIStyle lengthText = new GUIStyle();
		lengthText.normal.textColor = Color.cyan;
		lengthText.fontSize = 15;
		
		// Draw the length of the path in the center
		Handles.Label(path.centerPoint + Vector3.up, path.length.ToString(), lengthText);
		
		// Draw the number of the control point and the handle to translate it
		for (int i = 0; i < path.controlPoints.Length; i++)
		{
			if (i == path.controlPoints.Length -1)
			{
				if(!path.looping)
					Handles.Label(path.controlPoints[i] + textOffset, i.ToString(), controlPointText);
			}
			else
				Handles.Label(path.controlPoints[i] + textOffset, i.ToString(), controlPointText);
			
			
			Vector3 newPos = Handles.FreeMoveHandle(path.controlPoints[i], Quaternion.identity, 0.2f, Vector3.one, Handles.DotHandleCap);
			// Automatically rebuild the path luts if a point moves
			if (path.controlPoints[i] != newPos)
			{
				path.controlPoints[i] = newPos;
				path.Rebuild();
			}
		}
	}
	
	
	void OnEnable()
	{
		((MotionPath)target).Init();
		
		path = new SerializedObject(target);
		controlPoints = path.FindProperty("controlPoints");
        samples = path.FindProperty("samples");
        line = path.FindProperty("lineObject");
        width = path.FindProperty("width");
        rounding = path.FindProperty("rounding");
    }
	
	
	public override void OnInspectorGUI ()
	{
		path.Update();
		MotionPath pathObject = (MotionPath)target;
		GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(line, new GUIContent("Line Texture Prefab", string.Format("The texture used to render the line (line wont be rendered if not set)")));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(width, new GUIContent("Line Width", string.Format("The width of the rendered line (not needed if no line)")));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(rounding, new GUIContent("Line Rounding", string.Format("How rounded/polygonal the rendered line is (not needed if no line)")));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(samples, new GUIContent("Samples Per Span", string.Format("Total Samples = {0}", (pathObject.controlPoints.Length-1) * samples.intValue)));
		
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(20);
		
		
		GUILayout.Label("Path Points");
		// First row add button to for begining of path
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", indexWidth);
		if (GUILayout.Button("+", buttonWidth))
		{
			Vector3 start = controlPoints.GetArrayElementAtIndex(0).vector3Value;
			Vector3 end = controlPoints.GetArrayElementAtIndex(1).vector3Value;
			Vector3 norm = (start - end).normalized;
			controlPoints.InsertArrayElementAtIndex(0);
			controlPoints.GetArrayElementAtIndex(0).vector3Value = start + norm;
		}
		EditorGUILayout.EndHorizontal();
		
		int stopIndex = controlPoints.arraySize -1;
		for (int i = 0; i < controlPoints.arraySize; i++)
		{
			SerializedProperty
				point = controlPoints.GetArrayElementAtIndex(i);
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField(i.ToString(), indexWidth);
			if (GUILayout.Button("X", buttonWidth))
			{
				if (controlPoints.arraySize < 3)
					break;
				controlPoints.DeleteArrayElementAtIndex(i);
				if (i == stopIndex)
					break;
			}
			
			EditorGUILayout.PropertyField(point, GUIContent.none);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", indexWidth);
			if (GUILayout.Button("+", buttonWidth))
			{
				Vector3 start = controlPoints.GetArrayElementAtIndex(i).vector3Value;
				if (i == controlPoints.arraySize -1)
				{
					Vector3 pre = controlPoints.GetArrayElementAtIndex(i-1).vector3Value;
					Vector3 norm = (start - pre).normalized;
					controlPoints.InsertArrayElementAtIndex(i+1);
					controlPoints.GetArrayElementAtIndex(i+1).vector3Value = start + norm;
				}
				else
				{
					Vector3 end = controlPoints.GetArrayElementAtIndex(i+1).vector3Value;
					Vector3 newPoint = Vector3.Lerp(start, end, 0.5f);
					controlPoints.InsertArrayElementAtIndex(i+1);
					controlPoints.GetArrayElementAtIndex(i+1).vector3Value = newPoint;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		
		
		if (!pathObject.looping)
		{
			GUILayout.Space(5);
			if(GUILayout.Button("Make Loop"))
			{
				int i = controlPoints.arraySize-1;
				controlPoints.InsertArrayElementAtIndex(i);
				controlPoints.GetArrayElementAtIndex(i+1).vector3Value = controlPoints.GetArrayElementAtIndex(0).vector3Value;
			}
		}
		
		
		if(path.ApplyModifiedProperties())
			pathObject.Rebuild();
	}
	
	
}

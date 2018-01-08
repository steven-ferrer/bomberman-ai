using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		MapGenerator map = target as MapGenerator;
		if (GUILayout.Button ("GENERATE MAP")) {
			map.GenerateMap (); //Generate a random destructible walls
		}
	}
}

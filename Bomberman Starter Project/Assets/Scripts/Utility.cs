using System.Collections;
using UnityEngine;

public static class Utility{

	//Shuffle Array
	public static T[] ShuffleArray<T>(T[] array,int seed){
		System.Random rng = new System.Random (seed);

		for (int x = 0; x < array.Length - 1; x++) {
			int randomIndex = rng.Next (x, array.Length);
			T tempItem = array [randomIndex];
			array [randomIndex] = array [x];
			array [x] = tempItem;
		}
		return array;
	}

	//Round x y and z to integer
	public static Vector3 RoundToInt(Vector3 vector){
		return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
	}

}

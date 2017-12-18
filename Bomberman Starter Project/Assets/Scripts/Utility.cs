using System.Collections;

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

}

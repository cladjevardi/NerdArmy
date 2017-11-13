using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
	/// <summary>
	/// The single instance of a world manager. This class manages
	/// the players roster that move from mission to mission.
	/// </summary>
	public GameObject worldManager;

	/// <summary>
	/// The singleton instance of a sound manager. This class manages
	/// correcting repetitive sound effects and background music
	/// outside the scope of each scene for continuous playback.
	/// </summary>
	public GameObject soundManager;

	void Start ()
	{
		if (GameManager.instance == null)
			Instantiate(worldManager);

		if (SoundManager.instance == null)
			Instantiate(soundManager);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

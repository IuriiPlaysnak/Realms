using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalUnique : MonoBehaviour
{
    static GlobalUnique instance;

	// Use this for initialization
	void Awake () {
        if (instance != null)
            GameObject.Destroy(gameObject);
        else
            instance = this;

        GameObject.DontDestroyOnLoad(gameObject);
	}
}

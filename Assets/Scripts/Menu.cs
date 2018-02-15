using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    /// <summary>The canvas for UI.</summary>
    private GameObject canvas = null;

    /// <summary>The sideBar for button UI.</summary>
    private GameObject menuTest = null;

    // Use this for initialization
    void Awake ()
    {
        // Setup the Canvas for drawing UI elements.
        canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        Canvas can = canvas.AddComponent<Canvas>();
        can.renderMode = RenderMode.ScreenSpaceOverlay;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create the sidebar to contain the buttons.
        menuTest = new GameObject("Menu");
        menuTest.transform.SetParent(canvas.gameObject.transform);
        Image menuTestImage = menuTest.AddComponent<Image>();
        menuTestImage.material = GameManager.instance.uiMaterials[5];
        menuTestImage.transform.position = new Vector3(0, 0, -0.1f);
        RectTransform menuTestRect = menuTestImage.GetComponent<RectTransform>();
        menuTestRect.anchorMin = new Vector2(1, 0);
        menuTestRect.anchorMax = new Vector2(1, 1);
        menuTestRect.anchoredPosition = new Vector2(-360, 0);
        menuTestRect.sizeDelta = new Vector2(800, 0);
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}

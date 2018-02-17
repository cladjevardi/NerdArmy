using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    /// <summary>The canvas for UI.</summary>
    private GameObject canvas = null;

    /// <summary>The menu background for menu UI.</summary>
    private GameObject menuBackground = null;

    /// <summary>The menu title for the menu UI.</summary>
    private GameObject menuTitle = null;

    /// <summary>The playDemoButton for the menu UI.</summary>
    private GameObject playDemoButton = null;

    // Use this for initialization
    void Awake ()
    {
        // Setup the Canvas for drawing UI elements.
        canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        Canvas can = canvas.AddComponent<Canvas>();
        can.renderMode = RenderMode.ScreenSpaceOverlay;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Create the Menu Background to contain the buttons.
        menuBackground = new GameObject("Menu Background");
        menuBackground.transform.SetParent(canvas.gameObject.transform);
        Image menuBackgroundImage = menuBackground.AddComponent<Image>();
        menuBackgroundImage.material = GameManager.instance.uiMaterials[5];
        RectTransform menuBackgroundRect = menuBackgroundImage.GetComponent<RectTransform>();
        menuBackgroundRect.anchoredPosition = new Vector2(0, 0);
        menuBackgroundRect.sizeDelta = new Vector2(768, 768);

        // Create the Menu Title.
        menuTitle = new GameObject("menuTitle");
        menuTitle.transform.SetParent(canvas.gameObject.transform);
        Image menuTitleImage = menuTitle.AddComponent<Image>();
        menuTitleImage.material = GameManager.instance.uiMaterials[1];
        RectTransform menuTitleRect = menuTitleImage.GetComponent<RectTransform>();
        menuTitleRect.anchoredPosition = new Vector2(0, 120);
        menuTitleRect.sizeDelta = new Vector2(557, 191);

        /*
        // Create the Play Demo Button.
        playDemoButton = new GameObject("Play Demo playDemoButton");
        playDemoButton.AddComponent<Button>();
        // TODO: make playDemo play the demo
        //playDemoButton.GetComponent<Button>().onClick.AddListener(playDemo);
        playDemoButton.transform.SetParent(canvas.gameObject.transform);
        Image playDemoButtonImage = playDemoButton.AddComponent<Image>();
        playDemoButtonImage.material = GameManager.instance.uiMaterials[4];
        RectTransform playDemoButtonRect = playDemoButtonImage.GetComponent<RectTransform>();
        playDemoButtonRect.anchoredPosition = new Vector2(-170, -130);
        playDemoButtonRect.sizeDelta = new Vector2(320, 215);
        */

        // Create the Play Demo Button
        UiButton(GameManager.instance.uiMaterials[4], 
            new Vector2(320, 215), new Vector2(-170, -130), canvas);
    }

    /// <summary>Create a UI button</summary>
    /// <param name="material">The material for the button.</param>
    /// <param name="size">The Vector2 width and height of the button.</param>
    /// <param name="position">The Vector2 x and y anchor position of the button.</param>
    /// <param name="canvas">The canvas object to create the button on.</param>
    /// <returns>Returns whether the update loop should process the current frame as handled.</returns>
    public static UnityEngine.UI.Button UiButton(Material material, 
        Vector2 size, Vector2 position, GameObject canvas)
    {
        GameObject gameObject = new GameObject("Textured button (" + material.name + ")");

        Image image = gameObject.AddComponent<Image>();
        image.material = material;

        UnityEngine.UI.Button button = gameObject.AddComponent<UnityEngine.UI.Button>();
        gameObject.transform.SetParent(canvas.transform, false);

        image.rectTransform.sizeDelta = size;
        image.rectTransform.anchoredPosition = position;

        return button;
    }
}

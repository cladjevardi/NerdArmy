using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Events;

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

    /// <summary>List of all the game objects to be destroyed upon loading a level</summary>
    private List<GameObject> gameObjects = new List<GameObject>();

    // Use this for initialization.
    void Awake ()
    {
        // Clear any previously existing objects.
        gameObjects.Clear();

        // Setup the Canvas for drawing UI elements.
        canvas = new GameObject("Canvas");
        canvas.transform.SetParent(transform);
        Canvas can = canvas.AddComponent<Canvas>();
        GraphicRaycaster gr = canvas.AddComponent<GraphicRaycaster>();
        can.renderMode = RenderMode.ScreenSpaceOverlay;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        gameObjects.Add(canvas);

        // Create the Menu Background to contain the buttons.
        menuBackground = new GameObject("Menu_Background");
        menuBackground.transform.SetParent(canvas.gameObject.transform);
        Image menuBackgroundImage = menuBackground.AddComponent<Image>();
        menuBackgroundImage.material = GameManager.instance.uiMaterials[5];
        RectTransform menuBackgroundRect = menuBackgroundImage.GetComponent<RectTransform>();
        menuBackgroundRect.anchoredPosition = new Vector2(0, 0);
        menuBackgroundRect.sizeDelta = new Vector2(768, 768);
        gameObjects.Add(menuBackground);

        // Create the Menu Title.
        menuTitle = new GameObject("Menu_Title");
        menuTitle.transform.SetParent(canvas.gameObject.transform);
        Image menuTitleImage = menuTitle.AddComponent<Image>();
        menuTitleImage.material = GameManager.instance.uiMaterials[1];
        RectTransform menuTitleRect = menuTitleImage.GetComponent<RectTransform>();
        menuTitleRect.anchoredPosition = new Vector2(0, 120);
        menuTitleRect.sizeDelta = new Vector2(557, 191);
        gameObjects.Add(menuTitle);

        /*
        // Create the Play Demo Button.
        playDemoButton = new GameObject("Play Demo Button");
        playDemoButton.transform.SetParent(canvas.gameObject.transform);
        playDemoButton.AddComponent<Button>();
        playDemoButton.GetComponent<Button>().onClick.AddListener(HandleClick);
        Image playDemoButtonImage = playDemoButton.AddComponent<Image>();
        playDemoButtonImage.material = GameManager.instance.uiMaterials[4];
        RectTransform playDemoButtonRect = playDemoButtonImage.GetComponent<RectTransform>();
        playDemoButtonRect.anchoredPosition = new Vector2(-170, -130);
        playDemoButtonRect.sizeDelta = new Vector2(320, 215);
        gameObjects.Add(playDemoButton);
        */

        // Create the Play Demo Button.
        UiButton("Play_Demo", GameManager.instance.uiMaterials[4], 
            new Vector2(348.18f, 121.18f), new Vector2(-160, -64), gameObjects);

        // Create the Options Button.
        UiButton("Menu_Options", GameManager.instance.uiMaterials[3],
            new Vector2(178.18f, 60), new Vector2(230, -120), gameObjects);

        // Create the Mia's Story Button.
        UiButton("Mia's_Story", GameManager.instance.uiMaterials[2],
            new Vector2(291.81f, 74.54f), new Vector2(175, -40), gameObjects);
    }

    /// <summary>Create a UI button</summary>
    /// <param name="name">The name of the button.</param>
    /// <param name="material">The material for the button.</param>
    /// <param name="size">The Vector2 width and height of the button.</param>
    /// <param name="position">The Vector2 x and y anchor position of the button.</param>
    /// <param name="gameObjects">The list of all game objects, used to find the canvas
    /// to create the buttons on and to add the button to the list of game objects.</param>
    /// <returns>Returns the button that is created from the parameters.</returns>
    public Button UiButton(String name, Material material, 
        Vector2 size, Vector2 position, List<GameObject> gameObjects)
    {
        GameObject gameObject = new GameObject(name + " Button");

        // Set parent to the canvas.
        gameObject.transform.SetParent(gameObjects[0].transform, false);

        // Load the button sprite sheet that appears in the Resources/Demo folder.
        Sprite[] spriteSheetSprites = Resources.LoadAll<Sprite>("Demo/" + name);

        // Add image component.
        Image image = gameObject.AddComponent<Image>();
        image.sprite = spriteSheetSprites[0];
        image.rectTransform.sizeDelta = size;
        image.rectTransform.anchoredPosition = position;

        // Add button component.
        Button button = gameObject.AddComponent<Button>();

        // TODO: Make the button do what it's supposed to do.
        UnityAction action = new UnityAction(HandleClick);
        UnityEventTools.AddPersistentListener(button.onClick, action);

        // Add the button game object to the list.
        gameObjects.Add(gameObject);

        Debug.Log("You have created the button.");

        return button;
    }

    public void HandleClick()
    {
        Debug.Log("You have clicked the button.");
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChangeColor : MonoBehaviour
{
   [SerializeField] private GameObject entity;
   private VisualElement root;

   private void Awake()
   {
      root = GetComponent<UIDocument>().rootVisualElement;
      SetupButtonEvents();
   }

   public void SetupButtonEvents() // Call this when menu is shown
   {
      var redBtn = root.Q<Button>("RedButton");
      var greenBtn = root.Q<Button>("GreenButton");
      var blueBtn = root.Q<Button>("BlueButton");

      Debug.Log($"Buttons: Red={redBtn != null}, Green={greenBtn != null}, Blue={blueBtn != null}");
      
      // Clear previous handlers to avoid duplicates
      if (redBtn != null) redBtn.clicked -= () => SetColor(Color.red); // Unsubscribe first
      if (greenBtn != null) greenBtn.clicked -= () => SetColor(Color.green);
      if (blueBtn != null) blueBtn.clicked -= () => SetColor(Color.blue);

      // Subscribe new handlers
      if (redBtn != null) redBtn.clicked += () => SetColor(Color.red);
      if (greenBtn != null) greenBtn.clicked += () => SetColor(Color.green);
      if (blueBtn != null) blueBtn.clicked += () =>
      {
         Debug.Log("Blue button clicked!");
         SetColor(Color.blue);
      };
   }

   private void SetColor(Color c)
   {
      Debug.Log($"Changing color to: {c}");
      if (entity != null) entity.GetComponent<Renderer>().material.color = c;
      else Debug.LogError("Entity is null!");
   }
}

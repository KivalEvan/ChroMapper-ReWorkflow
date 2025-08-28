using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ReWorkflow.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ReWorkflow;

[Plugin("ReWorkflow")]
public class Main
{
   private readonly Dictionary<EditingMode, GameObject[]> _gameObjects = new();
   private EditingMode _editingMode;

   private bool _hasLoaded = true;
   private UI.UI _ui;

   [Init]
   private void Init()
   {
      SceneManager.sceneLoaded += SceneLoaded;
      var harmony = new Harmony("kvl.cm.reworkflow");
      harmony.PatchAll();
      _ui = new UI.UI(this);
   }

   [Exit]
   private void Exit()
   {
   }

   private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
   {
      if (arg0.buildIndex != 3)
      {
         if (!_hasLoaded) return;
         _gameObjects.Clear();
         _hasLoaded = false;
         _editingMode = EditingMode.Gameplay;
         return;
      }

      // im committing programming war crime with this one
      _gameObjects[EditingMode.Gameplay] =
      [
         Object.FindObjectOfType<NoteGridContainer>().GridTransform.gameObject,
         // Object.FindObjectOfType<NJSEventGridContainer>().GridTransform.parent.gameObject,
         Object.FindObjectOfType<GridOrderController>().transform.Find("Note Grid").gameObject
      ];

      foreach (var go in _gameObjects[EditingMode.Gameplay])
      {
         var gridChild = go.GetComponent<GridChild>();
         if (gridChild == null) continue;
         if (Object.FindObjectOfType<NoteGridContainer>().GridTransform.gameObject == go)
            // ok why do they just offset itself incorrectly
            gridChild.LocalOffset = new Vector3(0, -0.65f, 0);
         GridOrderController.DeregisterChild(gridChild);
      }

      _gameObjects[EditingMode.BasicEvent] =
      [
         Object.FindObjectOfType<EventGridContainer>().GridTransform.parent.gameObject,
         Object.FindObjectOfType<GridOrderController>().transform.Find("Event Grid").gameObject,
         Object.FindObjectOfType<GridOrderController>().transform.Find("Event Label").gameObject
      ];

      foreach (var go in _gameObjects[EditingMode.BasicEvent])
      {
         var gridChild = go.GetComponent<GridChild>();
         if (gridChild == null) continue;
         GridOrderController.DeregisterChild(gridChild);
         gridChild.Order = 0;

         var patchBehaviour = gridChild.gameObject.AddComponent<EventGridPatchBehaviour>();
         patchBehaviour.RefChild = _gameObjects[EditingMode.BasicEvent][1].GetComponent<GridChild>();
      }

      UpdateView();
   }

   public void SwitchToggle()
   {
      _editingMode = _editingMode switch
      {
         EditingMode.Gameplay => EditingMode.BasicEvent,
         EditingMode.BasicEvent => EditingMode.Gameplay,
         _ => _editingMode
      };

      UpdateView();
   }

   public void UpdateView()
   {
      foreach (var mode in Enum.GetValues(typeof(EditingMode)).Cast<EditingMode>())
      {
         var toggle = _editingMode == mode;
         foreach (var go in _gameObjects[mode])
         {
            // go.SetActive(toggle);
            var gridChild = go.GetComponent<GridChild>();
            if (gridChild == null) continue;
            if (toggle)
            {
               GridOrderController.RegisterChild(gridChild);
            }
            else
            {
               GridOrderController.DeregisterChild(gridChild);
               // apparently deactive is bad, send them to space instead
               gridChild.transform.localPosition = new Vector3(0, 69420, 69420);
            }
         }
      }

      GridOrderController.MarkDirty();
   }

   private enum EditingMode
   {
      Gameplay,
      BasicEvent
   }
}
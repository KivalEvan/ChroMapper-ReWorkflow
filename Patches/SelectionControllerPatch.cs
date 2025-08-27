using System;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Base;
using Beatmap.Helper;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace ReWorkflow.Patches;

[HarmonyPatch(typeof(SelectionController), nameof(SelectionController.Copy))]
public static class CopyEventPatch
{
   public static HashSet<BaseObject> original;

   public static void Postfix()
   {
      original = SelectionController.CopiedObjects;
   }
}

[HarmonyPatch(typeof(SelectionController), nameof(SelectionController.Paste))]
public static class PasteEventPatch
{
   public static void Prefix()
   {
      SelectionController.CopiedObjects = CopyEventPatch.original;
      SelectionController.GetObjectTypes(
         SelectionController.CopiedObjects.AsEnumerable(),
         out _,
         out var hasEv,
         out _,
         out _);
      if (!hasEv) return;
      var ep = Object.FindObjectOfType<EventPlacement>();
      var egc = Object.FindAnyObjectByType<EventGridContainer>();
      if (ep.queuedData is not BaseEvent) return;

      var newSet = new HashSet<BaseObject>();
      var expectedType = -1;
      var first = true;
      var isSingleIds = true;
      int[] lightIds = null;
      var minId = int.MaxValue;
      var hasNullId = false;
      foreach (var obj in SelectionController.CopiedObjects)
      {
         if (obj is not BaseEvent) return;
         var ev = (BaseEvent)BeatmapFactory.Clone(obj);
         if (expectedType == -1) expectedType = ev.Type;
         if (ev.Type != expectedType) return;

         ev.Type = ep.queuedData.Type;

         if (first) lightIds = ev.CustomLightID;

         if (ev.CustomLightID != null)
         {
            minId = Math.Min(ev.CustomLightID.Min(), minId);

            if (!first && (lightIds == null || ev.CustomLightID.Length != lightIds.Length)) isSingleIds = false;

            if (!first
               && lightIds != null
               && ev.CustomLightID.Length == lightIds.Length
               && !lightIds.OrderBy(s => s).SequenceEqual(ev.CustomLightID.OrderBy(s => s)))
               isSingleIds = false;
         }
         else
            hasNullId = true;

         first = false;
         newSet.Add(ev);
      }

      switch (egc.PropagationEditing)
      {
         case EventGridContainer.PropMode.Prop when isSingleIds:
         case EventGridContainer.PropMode.Light when hasNullId && isSingleIds:
         {
            foreach (BaseEvent ev in newSet)
            {
               ev.Type = egc.EventTypeToPropagate;
               ev.CustomLightID = ep.queuedData.CustomLightID;
            }

            break;
         }
         case EventGridContainer.PropMode.Light when !hasNullId:
         {
            foreach (BaseEvent ev in newSet)
            {
               ev.Type = egc.EventTypeToPropagate;
               if (ep.queuedData.CustomLightID == null)
               {
                  ev.CustomLightID = null;
                  continue;
               }

               for (var i = 0; i < ev.customLightID.Length; i++)
                  ev.CustomLightID[i] = ev.CustomLightID[i] - minId + ep.queuedData.CustomLightID[0];
            }

            break;
         }
      }

      SelectionController.CopiedObjects = newSet;
   }
}
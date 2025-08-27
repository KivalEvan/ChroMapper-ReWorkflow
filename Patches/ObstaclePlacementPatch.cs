using System;
using HarmonyLib;
using UnityEngine;

namespace ReWorkflow.Patches;

[HarmonyPatch(typeof(ObstaclePlacement), nameof(ObstaclePlacement.OnPhysicsRaycast))]
public static class ObstacleRaycastPatch
{
   private static int originY;

   public static void Postfix(
      ObstaclePlacement __instance,
      Intersections.IntersectionHit hit,
      Vector3 transformedPoint)
   {
      var vec3 = __instance.ParentTrack.InverseTransformPoint(hit.Point);
      var z = __instance.instantiatedContainer.GetScale().z;
      var target = __instance.instantiatedContainer.Animator.LocalTarget;
      if (ObstaclePlacement.IsPlacing)
      {
         vec3.x = Mathf.Floor(vec3.x + 1f);
         vec3.y = Mathf.Ceil(Math.Min(Math.Max(vec3.y - 0.2f, 0.01f), 5f));
         vec3.z = z;

         __instance.queuedData.Width = Mathf.CeilToInt(vec3.x + 2f) - __instance.originIndex;
         if (__instance.queuedData.Width <= 0)
         {
            __instance.queuedData.PosX = __instance.originIndex + __instance.queuedData.Width - 1;
            __instance.queuedData.Width = 2 - __instance.queuedData.Width;
         }
         else
            __instance.queuedData.PosX = __instance.originIndex;

         __instance.queuedData.Height = Mathf.CeilToInt(vec3.y) - originY;
         if (__instance.queuedData.Height <= 0)
         {
            __instance.queuedData.PosY = originY + __instance.queuedData.Height - 1;
            __instance.queuedData.Height = 2 - __instance.queuedData.Height;
         }
         else
            __instance.queuedData.PosY = originY;

         target.localPosition =
            new Vector3(
               (float)(__instance.queuedData.PosX - 2.0 + __instance.queuedData.Width / 2.0),
               __instance.queuedData.PosY,
               0.0f);
         __instance.instantiatedContainer.SetScale(
            new Vector3(
               __instance.queuedData.Width,
               target.localScale.y,
               vec3.z));
      }
      else
      {
         __instance.queuedData.PosX = (int)Mathf.Floor(vec3.x + 2f);
         __instance.originIndex = __instance.queuedData.PosX;

         __instance.queuedData.PosY = Mathf.Clamp((int)Mathf.Floor(vec3.y - 0.2f), 0, 2);
         originY = __instance.queuedData.PosY;

         __instance.queuedData.Height = 1;
         target.localPosition = new Vector3(__instance.queuedData.PosX - 1.5f, __instance.queuedData.PosY, 0.0f);
         __instance.instantiatedContainer.transform.localPosition = new Vector3(0.0f, 0.1f, transformedPoint.z);
         __instance.instantiatedContainer.SetScale(new Vector3(1f, 1f, 0.0f));
      }

      __instance.precisionPlacement.TogglePrecisionPlacement(true);
      __instance.precisionPlacement.UpdateMousePosition(hit.Point);
      __instance.UsePrecisionPlacement = false;
   }
}
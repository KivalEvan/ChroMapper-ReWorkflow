using Beatmap.Base;
using Beatmap.Containers;
using HarmonyLib;
using UnityEngine;

namespace ReWorkflow.Patches;

public static class EventPlacementPatch
{
   public static int OriginalValue;
}

// i beg u to revamp these whole grid system
[HarmonyPatch(typeof(EventPlacement), nameof(EventPlacement.SetGridSize))]
public static class EventPlacementSetGridSizePatch
{
   public static void Postfix(EventPlacement __instance)
   {
      EventPlacementPatch.OriginalValue = __instance.GridChild.Size;
      __instance.GridChild.Size = Mathf.CeilToInt(EventPlacementPatch.OriginalValue / 2f);
   }
}

[HarmonyPatch(
   typeof(PlacementController<BaseEvent, EventContainer, EventGridContainer>),
   nameof(PlacementController<BaseEvent, EventContainer, EventGridContainer>.PlacementXMax),
   MethodType.Getter)]
public static class PlacementControllerPlacementXMaxPatch
{
   public static void Postfix(
      PlacementController<BaseEvent, EventContainer, EventGridContainer> __instance,
      ref int __result)
   {
      if (__instance is EventPlacement) __result = EventPlacementPatch.OriginalValue;
   }
}
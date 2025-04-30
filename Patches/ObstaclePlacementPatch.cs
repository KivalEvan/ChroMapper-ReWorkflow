using System;
using System.Runtime.CompilerServices;
using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using HarmonyLib;
using UnityEngine;

namespace ReWorkflow;

[HarmonyPatch(typeof(ObstaclePlacement))]
[HarmonyPatch(nameof(ObstaclePlacement.OnPhysicsRaycast))]
public static class ObstacleRaycastPatch
{
    public static void Postfix(ObstaclePlacement __instance, Intersections.IntersectionHit hit,
        Vector3 transformedPoint)
    {
        __instance.Bounds = new Bounds();
        __instance.TestForType<ObstaclePlacement>(hit, ObjectType.Obstacle);
        var vec3 = __instance.ParentTrack.InverseTransformPoint(hit.Point);
        var z = __instance.BpmChangeGridContainer.JsonTimeToSongBpmTime(__instance.RoundedJsonTime) -
                __instance.startSongBpmTime;
        var target = __instance.instantiatedContainer.Animator.LocalTarget;
        if (ObstaclePlacement.IsPlacing)
        {
            if (__instance.UsePrecisionPlacement)
            {
                return;
            }

            vec3 = new Vector3(
                Mathf.Ceil(Math.Min(Math.Max(vec3.x, __instance.Bounds.min.x + 0.01f), __instance.Bounds.max.x)),
                Mathf.Ceil(Math.Min(Math.Max(vec3.y - 0.2f, 0.01f), 5f)),
                __instance.SongBpmTime * EditorScaleController.EditorScale);
            __instance.queuedData.Height = Mathf.CeilToInt(vec3.y) - __instance.queuedData.PosY;

            if (__instance.queuedData.Height <= 0)
                __instance.queuedData.Height = 1;

            target.localPosition =
                new Vector3(
                    (float)(__instance.queuedData.PosX - 2.0 + __instance.queuedData.Width / 2.0),
                    __instance.queuedData.PosY, 0.0f);
            __instance.instantiatedContainer.SetScale(new Vector3(__instance.queuedData.Width,
                target.localScale.y,
                z * EditorScaleController.EditorScale));
        }
        else
        {
            if (__instance.UsePrecisionPlacement)
            {
                return;
            }

            __instance.queuedData.PosX = Mathf.RoundToInt(transformedPoint.x);
            __instance.queuedData.PosY = Mathf.Clamp((int)Mathf.Floor(vec3.y - 0.2f), 0, 2);
            __instance.queuedData.Height = 1;
            target.localPosition = new Vector3(transformedPoint.x - 1.5f, __instance.queuedData.PosY, 0.0f);
            __instance.instantiatedContainer.transform.localPosition = new Vector3(0.0f, 0.1f, transformedPoint.z);
            __instance.instantiatedContainer.SetScale(new Vector3(1f, 1f, 0.0f));
        }
    }
}
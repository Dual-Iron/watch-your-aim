using BepInEx;
using System;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace WatchYourAim;

[BepInPlugin("com.dual.watch-your-aim", "Watch Your Aim", "0.1.0")]
sealed class Plugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.ScavengerAI.IsThrowPathClearFromFriends += ScavengerAI_IsThrowPathClearFromFriends;
    }

    private bool ScavengerAI_IsThrowPathClearFromFriends(On.ScavengerAI.orig_IsThrowPathClearFromFriends orig, ScavengerAI self, Vector2 throwPos, float margin)
    {
        return orig(self, throwPos, margin) && !Obstructed(self, throwPos, margin);
    }

    private bool Obstructed(ScavengerAI self, Vector2 to, float margin)
    {
        Vector2 from = self.scavenger.mainBodyChunk.pos;

        throw new NotImplementedException();
    }
}

using BepInEx;
using MoreSlugcats;
using RWCustom;
using System;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace WatchYourAim;

[BepInPlugin("com.dual.watch-your-aim", "Watch Your Aim", "1.0.2")]
sealed class Plugin : BaseUnityPlugin
{
    // Just a local
    private WeakReference<Creature> target;

    public void OnEnable()
    {
        On.Scavenger.TryThrow_BodyChunk_ViolenceType_Nullable1 += Scavenger_TryThrow_BodyChunk_ViolenceType_Nullable1;
        On.ScavengerAI.IsThrowPathClearFromFriends += ScavengerAI_IsThrowPathClearFromFriends;
    }

    private void Scavenger_TryThrow_BodyChunk_ViolenceType_Nullable1(On.Scavenger.orig_TryThrow_BodyChunk_ViolenceType_Nullable1 orig, Scavenger self, BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType, Vector2? aimPosition)
    {
        if (!self.abstractCreature.world.game.rainWorld.safariMode) {
            target = aimChunk?.owner is Creature c ? new(c) : null;
        }

        orig(self, aimChunk, violenceType, aimPosition);
    }

    private bool ScavengerAI_IsThrowPathClearFromFriends(On.ScavengerAI.orig_IsThrowPathClearFromFriends orig, ScavengerAI self, Vector2 throwPos, float margin)
    {
        if (self.scavenger.abstractCreature.world.game.rainWorld.safariMode) {
            return orig(self, throwPos, margin);
        }

        margin += 20f;

        return orig(self, throwPos, margin) && !Obstructed(self, throwPos, margin);
    }

    private bool Obstructed(ScavengerAI self, Vector2 to, float margin)
    {
        Vector2 from = self.scavenger.mainBodyChunk.pos;

        foreach (Tracker.CreatureRepresentation bystander in self.tracker.creatures) {
            if (!bystander.VisualContact || !self.DontWantToThrowAt(bystander) || bystander.representedCreature.realizedCreature == null) {
                continue;
            }

            Vector2 bystanderPos = bystander.representedCreature.realizedCreature.mainBodyChunk.pos;

            // For Custom.DistanceToLine, see https://www.desmos.com/calculator/gxxkp3wfmn

            bool alongThrowDir = Vector2.Dot(from - bystanderPos, from - to) > 0f;
            bool closeToThrowLine = Mathf.Abs(Custom.DistanceToLine(bystanderPos, from, to)) < 50f + margin;
            bool inFrontOfTarget = Vector2.Distance(from, bystanderPos) < Vector2.Distance(from, to) + margin;

            if (alongThrowDir && closeToThrowLine && inFrontOfTarget) {
                // Definitely crosses paths with target.
                return true;
            }

            if (alongThrowDir && closeToThrowLine && target.TryGetTarget(out Creature critter) && CouldMiss(critter)) {
                // Could pass through the target and hit friend. Vanilla doesn't perform this check, but should.
                return true;
            }
        }
        return false;
    }

    private bool CouldMiss(Creature critter)
    {
        return critter.collisionLayer == 0 || critter.collisionRange <= 0 || critter is Overseer or EggBug or Yeek or Player;
    }
}

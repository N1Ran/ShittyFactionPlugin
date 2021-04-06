using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using Sandbox.Definitions;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.Managers;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Network;
using VRage.Profiler;

namespace ShittyFactionPlugin.Patch
{
    [PatchShim]
    public static class ReputationPatch
    {

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyFactionCollection).GetMethod("AddFactionPlayerReputation",
                    BindingFlags.Public | BindingFlags.Instance))
                .Prefixes.Add(typeof(ReputationPatch).GetMethod(nameof(PlayerRepChange),  BindingFlags.Static | BindingFlags.NonPublic));
        }


        private static bool PlayerRepChange(MyFactionCollection __instance,
            long playerIdentityId,
            long factionId,
            int delta,
            bool propagate = true,
            bool adminChange = false)
        {
            if (!PluginCore.Instance.Config.Enable || !PluginCore.Instance.Config.KeepDefaultEnemy) return true;
            var enemyFactions = new List<MyFactionDefinition>(MyDefinitionManager.Static.GetDefaultFactions().Where(x=>x.DefaultRelation == MyRelationsBetweenFactions.Enemies));
            var faction = MySession.Static.Factions.TryGetFactionById(factionId);
            if (faction == null ||
                !enemyFactions.Contains(MyDefinitionManager.Static.TryGetFactionDefinition(faction.Tag)) || delta < 0)
            {
                return true;
            }

            return false;
        }


    }
}
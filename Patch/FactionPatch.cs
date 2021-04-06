using System;
using System.Reflection;
using NLog.Fluent;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.Managers;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI;
using VRage.Network;

namespace ShittyFactionPlugin.Patch
{
    [PatchShim]
    public static class FactionPatch
    {
        private static readonly MethodInfo ChangeFactionSuccess =
            typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess",
                BindingFlags.NonPublic | BindingFlags.Static);

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess",
                    BindingFlags.Static | BindingFlags.NonPublic)).Prefixes
                .Add(typeof(FactionPatch).GetMethod(nameof(ChangeRequest), BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static bool ChangeRequest(MyFactionStateChange action,
            long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (!PluginCore.Instance.Config.Enable) return true;
            var remoteUserId = MySession.Static.Players.TryGetSteamId(playerId);

            if (remoteUserId == 0) return true;

            if (MySession.Static.Factions.IsNpcFaction(toFactionId) ||
                MySession.Static.Factions.IsNpcFaction(fromFactionId) || MySession.Static.Factions.TryGetFactionById(toFactionId)?.Tag == "SPID" || MySession.Static.Factions.TryGetFactionById(fromFactionId)?.Tag == "SPID") return true;


            if (MySession.Static.Players.IdentityIsNpc(playerId)) return true;

            if ((action == MyFactionStateChange.FactionMemberSendJoin || action == MyFactionStateChange.FactionMemberAcceptJoin) && PluginCore.Instance.Config.FactionSize > 0)
            {

                if (MySession.Static.Factions.TryGetFactionById(toFactionId)?.Members.Count >= PluginCore.Instance.Config.FactionSize)
                {
                    try
                    {
                        NetworkManager.RaiseStaticEvent(ChangeFactionSuccess,MyFactionStateChange.FactionMemberCancelJoin,toFactionId,fromFactionId,playerId,senderId, new EndpointId(remoteUserId), null);
                        NetworkManager.RaiseStaticEvent(ChangeFactionSuccess,MyFactionStateChange.FactionMemberKick,toFactionId,fromFactionId,playerId,senderId, new EndpointId(remoteUserId), null);
                    }
                    catch (Exception e)
                    {
                        PluginCore.Log.Warn(e,"Join cancel failed to send to player");
                        throw;
                    }
                    Utility.SendDenyMessage(remoteUserId);
                    return false;
                }
            }

            return true;
        }
    }
}
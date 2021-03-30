using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch;
using Torch.API.Managers;
using Torch.Managers.ChatManager;
using Torch.Utils;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Network;
using VRageMath;

namespace ShittyFactionPlugin
{
    public static class Utility
    {
        public static bool IsFactionFull(long id)
        {
            var faction = MySession.Static.Factions.TryGetFactionById(id);
            return faction.Members.Count >= PluginCore.Instance.Config.FactionSize;
        }

        public static void BalanceFaction()
        {
            
        }

        public static void AssignPlayer(long playerId)
        {
            if (!PluginCore.Instance.Config.Enable || !PluginCore.Instance.Config.AssignFaction || playerId == 0) return;

            var factionDictionary = MySession.Static.Factions.Factions;
            if (factionDictionary == null || factionDictionary.Count == 0) return;
            IMyFaction potentialFaction = null;
            if (MySession.Static.Factions.GetPlayerFaction(playerId) != null) return;
            foreach (var (factionId, faction) in factionDictionary)
            {
                if (faction.IsEveryoneNpc() || MySession.Static.Players.IdentityIsNpc(faction.FounderId)) continue;
                
                if (faction.Members.Count >= PluginCore.Instance.Config.FactionSize) continue;

                potentialFaction = faction;
            }

            if (potentialFaction == null)
            {
                CreatePlayerFaction(playerId);
                return;
            }
            MyVisualScriptLogicProvider.SetPlayersFaction(playerId, potentialFaction.Tag);
        }

        private static void CreatePlayerFaction(long playerId)
        {
            var tag = RandomString(3);
            if (MySession.Static.Factions.FactionTagExists(tag))
            {
                CreatePlayerFaction(playerId);
                return;
            }
            MySession.Static.Factions.CreateFaction(playerId,tag,"Auto Assigned Shitty Faction",null,null,MyFactionTypes.PlayerMade);
        }

        public static void SendDenyMessage(ulong playerSteamId)
        {
            var player = MySession.Static.Players.TryGetPlayerBySteamId(playerSteamId);
            if (playerSteamId == 0 && !MySession.Static.Players.IsPlayerOnline(player.Identity.IdentityId)) return;
            PluginCore.Instance.Torch.CurrentSession.Managers.GetManager<IChatManagerServer>()?
                .SendMessageAsOther(PluginCore.Instance.Torch.Config.ChatName, PluginCore.Instance.Config.JoinDenyMessage, Color.Red, playerSteamId);
            SendFailSound(playerSteamId);
            ValidationFailed();

        }

        [ReflectedStaticMethod(Type = typeof(MyCubeBuilder), Name = "SpawnGridReply", OverrideTypes = new []{typeof(bool), typeof(ulong)})]
        private static Action<bool, ulong> _spawnGridReply;

        private static void SendFailSound(ulong target)
        {
            if (target == 0) return;
            _spawnGridReply(false, target);
        }

        private static void ValidationFailed(ulong id = 0)
        {
            var user = id > 0 ? id : MyEventContext.Current.Sender.Value;
            if (user == 0) return;
            ((MyMultiplayerServerBase)MyMultiplayer.Static).ValidationFailed(user);
        }

        private static readonly Random _random = new Random();

        private static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int letterOffset = 26;
            for (int i = 0; i < size; i++)
            {
                var @char = (char) _random.Next(offset, offset + letterOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }


    }
}
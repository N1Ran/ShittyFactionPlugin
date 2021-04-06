using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NLog;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Session;
using Torch.Utils;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace ShittyFactionPlugin
{
    public class PluginCore : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetLogger("ShittyFactionPlugin");

        public static PluginCore Instance { get; private set; }
        private Control _control;

        public UserControl GetControl() => _control ?? (_control = new Control(this));
        private Persistent<Config> _config;
        private TorchSessionManager _sessionManager;
        private IMultiplayerManagerBase _multiBase;
        private List<ulong> _connecting;
        public Config Config => _config?.Data;

        public void Save() => _config.Save();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            Instance = this;

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();


            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;

            LoadConfig();
        }

        public void LoadConfig()
        {
            var configFile = Path.Combine(StoragePath, "ShittyFactionPlugin.cfg");

            try 
            {

                _config = Persistent<Config>.Load(configFile);

            }
            catch (Exception e) 
            {
                Log.Warn(e);
            }

            if (_config?.Data != null) return;

            Log.Info("Created Default Config, because none was found!");

            _config = new Persistent<Config>(configFile, new Config());
            _config.Save();
        }

        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            switch (newState)
            {
                case TorchSessionState.Loading:
                    break;
                case TorchSessionState.Loaded:
                    Load();
                    break;
                case TorchSessionState.Unloading:
                    break;
                case TorchSessionState.Unloaded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void Load()
        {
            _multiBase = Instance.Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>();
            if (_multiBase != null)
            {
                _multiBase.PlayerJoined += MultiBaseOnPlayerJoined;
                _multiBase.PlayerLeft += MultiBaseOnPlayerLeft;
            }
            else
            {
                Log.Warn("Multibase is Null");
            }

            MyEntities.OnEntityAdd += MyEntitiesOnOnEntityAdd;


            if (Config.Enable && Config.KeepDefaultEnemy)
                Log.Warn($"{CheckRep()} reps altered");

        }

        private void MultiBaseOnPlayerLeft(IPlayer obj)
        {
            _connecting.Remove(obj.SteamId);
        }

        private void MultiBaseOnPlayerJoined(IPlayer obj)
        {
            _connecting.Add(obj.SteamId);
        }

        private void MyEntitiesOnOnEntityAdd(MyEntity entity)
        {
            if (!Config.AssignFaction || !Config.Enable) return;
            if (!(entity is MyCharacter character) || character.IsBot || !character.IsPlayer) return;
            ulong steamId = character.ControlSteamId;
            if (steamId == 0) return;
            if (!_connecting.Contains(steamId)) return;
            _connecting.Remove(steamId);
            var playerIdentity = character.GetIdentity();
            if (MySession.Static.Players.IdentityIsNpc(playerIdentity.IdentityId)) return;
            if (MySession.Static.Factions.TryGetPlayerFaction(playerIdentity.IdentityId) != null) return;
            Utility.AssignPlayer(playerIdentity.IdentityId);
        }


        private int CheckRep()
        {
            var repChanged = 0;

            var playerList = new List<MyIdentity>(MySession.Static.Players.GetAllIdentities());

            var enemyFactions = new List<MyFactionDefinition>(MyDefinitionManager.Static.GetDefaultFactions().Where(x=>x.DefaultRelation == MyRelationsBetweenFactions.Enemies || x.DefaultRelationToPlayers == MyRelationsBetweenFactions.Enemies));

            foreach (var id in playerList)
            {
                if (MySession.Static.Players.IdentityIsNpc(id.IdentityId) || string.IsNullOrEmpty(id.DisplayName))continue;

                foreach (var facDef in enemyFactions)
                {
                    var faction = MySession.Static.Factions.TryGetFactionByTag(facDef.Tag);
                    if (faction == null || MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(id.IdentityId, faction.FactionId) <= -1500) continue;

                    repChanged++;
                    MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, faction.FactionId, -1500);
                }
            }

            return repChanged;
        }

    }
}

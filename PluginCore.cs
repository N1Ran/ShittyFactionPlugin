using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Session;
using Torch.Utils;
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
        public Config Config => _config?.Data;

        public void Save() => _config.Save();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            Instance = this;

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();

            var configFile = Path.Combine(StoragePath, "ShittyFactionPlugin.cfg");

            if (_sessionManager != null)
                _sessionManager.SessionStateChanged += SessionChanged;

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
           MyEntities.OnEntityAdd += MyEntitiesOnOnEntityAdd;

        }

        private void MyEntitiesOnOnEntityAdd(MyEntity entity)
        {
            if (!Config.AssignFaction || !Config.Enable) return;
            if (!(entity is MyCharacter character) || character.IsBot || string.IsNullOrEmpty(character.DisplayName)) return;
            var playerIdentity = character.GetIdentity();
            if (MySession.Static.Players.IdentityIsNpc(playerIdentity.IdentityId)) return;
            if (MySession.Static.Factions.TryGetPlayerFaction(playerIdentity.IdentityId) != null) return;
            Utility.AssignPlayer(playerIdentity.IdentityId);
        }


    }
}

using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace ShittyFactionPlugin
{
    [Category("sfp")]
    public partial class Commands:CommandModule
    {
        [Command("enable", "enable/disable plugin")]
        [Permission(MyPromoteLevel.Admin)]
        public void Enable(bool enable = true)
        {
            PluginCore.Instance.Config.Enable = enable;
            Context.Respond(enable ? "Plugin Enabled" : "Plugin Disabled");
        }

        [Command("reload", "Reloads current config and apply changes")]
        [Permission(MyPromoteLevel.Admin)]
        public void Reload()
        {
            PluginCore.Instance.LoadConfig();
        }

    }
}
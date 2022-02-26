using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Reflection;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Utility.Signatures;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState.Objects.Types;

namespace SamplePlugin
{
    public sealed unsafe class InterruptCastOnDeath : IDalamudPlugin
    {
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static CommandManager CommandManager { get; private set; }
        [PluginService] public static Framework Framework { get; private set; }
        [PluginService] public static Condition Condition { get; private set; }
        [PluginService] public static ClientState ClientState { get; private set; }
        [PluginService] public static ObjectTable ObjectTable { get; private set; } 
        [PluginService] public static SigScanner SigScanner { get; private set; }
        [PluginService] public static TargetManager TargetManager { get; private set; }
        [PluginService] public static GameGui GameGui { get; private set; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public string Name => "Interrupt Cast On Death";

        private const string commandName = "/icod";

        private bool Casting { get; set; } = false;

        private delegate void CastInterruptDelegate();

        [Signature("48 83 EC 38 33 D2 C7 44 24 ?? ?? ?? ?? ?? 45 33 C9")]
        private CastInterruptDelegate? CastInterrupt { get; init; }
        
        public InterruptCastOnDeath()
        {
            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(PluginInterface);

            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.PluginUi = new PluginUI(Configuration);

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "/icod to open menu."
            });
            SignatureHelper.Initialise(this);
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawUI;
            Framework.Update += Framework_Update;
            Condition.ConditionChange += Condition_ConditionChange;
        }

        private void Condition_ConditionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.Casting)
            {
                Casting = value;
            }
        }

        private void Framework_Update(Framework framework)
        {     
            if (Configuration.Interrupt && Casting)
            {
                if (((BattleChara)ObjectTable.SearchById(ClientState.LocalPlayer.CastTargetObjectId)).CurrentHp == 0)
                {
                    CastInterrupt();
                }
            }
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            CommandManager.RemoveHandler(commandName);
            Framework.Update -= Framework_Update;
            Condition.ConditionChange -= Condition_ConditionChange;
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawUI;
        }

        private void OnCommand(string command, string args)
        {
            this.PluginUi.Visible = true;
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }
    }
}

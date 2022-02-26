using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace SamplePlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool interrupt = false;

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
            interrupt = configuration.Interrupt;
        }

        public void Dispose()
        {
            
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            if (ImGui.Begin("Interrupt Cast On Death", ref this.visible, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.Checkbox("Interrupt on Death", ref interrupt))
                {
                    configuration.Interrupt = interrupt;                    
                    configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}

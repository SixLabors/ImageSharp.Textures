using ImGuiNET;
using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Phoenix.Import.Application
{
    public class Program
    {
        private static UiManager _uiManager;
        private static Sdl2Window _window;
        private static DateTime _prevUpdateTime;

        public static void Main(string[] args)
        {
            _uiManager = new UiManager();

            _window = VeldridStartup.CreateWindow(new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, $"ImageSharp.Textures.InteractiveTest"));
            ApplicationManager.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, GraphicsBackend.OpenGL);      

            _window.Resized += _window_Resized;

            ApplicationManager.CommandList = ApplicationManager.GraphicsDevice.ResourceFactory.CreateCommandList();
            ApplicationManager.Controller = new ImGuiRenderer(ApplicationManager.GraphicsDevice, ApplicationManager.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

            ImGui.StyleColorsDark();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists)
                {
                    break;
                }

                DateTime curUpdateTime = DateTime.Now;
                if (_prevUpdateTime.Ticks == 0)
                {
                    _prevUpdateTime = curUpdateTime;
                }
                float dt = (float)(curUpdateTime - _prevUpdateTime).TotalSeconds;
                if (dt <= 0)
                {
                    dt = float.Epsilon;
                }
                _prevUpdateTime = curUpdateTime;

                ApplicationManager.Controller.Update(dt, snapshot); 

                SubmitUi();

                ApplicationManager.CommandList.Begin();
                ApplicationManager.CommandList.SetFramebuffer(ApplicationManager.GraphicsDevice.MainSwapchain.Framebuffer);
                ApplicationManager.CommandList.ClearColorTarget(0, new RgbaFloat(0.5f, 0.5f, 0.5f, 1f));
                try
                {
                    ApplicationManager.Controller.Render(ApplicationManager.GraphicsDevice, ApplicationManager.CommandList);
                }
                catch (Exception)
                {
                    // do nothing
                }
                ApplicationManager.CommandList.End();
                ApplicationManager.GraphicsDevice.SubmitCommands(ApplicationManager.CommandList);
                ApplicationManager.GraphicsDevice.SwapBuffers(ApplicationManager.GraphicsDevice.MainSwapchain);
            }

            ApplicationManager.GraphicsDevice.WaitForIdle();
            ApplicationManager.Controller.Dispose();
            ApplicationManager.CommandList.Dispose();
            ApplicationManager.GraphicsDevice.Dispose();
        }

        static void _window_Resized()
        {
            ApplicationManager.GraphicsDevice.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            ApplicationManager.Controller.WindowResized(_window.Width, _window.Height);
        }

        private static void SubmitUi()
        {
            _uiManager.Render(_window.Width, _window.Height);
        }
    }
}

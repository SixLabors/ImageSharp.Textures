// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Phoenix.Import.Application
{
    public class Program
    {
        private static UiManager uiManager;
        private static Sdl2Window window;
        private static DateTime prevUpdateTime;

        public static void Main()
        {
            uiManager = new UiManager();

            window = VeldridStartup.CreateWindow(new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, $"ImageSharp.Textures.InteractiveTest"));
            ApplicationManager.GraphicsDevice = VeldridStartup.CreateGraphicsDevice(window, GraphicsBackend.OpenGL);

            window.Resized += Window_Resized;

            ApplicationManager.CommandList = ApplicationManager.GraphicsDevice.ResourceFactory.CreateCommandList();
            ApplicationManager.Controller = new ImGuiRenderer(ApplicationManager.GraphicsDevice, ApplicationManager.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);

            ImGui.StyleColorsDark();

            // Main application loop
            while (window.Exists)
            {
                InputSnapshot snapshot = window.PumpEvents();
                if (!window.Exists)
                {
                    break;
                }

                DateTime curUpdateTime = DateTime.Now;
                if (prevUpdateTime.Ticks == 0)
                {
                    prevUpdateTime = curUpdateTime;
                }

                float dt = (float)(curUpdateTime - prevUpdateTime).TotalSeconds;
                if (dt <= 0)
                {
                    dt = float.Epsilon;
                }

                prevUpdateTime = curUpdateTime;

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
                    // do nothing.
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

        static void Window_Resized()
        {
            ApplicationManager.GraphicsDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
            ApplicationManager.Controller.WindowResized(window.Width, window.Height);
        }

        private static void SubmitUi()
        {
            uiManager.Render(window.Width, window.Height);
        }
    }
}

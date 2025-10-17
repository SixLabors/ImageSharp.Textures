// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace SixLabors.ImageSharp.Textures.InteractiveTest
{
    public static class ApplicationManager
    {
        public static CommandList CommandList { get; set; }

        public static GraphicsDevice GraphicsDevice { get; set; }

        public static ImGuiRenderer Controller { get; set; }

        private static readonly object LockObject = new();

        public static unsafe IntPtr Create(Image<Rgba32> image)
        {
            lock (LockObject)
            {
                Veldrid.Texture texture = GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)image.Width, (uint)image.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                bool gotPixelMemory = image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelsMemory);
                if (gotPixelMemory)
                {
                    System.Buffers.MemoryHandle pin = pixelsMemory.Pin();
                    GraphicsDevice.UpdateTexture(texture, (IntPtr)pin.Pointer, (uint)(4 * image.Width * image.Height), 0, 0, 0, (uint)image.Width, (uint)image.Height, 1, 0, 0);
                }
                else
                {
                    throw new Exception("DangerousTryGetSinglePixelMemory failed!");
                }

                return Controller.GetOrCreateImGuiBinding(GraphicsDevice.ResourceFactory, texture);
            }
        }

        public static void ClearImageCache() => Controller.ClearCachedImageResources();

        private static Dictionary<string, object> datastore;

        public static Dictionary<string, object> DataStore => datastore ?? (datastore = new Dictionary<string, object>());
    }
}

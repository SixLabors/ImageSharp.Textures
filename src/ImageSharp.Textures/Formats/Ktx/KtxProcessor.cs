// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;
using System.IO;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.TextureFormats;
using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Decodes ktx textures.
    /// </summary>
    internal class KtxProcessor
    {
        /// <summary>
        /// A scratch buffer to reduce allocations.
        /// </summary>
        private readonly byte[] buffer = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="KtxProcessor" /> class.
        /// </summary>
        /// <param name="ktxHeader">The KTX header.</param>
        public KtxProcessor(KtxHeader ktxHeader) => this.KtxHeader = ktxHeader;

        /// <summary>
        /// Gets the KTX header.
        /// </summary>
        public KtxHeader KtxHeader { get; }

        /// <summary>
        /// Decodes the mipmaps of a KTX textures.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        public MipMap[] DecodeMipMaps(Stream stream, int width, int height, uint count)
        {
            if (this.KtxHeader.GlTypeSize is 0 or 1)
            {
                switch (this.KtxHeader.GlFormat)
                {
                    case GlPixelFormat.Red:
                        return this.AllocateMipMaps<L8>(stream, width, height, count);
                    case GlPixelFormat.Rg:
                    case GlPixelFormat.RgInteger:
                        return this.AllocateMipMaps<Rg16>(stream, width, height, count);
                    case GlPixelFormat.Rgb:
                        return this.AllocateMipMaps<Rgb24>(stream, width, height, count);
                    case GlPixelFormat.Rgba:
                        return this.AllocateMipMaps<Rgba32>(stream, width, height, count);
                    case GlPixelFormat.Bgr:
                        return this.AllocateMipMaps<Bgr24>(stream, width, height, count);
                    case GlPixelFormat.Bgra:
                        return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
                    case GlPixelFormat.LuminanceAlpha:
                        return this.AllocateMipMaps<La16>(stream, width, height, count);
                    case GlPixelFormat.Luminance:
                        return this.AllocateMipMaps<L8>(stream, width, height, count);
                    case GlPixelFormat.Alpha:
                        return this.AllocateMipMaps<A8>(stream, width, height, count);
                    case GlPixelFormat.Compressed:
                        switch (this.KtxHeader.GlInternalFormat)
                        {
                            case GlInternalPixelFormat.RgbaDxt1:
                            case GlInternalPixelFormat.RgbDxt1:
                                return this.AllocateMipMaps<Dxt1>(stream, width, height, count);
                            case GlInternalPixelFormat.RgbaDxt3:
                                return this.AllocateMipMaps<Dxt3>(stream, width, height, count);
                            case GlInternalPixelFormat.RgbaDxt5:
                                return this.AllocateMipMaps<Dxt5>(stream, width, height, count);
                            case GlInternalPixelFormat.RedRgtc1:
                                return this.AllocateMipMaps<Bc4>(stream, width, height, count);
                            case GlInternalPixelFormat.SignedRedRgtc1:
                                return this.AllocateMipMaps<Bc4s>(stream, width, height, count);
                            case GlInternalPixelFormat.RedGreenRgtc2:
                                return this.AllocateMipMaps<Bc5>(stream, width, height, count);
                            case GlInternalPixelFormat.SignedRedGreenRgtc2:
                                return this.AllocateMipMaps<Bc5s>(stream, width, height, count);
                            case GlInternalPixelFormat.Etc1Rgb8Oes:
                                return this.AllocateMipMaps<Etc1>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgb8Etc2:
                                return this.AllocateMipMaps<Etc2>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc4x4Khr:
                                return this.AllocateMipMaps<RgbaAstc4X4>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc5x4Khr:
                                return this.AllocateMipMaps<RgbaAstc5X4>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc5x5Khr:
                                return this.AllocateMipMaps<RgbaAstc5X5>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc6x5Khr:
                                return this.AllocateMipMaps<RgbaAstc6X5>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc6x6Khr:
                                return this.AllocateMipMaps<RgbaAstc6X6>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc8x5Khr:
                                return this.AllocateMipMaps<RgbaAstc8X5>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc8x6Khr:
                                return this.AllocateMipMaps<RgbaAstc8X6>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc8x8Khr:
                                return this.AllocateMipMaps<RgbaAstc8X8>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc10x5Khr:
                                return this.AllocateMipMaps<RgbaAstc10X5>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc10x6Khr:
                                return this.AllocateMipMaps<RgbaAstc10X6>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc10x8Khr:
                                return this.AllocateMipMaps<RgbaAstc10X8>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc10x10Khr:
                                return this.AllocateMipMaps<RgbaAstc10X10>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc12x10Khr:
                                return this.AllocateMipMaps<RgbaAstc12X10>(stream, width, height, count);
                            case GlInternalPixelFormat.CompressedRgbaAstc12x12Khr:
                                return this.AllocateMipMaps<RgbaAstc12X12>(stream, width, height, count);
                        }

                        break;
                }
            }

            if (this.KtxHeader.GlTypeSize is 2 or 4)
            {
                // TODO: endianess is not respected here. Use stream reader which respects endianess.
                switch (this.KtxHeader.GlInternalFormat)
                {
                    case GlInternalPixelFormat.Rgb5A1:
                        return this.AllocateMipMaps<Rgba5551>(stream, width, height, count);
                    case GlInternalPixelFormat.Rgb10A2:
                        return this.AllocateMipMaps<Rgba1010102>(stream, width, height, count);
                    case GlInternalPixelFormat.Rgb16:
                        return this.AllocateMipMaps<Rgb48>(stream, width, height, count);
                    case GlInternalPixelFormat.Rgba16:
                        return this.AllocateMipMaps<Rgba64>(stream, width, height, count);
                    case GlInternalPixelFormat.Rgba32UnsignedInt:
                        return this.AllocateMipMaps<Rgba128>(stream, width, height, count);
                }
            }

            throw new NotSupportedException("The pixel format is not supported");
        }

        /// <summary>
        /// Decodes the a KTX cube map texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of a texture face.</param>
        /// <param name="height">The height of a texture face.</param>
        /// <returns>A decoded cubemap texture.</returns>
        /// <exception cref="NotSupportedException">The pixel format is not supported</exception>
        public CubemapTexture DecodeCubeMap(Stream stream, int width, int height)
        {
            switch (this.KtxHeader.GlFormat)
            {
                case GlPixelFormat.Red:
                    return this.AllocateCubeMap<L8>(stream, width, height);
                case GlPixelFormat.Rg:
                case GlPixelFormat.RgInteger:
                    return this.AllocateCubeMap<Rg16>(stream, width, height);
                case GlPixelFormat.Rgb:
                    return this.AllocateCubeMap<Rgb24>(stream, width, height);
                case GlPixelFormat.Rgba:
                    return this.AllocateCubeMap<Rgba32>(stream, width, height);
                case GlPixelFormat.Bgr:
                    return this.AllocateCubeMap<Bgr24>(stream, width, height);
                case GlPixelFormat.Bgra:
                    return this.AllocateCubeMap<Bgr32>(stream, width, height);
                case GlPixelFormat.LuminanceAlpha:
                    return this.AllocateCubeMap<La16>(stream, width, height);
                case GlPixelFormat.Luminance:
                    return this.AllocateCubeMap<L8>(stream, width, height);
                case GlPixelFormat.Alpha:
                    return this.AllocateCubeMap<A8>(stream, width, height);
                case GlPixelFormat.Compressed:
                    switch (this.KtxHeader.GlInternalFormat)
                    {
                        case GlInternalPixelFormat.RgbaDxt1:
                        case GlInternalPixelFormat.RgbDxt1:
                            return this.AllocateCubeMap<Dxt1>(stream, width, height);
                        case GlInternalPixelFormat.RgbaDxt3:
                            return this.AllocateCubeMap<Dxt3>(stream, width, height);
                        case GlInternalPixelFormat.RgbaDxt5:
                            return this.AllocateCubeMap<Dxt5>(stream, width, height);
                        case GlInternalPixelFormat.RedRgtc1:
                            return this.AllocateCubeMap<Bc4>(stream, width, height);
                        case GlInternalPixelFormat.SignedRedRgtc1:
                            return this.AllocateCubeMap<Bc4s>(stream, width, height);
                        case GlInternalPixelFormat.RedGreenRgtc2:
                            return this.AllocateCubeMap<Bc5>(stream, width, height);
                        case GlInternalPixelFormat.SignedRedGreenRgtc2:
                            return this.AllocateCubeMap<Bc5s>(stream, width, height);
                        case GlInternalPixelFormat.Etc1Rgb8Oes:
                            return this.AllocateCubeMap<Etc1>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgb8Etc2:
                            return this.AllocateCubeMap<Etc2>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc4x4Khr:
                            return this.AllocateCubeMap<RgbaAstc4X4>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc5x4Khr:
                            return this.AllocateCubeMap<RgbaAstc5X4>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc5x5Khr:
                            return this.AllocateCubeMap<RgbaAstc5X5>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc6x5Khr:
                            return this.AllocateCubeMap<RgbaAstc6X5>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc6x6Khr:
                            return this.AllocateCubeMap<RgbaAstc6X6>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc8x5Khr:
                            return this.AllocateCubeMap<RgbaAstc8X5>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc8x6Khr:
                            return this.AllocateCubeMap<RgbaAstc8X6>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc8x8Khr:
                            return this.AllocateCubeMap<RgbaAstc8X8>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc10x5Khr:
                            return this.AllocateCubeMap<RgbaAstc10X5>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc10x6Khr:
                            return this.AllocateCubeMap<RgbaAstc10X6>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc10x8Khr:
                            return this.AllocateCubeMap<RgbaAstc10X8>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc10x10Khr:
                            return this.AllocateCubeMap<RgbaAstc10X10>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc12x10Khr:
                            return this.AllocateCubeMap<RgbaAstc12X10>(stream, width, height);
                        case GlInternalPixelFormat.CompressedRgbaAstc12x12Khr:
                            return this.AllocateCubeMap<RgbaAstc12X12>(stream, width, height);
                    }

                    break;
            }

            if (this.KtxHeader.GlTypeSize is 2 or 4)
            {
                // TODO: endianess is not respected here. Use stream reader which respects endianess.
                switch (this.KtxHeader.GlInternalFormat)
                {
                    case GlInternalPixelFormat.Rgb5A1:
                        return this.AllocateCubeMap<Rgba5551>(stream, width, height);
                    case GlInternalPixelFormat.Rgb10A2:
                        return this.AllocateCubeMap<Rgba1010102>(stream, width, height);
                    case GlInternalPixelFormat.Rgb16:
                        return this.AllocateCubeMap<Rgb48>(stream, width, height);
                    case GlInternalPixelFormat.Rgba16:
                        return this.AllocateCubeMap<Rgba64>(stream, width, height);
                    case GlInternalPixelFormat.Rgba32UnsignedInt:
                        return this.AllocateCubeMap<Rgba128>(stream, width, height);
                }
            }

            throw new NotSupportedException("The pixel format is not supported");
        }

        /// <summary>
        /// Allocates and decodes the a KTX cube map texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of a texture face.</param>
        /// <param name="height">The height of a texture face.</param>
        /// <returns>A decoded cubemap texture.</returns>
        /// <exception cref="NotSupportedException">The pixel format is not supported</exception>
        private CubemapTexture AllocateCubeMap<TBlock>(Stream stream, int width, int height)
            where TBlock : struct, IBlock<TBlock>
        {
            var numberOfMipMaps = this.KtxHeader.NumberOfMipmapLevels != 0 ? this.KtxHeader.NumberOfMipmapLevels : 1;

            var cubeMapTexture = new CubemapTexture();
            var blockFormat = default(TBlock);
            for (int i = 0; i < numberOfMipMaps; i++)
            {
                var dataForEachFace = this.ReadTextureDataSize(stream);
                cubeMapTexture.PositiveX.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeX.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.PositiveY.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeY.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.PositiveZ.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeZ.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));

                width >>= 1;
                height >>= 1;
            }

            return cubeMapTexture;
        }

        private static MipMap<TBlock> ReadFaceTexture<TBlock>(Stream stream, int width, int height, TBlock blockFormat, uint dataForEachFace)
            where TBlock : struct, IBlock<TBlock>
        {
            byte[] faceData = new byte[dataForEachFace];
            ReadTextureData(stream, faceData);
            return new MipMap<TBlock>(blockFormat, faceData, width, height);
        }

        /// <summary>
        /// Allocates and decodes all mipmap levels of a ktx texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, uint count)
            where TBlock : struct, IBlock<TBlock>
        {
            MipMap[] mipMaps = this.ReadMipMaps<TBlock>(stream, width, height, count);

            return mipMaps;
        }

        private MipMap[] ReadMipMaps<TBlock>(Stream stream, int width, int height, uint count)
            where TBlock : struct, IBlock<TBlock>
        {
            // If numberOfMipmapLevels equals 0, it indicates that a full mipmap pyramid should be generated from level 0 at load time.
            // TODO: generate mipmap pyramid. For now only the first image is loaded.
            if (count == 0)
            {
                count = 1;
            }

            var blockFormat = default(TBlock);
            var mipMaps = new MipMap<TBlock>[count];
            for (int i = 0; i < count; i++)
            {
                var pixelDataSize = this.ReadTextureDataSize(stream);
                byte[] mipMapData = new byte[pixelDataSize];
                ReadTextureData(stream, mipMapData);

                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipMapData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }

        private static void ReadTextureData(Stream stream, byte[] mipMapData)
        {
            int bytesRead = stream.Read(mipMapData, 0, mipMapData.Length);
            if (bytesRead != mipMapData.Length)
            {
                throw new TextureFormatException("could not read enough texture data from the stream");
            }
        }

        private uint ReadTextureDataSize(Stream stream)
        {
            int bytesRead = stream.Read(this.buffer, 0, 4);
            if (bytesRead != 4)
            {
                throw new TextureFormatException("could not read texture data length from the stream");
            }

            var pixelDataSize = BinaryPrimitives.ReadUInt32LittleEndian(this.buffer);

            return pixelDataSize;
        }
    }
}

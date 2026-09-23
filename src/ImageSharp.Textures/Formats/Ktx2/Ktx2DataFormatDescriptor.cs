// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2;

/// <summary>
/// The fields of a KTX2 Data Format Descriptor basic block needed to distinguish formats that
/// share a <see cref="Enums.VkFormat"/>. UASTC in particular uses <c>VK_FORMAT_UNDEFINED</c> and
/// is identified by its DFD colour model and transfer function rather than by the format enum.
/// </summary>
internal readonly struct Ktx2DataFormatDescriptor
{
    /// <summary>
    /// Number of Data Format Descriptor bytes that cover every field <see cref="Parse"/> reads:
    /// the <c>dfdTotalSize</c> field plus the basic descriptor block's first three words.
    /// </summary>
    public const int BasicBlockHeaderSize = 16;

    /// <summary>
    /// Byte offset, from the start of the Data Format Descriptor, of the colour model field: 4 bytes
    /// for <c>dfdTotalSize</c> plus 8 bytes for the block's first two words.
    /// </summary>
    private const int ColorModelOffset = 12;

    /// <summary>
    /// Byte offset, from the start of the Data Format Descriptor, of the transfer function field
    /// (two bytes past the colour model, after colour primaries).
    /// </summary>
    private const int TransferFunctionOffset = 14;

    private Ktx2DataFormatDescriptor(byte colorModel, byte transferFunction)
    {
        this.ColorModel = colorModel;
        this.TransferFunction = transferFunction;
    }

    /// <summary>
    /// Gets the colour model (a <c>KHR_DF_MODEL_*</c> value).
    /// </summary>
    public byte ColorModel { get; }

    /// <summary>
    /// Gets the transfer function (a <c>KHR_DF_TRANSFER_*</c> value).
    /// </summary>
    public byte TransferFunction { get; }

    /// <summary>
    /// Gets a value indicating whether the colour model identifies UASTC content.
    /// </summary>
    public bool IsUastc => this.ColorModel == Ktx2Constants.KhrDfModelUastc;

    /// <summary>
    /// Gets a value indicating whether the transfer function is sRGB.
    /// </summary>
    public bool IsSrgbTransfer => this.TransferFunction == Ktx2Constants.KhrDfTransferSrgb;

    /// <summary>
    /// Parses the basic descriptor block fields from the Data Format Descriptor bytes
    /// </summary>
    /// <remarks>
    /// Fields beyond <paramref name="dfd"/> are treated as absent (zero), so a truncated
    /// or missing descriptor simply reports no match
    /// </remarks>
    /// <param name="dfd">The Data Format Descriptor bytes, starting at <c>dfdTotalSize</c>.</param>
    /// <returns>The parsed descriptor fields.</returns>
    public static Ktx2DataFormatDescriptor Parse(ReadOnlySpan<byte> dfd)
    {
        byte colorModel = dfd.Length > ColorModelOffset
            ? dfd[ColorModelOffset]
            : (byte)0;
        byte transferFunction = dfd.Length > TransferFunctionOffset
            ? dfd[TransferFunctionOffset]
            : (byte)0;

        return new Ktx2DataFormatDescriptor(colorModel, transferFunction);
    }
}

// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// One candidate RGB(A) endpoint encoding evaluated by <see cref="EndpointEncoder.EncodeColorsForMode"/>.
/// Wraps a <see cref="QuantizedEndpointPair"/> together with the encoding flags (blue-contract,
/// offset-mode, swap) that describe how the quantized values map to the ASTC endpoint modes
/// in spec §C.2.14 (RGB/RGBA direct vs base+offset).
/// </summary>
internal sealed class CEEncodingOption
{
    private readonly QuantizedEndpointPair quantizedEndpoints;
    private readonly bool swapEndpoints;
    private readonly bool blueContract;
    private readonly bool useOffsetMode;

    public CEEncodingOption(
        int squaredError,
        QuantizedEndpointPair quantizedEndpoints,
        bool swapEndpoints,
        bool blueContract,
        bool useOffsetMode)
    {
        this.Error = squaredError;
        this.quantizedEndpoints = quantizedEndpoints;
        this.swapEndpoints = swapEndpoints;
        this.blueContract = blueContract;
        this.useOffsetMode = useOffsetMode;
    }

    /// <summary>
    /// Gets the reconstruction error for this candidate (sum of squared channel diffs).
    /// </summary>
    public int Error { get; }

    /// <summary>
    /// Attempts to pack this candidate into the output <paramref name="values"/> list.
    /// Updates <paramref name="endpointMode"/> and toggles
    /// <paramref name="needsWeightSwap"/> when the encoding implies a weight swap.
    /// </summary>
    /// <returns>
    /// False when the candidate is not expressible in its mode (e.g. the base-offset branch cannot
    /// represent the signed-sum result), in which case the caller should try the next candidate
    /// </returns>
    public bool Pack(bool hasAlpha, out ColorEndpointMode endpointMode, List<int> values, ref bool needsWeightSwap)
    {
        endpointMode = ColorEndpointMode.LdrLumaDirect;

        int[] unquantizedLow = (int[])this.quantizedEndpoints.UnquantizedLow.Clone();
        int[] unquantizedHigh = (int[])this.quantizedEndpoints.UnquantizedHigh.Clone();

        if (this.useOffsetMode)
        {
            for (int i = 0; i < 4; ++i)
            {
                (unquantizedHigh[i], unquantizedLow[i]) = BitOperations.TransferPrecision(unquantizedHigh[i], unquantizedLow[i]);
            }
        }

        if (!this.TryResolveEndpointOrder(unquantizedLow, unquantizedHigh, out bool swapVals, ref needsWeightSwap))
        {
            return false;
        }

        int[] quantizedLow = (int[])this.quantizedEndpoints.QuantizedLow.Clone();
        int[] quantizedHigh = (int[])this.quantizedEndpoints.QuantizedHigh.Clone();

        if (swapVals)
        {
            if (this.useOffsetMode)
            {
                throw new InvalidOperationException("Offset mode requires sign test to resolve order, not a swap.");
            }

            (quantizedHigh, quantizedLow) = (quantizedLow, quantizedHigh);
            needsWeightSwap = !needsWeightSwap;
        }

        WriteValues(values, quantizedLow, quantizedHigh, hasAlpha);
        endpointMode = ResolveMode(this.useOffsetMode, hasAlpha);

        if (this.swapEndpoints)
        {
            needsWeightSwap = !needsWeightSwap;
        }

        return true;
    }

    /// <summary>
    /// Decides whether this candidate's stored (low, high) need to be swapped before emitting
    /// (non-offset mode only), and rejects offset-mode candidates whose sign test fails.
    /// </summary>
    private bool TryResolveEndpointOrder(int[] unquantizedLow, int[] unquantizedHigh, out bool swapVals, ref bool needsWeightSwap)
    {
        int sumLow = unquantizedLow[0] + unquantizedLow[1] + unquantizedLow[2];
        int sumHigh = unquantizedHigh[0] + unquantizedHigh[1] + unquantizedHigh[2];
        swapVals = false;

        if (this.useOffsetMode)
        {
            // Offset mode: §C.2.14 requires sumHigh < 0 (or >= 0 under blue-contract) to be
            // representable; otherwise the caller tries the next candidate.
            bool misordered = this.blueContract ? sumHigh >= 0 : sumHigh < 0;
            return !misordered;
        }

        if (this.blueContract)
        {
            if (sumHigh == sumLow)
            {
                return false;
            }

            swapVals = sumHigh > sumLow;
            needsWeightSwap = !needsWeightSwap;
            return true;
        }

        swapVals = sumHigh < sumLow;
        return true;
    }

    private static void WriteValues(List<int> values, int[] quantizedLow, int[] quantizedHigh, bool hasAlpha)
    {
        values[0] = quantizedLow[0];
        values[1] = quantizedHigh[0];
        values[2] = quantizedLow[1];
        values[3] = quantizedHigh[1];
        values[4] = quantizedLow[2];
        values[5] = quantizedHigh[2];
        if (hasAlpha)
        {
            values[6] = quantizedLow[3];
            values[7] = quantizedHigh[3];
        }
    }

    private static ColorEndpointMode ResolveMode(bool useOffsetMode, bool hasAlpha)
        => (useOffsetMode, hasAlpha) switch
        {
            (true, true) => ColorEndpointMode.LdrRgbaBaseOffset,
            (true, false) => ColorEndpointMode.LdrRgbBaseOffset,
            (false, true) => ColorEndpointMode.LdrRgbaDirect,
            (false, false) => ColorEndpointMode.LdrRgbDirect,
        };
}

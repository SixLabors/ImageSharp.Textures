// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Uastc;

[Trait("Format", "Astc")]
public class UastcModeTableTests
{
    [Fact]
    public void AllTables_HaveExpectedLength()
    {
        Assert.Equal(UastcMode.TotalModes, UastcMode.WeightBits.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.WeightRanges.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.EndpointRanges.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.Subsets.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.Planes.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.Comps.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.Cem.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.IsLa.Length);
        Assert.Equal(UastcMode.TotalModes, UastcMode.TotalHintBits.Length);
        Assert.Equal(128, UastcMode.HuffModes.Length);
        Assert.Equal(UastcMode.TotalModes + 1, UastcMode.HuffCodes.Length); // includes reserved mode 19
        Assert.Equal(21, UastcMode.BiseRangeTable.Length);
    }

    [Fact]
    public void ModeMetadata_IsInternallyConsistent()
    {
        for (int mode = 0; mode < UastcMode.TotalModes; mode++)
        {
            if (mode == UastcMode.SolidColorModeIndex)
            {
                Assert.Equal(0, UastcMode.Subsets[mode]);
                Assert.Equal(0, UastcMode.Planes[mode]);
                continue;
            }

            Assert.InRange(UastcMode.Subsets[mode], 1, 3);
            Assert.InRange(UastcMode.Planes[mode], 1, 2);
            Assert.InRange(UastcMode.WeightBits[mode], 1, 5);
            Assert.Contains(UastcMode.Comps[mode], new byte[] { 2, 3, 4 });
            Assert.Contains(UastcMode.Cem[mode], new byte[] { 4, 8, 12 });

            // Dual-plane modes are always single-subset (ASTC/UASTC constraint).
            if (UastcMode.Planes[mode] == 2)
            {
                Assert.Equal(1, UastcMode.Subsets[mode]);
            }

            // LA modes carry the LA CEM.
            if (UastcMode.IsLa[mode] != 0)
            {
                Assert.Equal(4, UastcMode.Cem[mode]);
                Assert.Equal(2, UastcMode.Comps[mode]);
            }
        }
    }

    [Fact]
    public void HuffModes_DecodeEveryPrefixToTheExpandedHuffCode()
    {
        // HuffModes is a 128-entry accelerator indexed by the low 7 bits of the block's first byte.
        // We rebuild it independently from the per-mode Huffman codes and assert they agree.
        byte[] expected = new byte[128];
        Array.Fill(expected, (byte)0xFF); // Any prefix not claimed by a mode stays 0xFF (invalid)

        for (int mode = 0; mode <= UastcMode.TotalModes; mode++)
        {
            // The mode's Huffman code occupies the low `length` bits
            (byte code, byte length) = UastcMode.HuffCodes[mode];

            // The remaining high bits belong to the block's payload
            int bitsLeft = 7 - length;
            for (int i = 0; i < (1 << bitsLeft); i++)
            {
                // Enumerate all prefixes that share this code and map each to the mode
                expected[code | (i << length)] = (byte)mode;
            }
        }

        for (int i = 0; i < 128; i++)
        {
            Assert.Equal(expected[i], UastcMode.HuffModes[i]);
        }
    }

    [Theory]
    [InlineData(0x1, 0)] // mode 0
    [InlineData(0x0, 11)] // mode 11 (2-bit code 0x0)
    [InlineData(0x17, 8)] // mode 8 (solid)
    [InlineData(0x45, 19)] // reserved
    public void TryDecodeMode_MapsKnownPrefixes(byte firstByte, int expectedMode)
    {
        bool ok = UastcMode.TryDecodeMode(firstByte, out int mode);

        Assert.Equal(expectedMode, mode);
        Assert.Equal(expectedMode < UastcMode.TotalModes, ok);
    }
}

namespace IdGen.Core.Services;

public record BarcodeBar(double X, double Width);

public class Code128Result
{
    public string OriginalText { get; init; } = string.Empty;
    public IReadOnlyList<int> SymbolCodes { get; init; } = Array.Empty<int>();
    public int Checksum { get; init; }
    public int TotalModules { get; init; }
    public int QuietZoneModules { get; init; } = 10;
    public IReadOnlyList<BarcodeBar> Bars { get; init; } = Array.Empty<BarcodeBar>();
}

public static class Code128Encoder
{
    public const int QuietZone = 10;

    // Standard Code 128 bar/space patterns (indices 0 to 106)
    // 0..105 have 6 digits (lengths of 3 bars and 3 spaces, sum = 11 modules)
    // 106 (Stop) has 7 digits (lengths of 4 bars and 3 spaces, sum = 13 modules)
    private static readonly string[] Patterns =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213", // 0-9
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132", // 10-19
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211", // 20-29
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313", // 30-39
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331", // 40-49
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111", // 50-59
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214", // 60-69
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111", // 70-79
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141", // 80-89
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141", // 90-99
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112"                                  // 100-106
    ];

    public const int StartA = 103;
    public const int StartB = 104;
    public const int StartC = 105;
    public const int StopCode = 106;

    /// <summary>
    /// Encodes a text string using Code 128 (auto Code B / Code C).
    /// </summary>
    public static Code128Result Encode(string text, bool includeQuietZone = true)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Barcode text cannot be null or empty.", nameof(text));
        }

        // Validate ASCII 32-126
        foreach (char c in text)
        {
            if (c < 32 || c > 126)
            {
                throw new ArgumentException($"Character '{c}' (ASCII {(int)c}) is not supported in Code 128.", nameof(text));
            }
        }

        var symbolCodes = new List<int>();

        // Check if string is purely numeric and has at least 4 digits
        bool isAllDigits = text.All(char.IsDigit);

        if (isAllDigits && text.Length >= 4)
        {
            // If even length, pure Code C
            if (text.Length % 2 == 0)
            {
                symbolCodes.Add(StartC);
                for (int i = 0; i < text.Length; i += 2)
                {
                    int val = int.Parse(text.Substring(i, 2));
                    symbolCodes.Add(val);
                }
            }
            else
            {
                // Odd digits: Start with Code B for first digit, then switch to Code C (code 99)
                symbolCodes.Add(StartB);
                symbolCodes.Add(text[0] - 32);
                symbolCodes.Add(99); // Code C switch
                for (int i = 1; i < text.Length; i += 2)
                {
                    int val = int.Parse(text.Substring(i, 2));
                    symbolCodes.Add(val);
                }
            }
        }
        else
        {
            // Standard Code B
            symbolCodes.Add(StartB);
            foreach (char c in text)
            {
                symbolCodes.Add(c - 32);
            }
        }

        // Calculate Modulo 103 Checksum:
        // Checksum = (StartCode + Sum(i * Code_i)) % 103
        long sum = symbolCodes[0];
        for (int i = 1; i < symbolCodes.Count; i++)
        {
            sum += (long)i * symbolCodes[i];
        }

        int checksum = (int)(sum % 103);
        symbolCodes.Add(checksum);
        symbolCodes.Add(StopCode);

        // Convert symbol patterns into Bars
        int qz = includeQuietZone ? QuietZone : 0;
        int currentModule = qz;
        var bars = new List<BarcodeBar>();

        foreach (int code in symbolCodes)
        {
            string pattern = Patterns[code];
            bool isBar = true;

            foreach (char digit in pattern)
            {
                int width = digit - '0';
                if (isBar)
                {
                    bars.Add(new BarcodeBar(currentModule, width));
                }
                currentModule += width;
                isBar = !isBar;
            }
        }

        int totalModules = currentModule + qz;

        return new Code128Result
        {
            OriginalText = text,
            SymbolCodes = symbolCodes,
            Checksum = checksum,
            TotalModules = totalModules,
            QuietZoneModules = qz,
            Bars = bars
        };
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace IdGen.App.Services;

public record CardPrintPage(byte[] PixelBytes, int Width, int Height, bool IsPortrait);

public static class NativePrintHelper
{
    private const int PD_RETURNDC = 0x00000100;
    private const int PD_NOSELECTION = 0x00000004;
    private const int PD_USEDEVMODECOPIESANDCOLLATE = 0x00040000;
    private const int PD_HIDEPRINTTOFILE = 0x00080000;

    private const int HORZRES = 8;
    private const int VERTRES = 10;
    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private const uint SRCCOPY = 0x00CC0020;
    private const uint DIB_RGB_COLORS = 0;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PRINTDLG
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hDevMode;
        public IntPtr hDevNames;
        public IntPtr hDC;
        public int Flags;
        public short nFromPage;
        public short nToPage;
        public short nMinPage;
        public short nMaxPage;
        public short nCopies;
        public IntPtr hInstance;
        public IntPtr lCustData;
        public IntPtr lpfnPrintHook;
        public IntPtr lpfnSetupHook;
        public string? lpPrintTemplateName;
        public string? lpSetupTemplateName;
        public IntPtr hPrintTemplate;
        public IntPtr hSetupTemplate;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DOCINFO
    {
        public int cbSize;
        public string lpszDocName;
        public string? lpszOutput;
        public string? lpszDatatype;
        public int fwType;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool PrintDlg(ref PRINTDLG lppd);

    [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int StartDoc(IntPtr hdc, ref DOCINFO lpdi);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int StartPage(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int EndPage(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int EndDoc(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int AbortDoc(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int StretchDIBits(
        IntPtr hdc,
        int xDest, int yDest, int destWidth, int destHeight,
        int xSrc, int ySrc, int srcWidth, int srcHeight,
        IntPtr lpBits,
        ref BITMAPINFO pbmi,
        uint iUsage,
        uint rop);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    public static bool PrintPages(IntPtr hwndOwner, string documentName, IReadOnlyList<CardPrintPage> pages)
    {
        if (pages == null || pages.Count == 0) return false;

        var pd = new PRINTDLG
        {
            lStructSize = Marshal.SizeOf<PRINTDLG>(),
            hwndOwner = hwndOwner,
            Flags = PD_RETURNDC | PD_NOSELECTION | PD_USEDEVMODECOPIESANDCOLLATE | PD_HIDEPRINTTOFILE,
            nFromPage = 1,
            nToPage = (short)pages.Count,
            nMinPage = 1,
            nMaxPage = (short)pages.Count,
            nCopies = 1
        };

        if (!PrintDlg(ref pd))
        {
            // User cancelled or dialog error
            FreePrintDlgHandles(ref pd);
            return false;
        }

        IntPtr hdc = pd.hDC;
        if (hdc == IntPtr.Zero)
        {
            FreePrintDlgHandles(ref pd);
            return false;
        }

        try
        {
            var docInfo = new DOCINFO
            {
                cbSize = Marshal.SizeOf<DOCINFO>(),
                lpszDocName = documentName
            };

            int docResult = StartDoc(hdc, ref docInfo);
            if (docResult <= 0)
            {
                return false;
            }

            int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
            int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
            int pageWidth = GetDeviceCaps(hdc, HORZRES);
            int pageHeight = GetDeviceCaps(hdc, VERTRES);

            if (dpiX <= 0) dpiX = 300;
            if (dpiY <= 0) dpiY = 300;

            foreach (var page in pages)
            {
                StartPage(hdc);

                // Physical CR80 dimension: 3.370" x 2.125" (standard ID-1 card)
                double cardWidthInches = page.IsPortrait ? 2.125 : 3.375;
                double cardHeightInches = page.IsPortrait ? 3.375 : 2.125;

                int targetWidth = (int)Math.Round(cardWidthInches * dpiX);
                int targetHeight = (int)Math.Round(cardHeightInches * dpiY);

                // Center the card on the page (or fit edge if on card printer)
                int destX = Math.Max(0, (pageWidth - targetWidth) / 2);
                int destY = Math.Max(0, (pageHeight - targetHeight) / 2);

                // If page matches card size within 15% (card printer), align to full printable area
                if (pageWidth > 0 && Math.Abs(pageWidth - targetWidth) < targetWidth * 0.15 &&
                    pageHeight > 0 && Math.Abs(pageHeight - targetHeight) < targetHeight * 0.15)
                {
                    destX = 0;
                    destY = 0;
                    targetWidth = pageWidth;
                    targetHeight = pageHeight;
                }

                var bmi = new BITMAPINFO
                {
                    bmiHeader = new BITMAPINFOHEADER
                    {
                        biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                        biWidth = page.Width,
                        biHeight = -page.Height, // top-down scanlines matching BGRA pixel buffer
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = 0, // BI_RGB
                        biSizeImage = (uint)(page.Width * page.Height * 4)
                    }
                };

                GCHandle pinned = GCHandle.Alloc(page.PixelBytes, GCHandleType.Pinned);
                try
                {
                    IntPtr pBits = pinned.AddrOfPinnedObject();
                    StretchDIBits(
                        hdc,
                        destX, destY, targetWidth, targetHeight,
                        0, 0, page.Width, page.Height,
                        pBits,
                        ref bmi,
                        DIB_RGB_COLORS,
                        SRCCOPY);
                }
                finally
                {
                    pinned.Free();
                }

                EndPage(hdc);
            }

            EndDoc(hdc);
            return true;
        }
        catch
        {
            AbortDoc(hdc);
            return false;
        }
        finally
        {
            FreePrintDlgHandles(ref pd);
        }
    }

    private static void FreePrintDlgHandles(ref PRINTDLG pd)
    {
        if (pd.hDC != IntPtr.Zero)
        {
            DeleteDC(pd.hDC);
            pd.hDC = IntPtr.Zero;
        }
        if (pd.hDevMode != IntPtr.Zero)
        {
            GlobalFree(pd.hDevMode);
            pd.hDevMode = IntPtr.Zero;
        }
        if (pd.hDevNames != IntPtr.Zero)
        {
            GlobalFree(pd.hDevNames);
            pd.hDevNames = IntPtr.Zero;
        }
    }
}

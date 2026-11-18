using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace IdGen.App.Services;

public static class CardExportHelper
{
    public static async Task<(byte[] PixelBytes, int Width, int Height)?> RenderElementToBgraAsync(UIElement element, int width, int height)
    {
        if (element == null || width <= 0 || height <= 0) return null;

        try
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element, width, height);

            var pixelBuffer = await rtb.GetPixelsAsync();
            byte[] pixelBytes = pixelBuffer.ToArray();

            return (pixelBytes, rtb.PixelWidth, rtb.PixelHeight);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<byte[]?> EncodeBgraToPngBytesAsync(byte[] bgraBytes, int width, int height, double dpi = 300.0)
    {
        if (bgraBytes == null || bgraBytes.Length == 0 || width <= 0 || height <= 0) return null;

        try
        {
            using var memStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memStream);

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                (uint)width,
                (uint)height,
                dpi,
                dpi,
                bgraBytes);

            await encoder.FlushAsync();

            byte[] pngBytes = new byte[memStream.Size];
            using var reader = new DataReader(memStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)memStream.Size);
            reader.ReadBytes(pngBytes);

            return pngBytes;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> SavePngFilePickerAsync(byte[] pngBytes, string suggestedName, IntPtr hwnd)
    {
        if (pngBytes == null || pngBytes.Length == 0) return false;

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = suggestedName
            };
            picker.FileTypeChoices.Add("PNG Image (*.png)", new List<string> { ".png" });

            if (hwnd != IntPtr.Zero)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSaveFileAsync();
            if (file == null) return false;

            await FileIO.WriteBytesAsync(file, pngBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<string?> SaveMultiplePngsToFolderPickerAsync(IReadOnlyList<(string FileName, byte[] PngBytes)> items, IntPtr hwnd)
    {
        if (items == null || items.Count == 0) return null;

        try
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add("*");

            if (hwnd != IntPtr.Zero)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var folder = await picker.PickSingleFolderAsync();
            if (folder == null) return null;

            foreach (var (fileName, bytes) in items)
            {
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteBytesAsync(file, bytes);
            }

            return folder.Path;
        }
        catch
        {
            return null;
        }
    }
}

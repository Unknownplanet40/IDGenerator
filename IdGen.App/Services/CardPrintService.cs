using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdGen.App.Views.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Services;

public record PrintExportResult(bool Success, string Message, bool IsCancelled);

public class CardPrintService
{
    public async Task<PrintExportResult> PrintOrExportCardAsync(
        XamlRoot xamlRoot,
        IntPtr hwnd,
        string studentName,
        string studentNo,
        Canvas? frontCanvas,
        Canvas? backCanvas,
        int canvasWidth,
        int canvasHeight,
        bool isPortrait)
    {
        if (frontCanvas == null && backCanvas == null)
        {
            return new PrintExportResult(false, "No card canvases available to render.", false);
        }

        var dialog = new PrintCardOptionDialog
        {
            XamlRoot = xamlRoot
        };

        var dialogResult = await dialog.ShowAsync();
        if (dialogResult == ContentDialogResult.None)
        {
            return new PrintExportResult(false, "Operation cancelled.", true);
        }

        var sideChoice = dialog.SelectedSide;

        // Collect pages to print or export
        var pages = new List<CardPrintPage>();
        var pngFiles = new List<(string FileName, byte[] PngBytes)>();

        // 1. Front Side
        if (sideChoice == CardPrintSideOption.BothSides || sideChoice == CardPrintSideOption.FrontOnly)
        {
            if (frontCanvas != null)
            {
                var rendered = await CardExportHelper.RenderElementToBgraAsync(frontCanvas, canvasWidth, canvasHeight);
                if (rendered.HasValue)
                {
                    pages.Add(new CardPrintPage(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height, isPortrait));

                    var pngBytes = await CardExportHelper.EncodeBgraToPngBytesAsync(
                        rendered.Value.PixelBytes,
                        rendered.Value.Width,
                        rendered.Value.Height,
                        300.0);

                    if (pngBytes != null)
                    {
                        string safeNo = string.IsNullOrWhiteSpace(studentNo) ? "Student" : studentNo;
                        pngFiles.Add(($"{safeNo}_ID_Front_CR80.png", pngBytes));
                    }
                }
            }
        }

        // 2. Back Side
        if (sideChoice == CardPrintSideOption.BothSides || sideChoice == CardPrintSideOption.BackOnly)
        {
            if (backCanvas != null)
            {
                var rendered = await CardExportHelper.RenderElementToBgraAsync(backCanvas, canvasWidth, canvasHeight);
                if (rendered.HasValue)
                {
                    pages.Add(new CardPrintPage(rendered.Value.PixelBytes, rendered.Value.Width, rendered.Value.Height, isPortrait));

                    var pngBytes = await CardExportHelper.EncodeBgraToPngBytesAsync(
                        rendered.Value.PixelBytes,
                        rendered.Value.Width,
                        rendered.Value.Height,
                        300.0);

                    if (pngBytes != null)
                    {
                        string safeNo = string.IsNullOrWhiteSpace(studentNo) ? "Student" : studentNo;
                        pngFiles.Add(($"{safeNo}_ID_Back_CR80.png", pngBytes));
                    }
                }
            }
        }

        if (pages.Count == 0)
        {
            return new PrintExportResult(false, "Failed to render card canvas.", false);
        }

        // Action: Print to Printer (Primary)
        if (dialogResult == ContentDialogResult.Primary)
        {
            string docName = $"Student ID - {studentNo} ({studentName})";
            bool printSuccess = NativePrintHelper.PrintPages(hwnd, docName, pages);
            if (printSuccess)
            {
                return new PrintExportResult(true, $"Card sent to printer successfully ({pages.Count} page(s)).", false);
            }
            else
            {
                return new PrintExportResult(false, "Print cancelled or printer error.", true);
            }
        }

        // Action: Export PNG (Secondary)
        if (dialogResult == ContentDialogResult.Secondary)
        {
            if (pngFiles.Count == 1)
            {
                var (fileName, bytes) = pngFiles[0];
                bool saved = await CardExportHelper.SavePngFilePickerAsync(bytes, fileName, hwnd);
                if (saved)
                {
                    return new PrintExportResult(true, $"Exported {fileName} successfully.", false);
                }
                else
                {
                    return new PrintExportResult(false, "Export cancelled.", true);
                }
            }
            else if (pngFiles.Count > 1)
            {
                string? folderPath = await CardExportHelper.SaveMultiplePngsToFolderPickerAsync(pngFiles, hwnd);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    return new PrintExportResult(true, $"Exported {pngFiles.Count} card images to {folderPath}.", false);
                }
                else
                {
                    return new PrintExportResult(false, "Export folder selection cancelled.", true);
                }
            }
        }

        return new PrintExportResult(false, "No action performed.", true);
    }
}

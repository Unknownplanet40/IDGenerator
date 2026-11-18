using Microsoft.UI.Xaml.Controls;

namespace IdGen.App.Views.Common;

public enum CardPrintSideOption
{
    BothSides,
    FrontOnly,
    BackOnly
}

public sealed partial class PrintCardOptionDialog : ContentDialog
{
    public CardPrintSideOption SelectedSide
    {
        get
        {
            if (FrontOnlyRadio.IsChecked == true) return CardPrintSideOption.FrontOnly;
            if (BackOnlyRadio.IsChecked == true) return CardPrintSideOption.BackOnly;
            return CardPrintSideOption.BothSides;
        }
    }

    public PrintCardOptionDialog()
    {
        this.InitializeComponent();
    }
}

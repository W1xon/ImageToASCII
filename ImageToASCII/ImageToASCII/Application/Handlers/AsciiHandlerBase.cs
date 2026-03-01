using ImageToASCII.ColorSystem;
using ImageToASCII.Core.Converters;
using ImageToASCII.Core.Processors;
using ImageToASCII.UI;

namespace ImageToASCII.Application;

public abstract class AsciiHandlerBase : BaseHandler
{
    protected BitmapToAsciiConverter AsciiConverter { get; private set; } = null!;
    protected AsciiExporter AsciiExporter { get; private set; } = null!;

    protected override bool CollectSettings()
    {
        Settings.AsciiPalette = ConsoleUI.AskAsciiPalette();
        Settings.Width        = ConsoleUI.AskWidth();
        Settings.PaletteType  = AskPaletteType();

        InitializeAsciiPipeline();

        return true;
    }
    
    protected virtual PaletteType AskPaletteType()
    {
        return ConsoleUI.AskPalette();
    }

    private void InitializeAsciiPipeline()
    {
        AsciiConverter = new BitmapToAsciiConverter(Settings.AsciiPalette.Characters.ToArray());

        AsciiExporter = new AsciiExporter(AsciiConverter)
        {
            AsciiWidth = Settings.Width
        };
    }
}
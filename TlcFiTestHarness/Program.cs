using System.Globalization;
using TLCFI.UI;

namespace TLCFI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

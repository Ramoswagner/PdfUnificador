using System.Runtime.InteropServices;

namespace PdfUnificador;

static class Program
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    /// <summary>
    ///  Habilita dark mode para a barra de título do Windows 10/11.
    /// </summary>
    public static void EnableDarkTitleBar(IntPtr handle)
    {
        if (Environment.OSVersion.Version.Major >= 10)
        {
            int value = 1;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }
    }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Forçar renderização de alta qualidade
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        Application.Run(new MainForm());
    }
}

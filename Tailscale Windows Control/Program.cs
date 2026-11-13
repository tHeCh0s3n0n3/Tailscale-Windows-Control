namespace Tailscale_Windows_Control;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // 1. Enable the .NET 11 modernized control renderer
        Application.SetDefaultVisualStylesMode(VisualStylesMode.Latest);

        // 2. Opt into Dark Mode (or follow the Windows OS setting)
        Application.SetColorMode(SystemColorMode.System); // Options: Dark, Classic, System

        // 3. Optional (.NET 11): Prevent white startup flash before dark theme settles
        Application.SetDefaultFormRevealMode(FormRevealMode.Deferred);

        Application.Run(new FrmMain());
    }
}
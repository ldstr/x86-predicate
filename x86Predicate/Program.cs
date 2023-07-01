using dnlib.DotNet.Writer;
using dnlib.DotNet;

namespace x86Predicate;

internal class Program
{
    static void Main(string[] args)
    {
    }

    private static void Save(
        string file,
        ModuleDefMD module
        )
    {
        Console.WriteLine("saving module...");

        var writer = new NativeModuleWriterOptions(module, true)
        {
            KeepExtraPEData = true,
            KeepWin32Resources = true,
            Logger = DummyLogger.NoThrowInstance
        };

        writer.MetadataOptions.Flags =
            MetadataFlags.PreserveAll |
            MetadataFlags.KeepOldMaxStack;

        module.NativeWrite(
            file + $"_No-x86.exe",
            writer
            );

        Console.WriteLine("Saved!");
    }

    private static void Exit(string? msg = null)
    {
        Console.WriteLine();

        if (!string.IsNullOrEmpty(msg))
            Console.WriteLine(msg);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
        Environment.Exit();
    }
}

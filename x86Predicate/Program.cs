using dnlib.DotNet.Writer;
using dnlib.DotNet;

namespace x86Predicate;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Exit("Usage: x86Predicate <PATH>");
            return;
        }

        var src = args[0];

        if (File.Exists(src))
        {
            Exit("File not found!");
            return;
        }

        var module = ModuleDefMD.Load(src);
        MethodDefExt.OriginalMD = module;

        foreach (var type in module.GetTypes())
        {
            if (!type.HasMethods)
                continue;

            foreach (var method in type.Methods)
            {
                if (!method.IsNative)
                    continue;
            }
        }

        Save(src, module);
        Exit();
    }

    private static void Save(
        string file,
        ModuleDefMD module
        )
    {
        Console.WriteLine("Saving...");

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
        Environment.Exit(0);
    }
}

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Runtime.InteropServices;

namespace x86Predicate;

internal static partial class MethodDefExt
{
    public static ModuleDefMD? OriginalMD { get; set; }

    [LibraryImport(
        "kernel32.dll",
        StringMarshalling = StringMarshalling.Utf16
        )]
    public static partial IntPtr LoadLibrary(string dllToLoad);

    public static byte[] ReadBodyFromRva(this MethodDef method)
    {
        if (OriginalMD == null)
            throw new Exception("Module is not set!");

        var stream = OriginalMD.Metadata.PEImage.CreateReader();
        var offset = OriginalMD.Metadata.PEImage.ToFileOffset(method.RVA);

        var buff = new byte[500];

        stream.Position = (uint)offset + 20;
        stream.ReadBytes(buff, 0, buff.Length);

        return buff;
    }

    public static IEnumerable<Instruction> FindAllReferences(
        this MethodDef mDef,
        ModuleDefMD module
        )
    {
        var returnList = new List<Instruction>();
        var totalMethods = new List<MethodDef>();

        for (var i = 1; i < 0x10000; i++)
        {
            var resolved = module.ResolveMethod((uint)i);
            if (resolved == null)
                continue;

            if (resolved.HasBody)
                totalMethods.Add(resolved);
        }

        foreach (var method in totalMethods)
        {
            if (!method.HasBody)
                continue;

            var insts = method.Body.Instructions;

            for (var i = 0; i < insts.Count; i++)
            {
                if (insts[i].OpCode != OpCodes.Call)
                    continue;

                if (
                    insts[i].Operand is MethodDef currentMethod &&
                    currentMethod.MDToken.ToInt32() == mDef.MDToken.ToInt32()
                    ) returnList.Add(insts[i]);

                if (
                    insts[i].Operand is MethodSpec currentMethodSpec &&
                    currentMethodSpec.MDToken.ToInt32() == mDef.MDToken.ToInt32()
                    ) returnList.Add(insts[i]);
            }
        }

        return returnList;
    }
}
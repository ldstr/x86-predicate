using dnlib.DotNet;
using SharpDisasm;
using x86Predicate.X86.Instructions;
using x86Predicate.X86.Types;

namespace x86Predicate.X86;

internal class X86Method
{
    public List<X86Instruction> Instructions;
    public Stack<int> LocalStack = new();
    public MethodDef OriginalMethod;

    public Dictionary<string, int> Registers = new()
    {
        {"EAX", 0},
        {"EBX", 0},
        {"ECX", 0},
        {"EDX", 0},
        {"ESP", 0},
        {"EBP", 0},
        {"ESI", 0},
        {"EDI", 0}
    };

    public X86Method(MethodDef method)
    {
        Instructions = new List<X86Instruction>();
        ParseInstructions(method);

        OriginalMethod = method;
    }

    private void ParseInstructions(MethodDef method)
    {
        var instructions = new Disassembler(
            method.ReadBodyFromRva(),
            ArchitectureMode.x86_32
            ).Disassemble().ToList();

        if (instructions.Count == 0)
            return;

        foreach (var inst in instructions)
        {
            AddIns(inst, out bool retReached);

            if (retReached)
                break;
        }
    }

    private void AddIns(
        Instruction inst,
        out bool retReached
        )
    {
        retReached = false;
        var currentInstruction = inst + " ";

        currentInstruction = currentInstruction.Remove(
            currentInstruction.IndexOf(" ")
            );

        switch (currentInstruction)
        {
            case "nop": break;
            case "mov":
                Instructions.Add(new X86Mov(inst));
                break;
            case "add":
                Instructions.Add(new X86Add(inst));
                break;
            case "sub":
                Instructions.Add(new X86Sub(inst));
                break;
            case "imul":
                Instructions.Add(new X86IMul(inst));
                break;
            case "div":
                Instructions.Add(new X86Div(inst));
                break;
            case "neg":
                Instructions.Add(new X86Neg(inst));
                break;
            case "not":
                Instructions.Add(new X86Not(inst));
                break;
            case "xor":
                Instructions.Add(new X86Xor(inst));
                break;
            case "pop":
                Instructions.Add(new X86Pop(inst));
                break;

            case "ret":
                try
                {
                    while (Instructions[^1].OpCode == X86OpCode.POP)
                        Instructions.RemoveAt(Instructions.Count - 1);
                }
                catch { }

                retReached = true;
                break;
        }
    }

    public int Execute(params int[] @params)
    {
        foreach (var param in @params)
            LocalStack.Push(param);

        foreach (var inst in Instructions)
            inst.Execute(Registers, LocalStack);

        return Registers["EAX"];
    }
}
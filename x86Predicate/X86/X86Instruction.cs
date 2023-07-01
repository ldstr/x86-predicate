using x86Predicate.X86.Types;

namespace x86Predicate.X86;

internal abstract class X86Instruction
{
    public abstract X86OpCode OpCode { get; }

    public IX86Operand[]? Operands { get; set; }

    public abstract void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        );
}
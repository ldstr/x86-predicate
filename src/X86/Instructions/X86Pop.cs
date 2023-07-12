using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86.Instructions;

internal class X86Pop : X86Instruction
{
    public X86Pop(Instruction rawInstruction) => Operands = new IX86Operand[1]
    {
        rawInstruction.Operands[0].GetOperand()
    };

    public override X86OpCode OpCode => X86OpCode.POP;

    public override void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        )
    {
        if (Operands == null || localStack.Count < 1)
            return;

        registers[
            ((X86RegisterOperand)Operands[0]).Register.ToString()
            ] = localStack.Pop();
    }
}
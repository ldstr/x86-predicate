using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86.Instructions;

internal class X86Not : X86Instruction
{
    public X86Not(Instruction rawInstruction) => Operands = new IX86Operand[1]
    {
        rawInstruction.Operands[0].GetOperand()
    };

    public override X86OpCode OpCode => X86OpCode.NOT;

    public override void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        )
    {
        if (Operands == null)
            return;

        registers[((X86RegisterOperand)Operands[0]).Register.ToString()] =
            ~registers[((X86RegisterOperand)Operands[0]).Register.ToString()];
    }
}
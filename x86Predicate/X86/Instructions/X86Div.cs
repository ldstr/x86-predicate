using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86.Instructions;

internal class X86Div : X86Instruction
{
    public X86Div(Instruction rawInstruction) => Operands = new IX86Operand[2]
    {
        rawInstruction.Operands[0].GetOperand(),
        rawInstruction.Operands[1].GetOperand()
    };

    public override X86OpCode OpCode => X86OpCode.DIV;

    public override void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        ) { }
}
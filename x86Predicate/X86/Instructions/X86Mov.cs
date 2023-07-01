using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86.Instructions;

internal class X86Mov : X86Instruction
{
    public X86Mov(Instruction rawInstruction) => Operands = new IX86Operand[2]
    {
        rawInstruction.Operands[0].GetOperand(),
        rawInstruction.Operands[1].GetOperand()
    };

    public override X86OpCode OpCode => X86OpCode.MOV;

    public override void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        )
    {
        if (Operands == null) return;
        else if (Operands[1] is X86ImmediateOperand operand)
            registers[((X86RegisterOperand)Operands[0]).Register.ToString()] =
                operand.Immediate;
        else
            registers[((X86RegisterOperand)Operands[0]).Register.ToString()] =
                registers[((X86RegisterOperand)Operands[1]).Register.ToString()];
    }
}
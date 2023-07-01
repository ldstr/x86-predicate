using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86.Instructions;

internal class X86IMul : X86Instruction
{
    public X86IMul(Instruction rawInstruction) => Operands = new IX86Operand[3]
    {
        rawInstruction.Operands[0].GetOperand(),
        rawInstruction.Operands[1].GetOperand(),
        rawInstruction.Operands[2].GetOperand()
    };

    public override X86OpCode OpCode => X86OpCode.IMUL;

    public override void Execute(
        Dictionary<string, int> registers,
        Stack<int> localStack
        )
    {
        if (Operands == null)
            return;

        var source = ((X86RegisterOperand)Operands[0]).Register.ToString();
        var target = ((X86RegisterOperand)Operands[1]).Register.ToString();

        if (Operands[2] is X86ImmediateOperand operand)
            registers[source] = registers[target] * operand.Immediate;
        else
            registers[source] =
                registers[target] * registers[((X86RegisterOperand)Operands[2]).Register.ToString()];
    }
}
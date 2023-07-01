using x86Predicate.X86.Types;

namespace x86Predicate.X86;

internal class X86ImmediateOperand : IX86Operand
{
    public X86ImmediateOperand(int imm) => Immediate = imm;

    public int Immediate { get; set; }
}
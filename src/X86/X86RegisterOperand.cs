using x86Predicate.X86.Types;

namespace x86Predicate.X86;

internal class X86RegisterOperand : IX86Operand
{
    public X86RegisterOperand(X86Register reg) => Register = reg;

    public X86Register Register { get; set; }
}
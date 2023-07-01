using SharpDisasm.Udis86;
using SharpDisasm;
using x86Predicate.X86.Types;

namespace x86Predicate.X86;

internal static class MiscExt
{
    public static IX86Operand GetOperand(this Operand arg) => arg.Type switch
    {
        ud_type.UD_OP_IMM => new X86ImmediateOperand((int)arg.Value),
        _ => new X86RegisterOperand((X86Register)arg.Base)
    };
}
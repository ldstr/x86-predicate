using dnlib.DotNet;
using dnlib.DotNet.Emit;
using x86Predicate.X86;
using x86Predicate.X86.Types;

namespace x86Predicate;

internal class X86ToILConverter
{
    public static MethodDef ILFromX86Method(X86Method methodToConvert)
    {
        var int32Type = methodToConvert.OriginalMethod.ReturnType;
        var name = new UTF8String(methodToConvert.OriginalMethod.Name + "_IL");
        var metSig = MethodSig.CreateStatic(int32Type, int32Type);

        var impFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
        var metAttr = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

        var returnMethod = new MethodDefUser(name, metSig, impFlags, metAttr)
        {
            Body = new CilBody()
        };

        returnMethod.Body.MaxStack = 12;

        for (var i = 0; i < 8; i++)
            returnMethod.Body.Variables.Add(new Local(int32Type));

        var returnMethodBody = returnMethod.Body;

        returnMethodBody.Instructions.Add(new Instruction(OpCodes.Ldarg_0));

        foreach (var x86Inst in methodToConvert.Instructions)
        {
            if (x86Inst.Operands == null)
                continue;

            switch (x86Inst.OpCode)
            {
                case X86OpCode.MOV:
                    ConvertMov(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.ADD:
                    ConvertAdd(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.DIV:
                    ConvertDiv(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.IMUL:
                    ConvertIMul(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.NEG:
                    ConvertNeg(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.NOT:
                    ConvertNot(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.POP:
                    returnMethodBody.Instructions.Add(
                        GetStlocInsFromReg(
                            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
                            returnMethodBody
                        ));
                    break;

                case X86OpCode.SUB:
                    ConvertSub(x86Inst, ref returnMethodBody);
                    break;

                case X86OpCode.XOR:
                    ConvertXor(x86Inst, ref returnMethodBody);
                    break;
            }
        }

        returnMethodBody.Instructions.Add(
            new(OpCodes.Ldloc_0)
            );

        returnMethodBody.Instructions.Add(
            new(OpCodes.Ret)
            );

        return returnMethod;
    }

    private static void ConvertMov(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't mov value to immediate value");

        if (x86Inst.Operands[1] is X86RegisterOperand operand)
            returnMethodBody.Instructions.Add(
                GetLdlocInsFromReg(operand.Register, returnMethodBody)
                );
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand1)
            returnMethodBody.Instructions.Add(
                new(OpCodes.Ldc_I4, operand1.Immediate)
                );

        returnMethodBody.Instructions.Add(
            GetStlocInsFromReg(
                ((X86RegisterOperand)x86Inst.Operands[0]).Register,
                returnMethodBody
                )
            );
    }

    private static void ConvertAdd(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't add value to immediate value");

        returnMethodBody.Instructions.Add(
            GetLdlocInsFromReg(
                ((X86RegisterOperand)x86Inst.Operands[0]).Register,
                returnMethodBody
                ));

        if (x86Inst.Operands[1] is X86RegisterOperand operand1)
            returnMethodBody.Instructions.Add(
                GetLdlocInsFromReg(
                    operand1.Register,
                    returnMethodBody
                    ));
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand2)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand2.Immediate));

        returnMethodBody.Instructions.Add(new(OpCodes.Add));

        returnMethodBody.Instructions.Add(
            GetStlocInsFromReg(
                ((X86RegisterOperand)x86Inst.Operands[0]).Register,
                returnMethodBody
                ));
    }

    private static void ConvertDiv(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't div value to immediate value");

        returnMethodBody.Instructions.Add(
            GetLdlocInsFromReg((
            (X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));

        if (x86Inst.Operands[1] is X86RegisterOperand operand2)
            returnMethodBody.Instructions.Add(
                GetLdlocInsFromReg(operand2.Register, returnMethodBody)
                );
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand.Immediate));

        returnMethodBody.Instructions.Add(new(OpCodes.Div_Un));

        returnMethodBody.Instructions.Add(
            GetStlocInsFromReg((
            (X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static void ConvertIMul(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't imul value to immediate value");

        if (x86Inst.Operands[1] is X86RegisterOperand operand)
            returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
                operand.Register,
                returnMethodBody
                ));
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand1)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand1.Immediate));

        if (x86Inst.Operands[2] is X86RegisterOperand operand2)
            returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
                operand2.Register,
                returnMethodBody
                ));
        else if (x86Inst.Operands[2] is X86ImmediateOperand operand3)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand3.Immediate));

        returnMethodBody.Instructions.Add(new(OpCodes.Mul));

        returnMethodBody.Instructions.Add(GetStlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static void ConvertNeg(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't neg immediate value");

        returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));

        returnMethodBody.Instructions.Add(new(OpCodes.Neg));

        returnMethodBody.Instructions.Add(GetStlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static void ConvertNot(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't not immediate value");

        returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));

        returnMethodBody.Instructions.Add(new(OpCodes.Not));

        returnMethodBody.Instructions.Add(GetStlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static void ConvertSub(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't sub value to immediate value");

        returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));

        if (x86Inst.Operands[1] is X86RegisterOperand operand)
            returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
                operand.Register,
                returnMethodBody
                ));
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand1)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand1.Immediate));

        returnMethodBody.Instructions.Add(new(OpCodes.Sub));

        returnMethodBody.Instructions.Add(GetStlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static void ConvertXor(
        X86Instruction x86Inst,
        ref CilBody returnMethodBody
        )
    {
        if (x86Inst.Operands == null)
            throw new ArgumentNullException(nameof(x86Inst));
        else if (x86Inst.Operands[0] is X86ImmediateOperand)
            throw new Exception("Can't xor value to immediate value");

        returnMethodBody.Instructions.Add(GetLdlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));

        if (x86Inst.Operands[1] is X86RegisterOperand operand)
            returnMethodBody.Instructions.Add(
                GetLdlocInsFromReg(
                    operand.Register,
                    returnMethodBody
                    ));
        else if (x86Inst.Operands[1] is X86ImmediateOperand operand1)
            returnMethodBody.Instructions.Add(new(OpCodes.Ldc_I4, operand1.Immediate));

        returnMethodBody.Instructions.Add(new(OpCodes.Xor));

        returnMethodBody.Instructions.Add(GetStlocInsFromReg(
            ((X86RegisterOperand)x86Inst.Operands[0]).Register,
            returnMethodBody
            ));
    }

    private static Instruction GetLdlocInsFromReg(
        X86Register register,
        CilBody body
        ) => register switch
        {
            X86Register.EAX => new(OpCodes.Ldloc_0),
            X86Register.ECX => new(OpCodes.Ldloc_1),
            X86Register.EDX => new(OpCodes.Ldloc_2),
            X86Register.EBX => new(OpCodes.Ldloc_3),
            X86Register.ESI => new(OpCodes.Ldloc_S, body.Variables[6]),
            X86Register.EDI => new(OpCodes.Ldloc, body.Variables[7]),
            _ => throw new Exception("Not implemented"),
        };

    private static Instruction GetStlocInsFromReg(
        X86Register register,
        CilBody body
        ) => register switch
        {
            X86Register.EAX => new(OpCodes.Stloc_0),
            X86Register.ECX => new(OpCodes.Stloc_1),
            X86Register.EDX => new(OpCodes.Stloc_2),
            X86Register.EBX => new(OpCodes.Stloc_3),
            X86Register.ESI => new(OpCodes.Stloc_S, body.Variables[6]),
            X86Register.EDI => new(OpCodes.Stloc_S, body.Variables[7]),
            _ => throw new Exception("Not implemented"),
        };
}
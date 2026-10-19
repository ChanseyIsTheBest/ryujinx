using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    // banjo-nx (2026-10-18): PRMT (byte permute), which this translator logged as not implemented and skipped. The
    // default mode: each result byte i is byte (nibble i of the selector) & 7 of the pair {C:A} (A bytes 0-3, C bytes
    // 4-7), or that byte's sign replicated when bit 3 of the nibble is set. The other modes are still not implemented.
    static partial class InstEmit
    {
        public static void PrmtR(EmitterContext context)
        {
            InstPrmtR op = context.GetOp<InstPrmtR>();

            EmitPrmt(context, GetSrcReg(context, op.SrcA), GetSrcReg(context, op.SrcB), GetSrcReg(context, op.SrcC), op.Dest, op.PMode);
        }

        public static void PrmtI(EmitterContext context)
        {
            InstPrmtI op = context.GetOp<InstPrmtI>();

            EmitPrmt(context, GetSrcReg(context, op.SrcA), GetSrcImm(context, Imm20ToSInt(op.Imm20)), GetSrcReg(context, op.SrcC), op.Dest, op.PMode);
        }

        public static void PrmtC(EmitterContext context)
        {
            InstPrmtC op = context.GetOp<InstPrmtC>();

            EmitPrmt(context, GetSrcReg(context, op.SrcA), GetSrcCbuf(context, op.CbufSlot, op.CbufOffset), GetSrcReg(context, op.SrcC), op.Dest, op.PMode);
        }

        public static void PrmtRc(EmitterContext context)
        {
            InstPrmtRc op = context.GetOp<InstPrmtRc>();

            EmitPrmt(context, GetSrcReg(context, op.SrcA), GetSrcReg(context, op.SrcC), GetSrcCbuf(context, op.CbufSlot, op.CbufOffset), op.Dest, op.PMode);
        }

        private static void EmitPrmt(EmitterContext context, Operand srcA, Operand selector, Operand srcC, int rd, PMode mode)
        {
            if (mode != 0)
            {
                context.TranslatorContext.GpuAccessor.Log($"Shader instruction Prmt mode {mode} is not implemented.");

                return;
            }

            Operand res = Const(0);

            for (int i = 0; i < 4; i++)
            {
                Operand nibble = context.BitfieldExtractU32(selector, Const(i * 4), Const(4));
                Operand index = context.BitwiseAnd(nibble, Const(7));
                Operand source = context.ConditionalSelect(context.ICompareLessUnsigned(index, Const(4)), srcA, srcC);
                Operand shift = context.ShiftLeft(context.BitwiseAnd(index, Const(3)), Const(3));
                Operand value = context.BitfieldExtractU32(source, shift, Const(8));
                Operand sign = context.INegate(context.ShiftRightU32(value, Const(7)));
                Operand signed = context.BitwiseAnd(sign, Const(0xff));
                value = context.ConditionalSelect(context.ICompareNotEqual(context.BitwiseAnd(nibble, Const(8)), Const(0)), signed, value);
                res = context.BitwiseOr(res, context.ShiftLeft(value, Const(i * 8)));
            }

            context.Copy(GetDest(rd), res);
        }
    }
}

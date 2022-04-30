using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MathCleaner.Parameters
{
    internal class Two
    {
        public static void Execute(ModuleDefMD mod, Assembly asm)
        {
            foreach (var type in mod.GetTypes())
            {
                foreach (var method in type.Methods.Where(x => x.HasBody))
                {
                    DoubleDoubleParams(method, asm);
                    DoubleIntParams(method, asm);
                    SingleSingleParams(method, asm);
                    IntIntParams(method, asm);
                }
            }
        }
        static void DoubleDoubleParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr &&
                    mr.MethodSig.ToString().Contains("System.Double (System.Double,System.Double)") && instr[i - 1].OpCode == OpCodes.Ldc_R8 && instr[i - 2].OpCode == OpCodes.Ldc_R8)
                {
                    double val1 = (double)instr[i - 2].Operand;
                    double val2 = (double)instr[i - 1].Operand;
                    var result = (double)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { val1, val2});
                    Console.WriteLine($"Found Math.{mr.Name}({val1},{val2}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Ldc_R8;
                    instr[i].Operand = result;
                    instr[i - 1].OpCode = OpCodes.Nop;
                    instr[i - 2].OpCode = OpCodes.Nop;
                }
            }
            Program.Cleaner(method);
        }
        static void DoubleIntParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr &&
                    mr.MethodSig.ToString().Contains("System.Double (System.Double,System.Int32)") && instr[i - 1].IsLdcI4() && instr[i - 2].OpCode == OpCodes.Ldc_R8)
                {
                    double val1 = (double)instr[i - 2].Operand;
                    int val2 = instr[i - 1].GetLdcI4Value();
                    var result = (double)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { val1, val2 });
                    Console.WriteLine($"Found Math.{mr.Name}({val1},{val2}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Ldc_R8;
                    instr[i].Operand = result;
                    instr[i - 1].OpCode = OpCodes.Nop;
                    instr[i - 2].OpCode = OpCodes.Nop;
                }
            }
            Program.Cleaner(method);
        }
        static void SingleSingleParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr &&
                    mr.MethodSig.ToString().Contains("System.Single (System.Single,System.Single)") && instr[i - 1].OpCode == OpCodes.Ldc_R4 && instr[i - 2].OpCode == OpCodes.Ldc_R4)
                {
                    float val1 = (float)instr[i - 2].Operand;
                    float val2 = (float)instr[i - 1].Operand;
                    var result = (float)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { val1, val2 });
                    Console.WriteLine($"Found Math.{mr.Name}({val1},{val2}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Ldc_R4;
                    instr[i].Operand = result;
                    instr[i - 1].OpCode = OpCodes.Nop;
                    instr[i - 2].OpCode = OpCodes.Nop;
                }
            }
            Program.Cleaner(method);
        }
        static void IntIntParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr &&
                    mr.MethodSig.ToString().Contains("System.Int32 (System.Int32,System.Int32)") && instr[i - 1].IsLdcI4() && instr[i - 2].IsLdcI4())
                {
                    int val1 = instr[i - 2].GetLdcI4Value();
                    int val2 = instr[i - 1].GetLdcI4Value();
                    var result = (int)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { val1, val2 });
                    Console.WriteLine($"Found Math.{mr.Name}({val1},{val2}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Ldc_I4;
                    instr[i].Operand = result;
                    instr[i - 1].OpCode = OpCodes.Nop;
                    instr[i - 2].OpCode = OpCodes.Nop;
                }
            }
            Program.Cleaner(method);
        }
    }
}

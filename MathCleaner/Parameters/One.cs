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
    internal class One
    {
        public static void Execute(ModuleDefMD mod, Assembly asm)
        {
            foreach(var type in mod.GetTypes())
            {
                foreach(var method in type.Methods.Where(x => x.HasBody))
                {
                    IntParams(method, asm);
                    SingleParams(method, asm);
                    DoubleParams(method, asm);
                }
            }
        }
        static void IntParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr && 
                    mr.MethodSig.ToString().Contains("System.Int32") && instr[i - 1].IsLdcI4())
                {
                    var result = (double)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { instr[i - 1].GetLdcI4Value() });
                    Console.WriteLine($"Found Math.{mr.Name}({instr[i - 1].GetLdcI4Value()}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i - 1].Operand = result;
                }
            }
            Program.Cleaner(method);
        }
        static void SingleParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for (var i = 0; i < instr.Count; i++)
            {
                if (instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr && 
                    mr.MethodSig.ToString().Contains("System.Single") && instr[i - 1].OpCode == OpCodes.Ldc_R4)
                {
                    var result = (float)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { (float)instr[i - 1].Operand });
                    Console.WriteLine($"Found Math.{mr.Name}({(float)instr[i - 1].Operand}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i - 1].Operand = result;
                }
            }
            Program.Cleaner(method);
        }
        static void DoubleParams(MethodDef method, Assembly asm)
        {
            var instr = method.Body.Instructions;
            for(var i = 0; i < instr.Count; i++)
            {
                if(instr[i].OpCode == OpCodes.Call && instr[i].Operand.ToString().Contains("System.Math") && instr[i].Operand is MemberRef mr &&
                    mr.MethodSig.ToString().Contains("System.Double") && instr[i - 1].OpCode == OpCodes.Ldc_R8)
                {
                    var result = (double)asm.ManifestModule.ResolveMethod(mr.MDToken.ToInt32()).Invoke(null, new object[] { (double)instr[i - 1].Operand });
                    Console.WriteLine($"Found Math.{mr.Name}({(double)instr[i - 1].Operand}) --> Result : {result}");
                    instr[i].OpCode = OpCodes.Nop;
                    instr[i - 1].Operand = result;
                }
            }
            Program.Cleaner(method);
        }
    }
}

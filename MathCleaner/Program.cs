using de4dot.blocks;
using de4dot.blocks.cflow;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MathCleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filename = args[0];
            var mod = ModuleDefMD.Load(filename);
            var asm = Assembly.Load(filename);
            Parameters.One.Execute(mod, asm);
            Parameters.Two.Execute(mod, asm);
            LoadAsmRef(mod, filename);
            SaveFile(mod, filename);
        }
        public static void Cleaner(MethodDef method)
        {
            try
            {
                var blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
                Blocks blocks = new Blocks(method);
                blocksCflowDeobfuscator.Initialize(blocks);
                blocksCflowDeobfuscator.Deobfuscate();
                blocks.RepartitionBlocks();
                blocks.GetCode(out IList<Instruction> allInstructions, out IList<ExceptionHandler> allExceptionHandlers);
                DotNetUtils.RestoreBody(method, allInstructions, allExceptionHandlers);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static void SaveFile(ModuleDefMD Module, string filename)
        {
            var InputExtension = Path.GetExtension(filename);
            var InputPath = filename;
            var OutputPath = InputPath.Replace(InputExtension, "_NoMath" + InputExtension);
            if (Module.IsILOnly)
            {
                ModuleWriterOptions options = new ModuleWriterOptions(Module)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                options.MetadataOptions.Flags = MetadataFlags.PreserveAll | MetadataFlags.KeepOldMaxStack;
                Module.Write(OutputPath, options);
            }
            else
            {
                NativeModuleWriterOptions options = new NativeModuleWriterOptions(Module, optimizeImageSize: false)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                options.MetadataOptions.Flags = MetadataFlags.PreserveAll | MetadataFlags.KeepOldMaxStack;
                Module.NativeWrite(OutputPath, options);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n\nFile Saved To : {OutputPath}");
        }
        public static void LoadAsmRef(ModuleDefMD Module, string filename)
        {
            var asmResolver = new AssemblyResolver();
            var modCtx = new ModuleContext(asmResolver);
            asmResolver.DefaultModuleContext = modCtx;
            asmResolver.EnableTypeDefCache = true;

            Module.Location = filename;
            var asmRefs = Module.GetAssemblyRefs();
            Module.Context = modCtx;
            foreach (var asmRef in asmRefs)
            {
                if (asmRef == null)
                    continue;
                var asma = asmResolver.Resolve(asmRef.FullName, Module);


                //	Protections.Protections.ModuleDef.Context.AssemblyResolver.AddToCache(asma);
                ((AssemblyResolver)Module.Context.AssemblyResolver).AddToCache(asma);
            }
        }
    }

}

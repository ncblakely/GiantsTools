﻿namespace Giants.EffectCompiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Giants.BinTools;
    using Giants.BinTools.Macro;
    using NLog;

    /// <summary>
    /// Effect binary decompiler.
    /// </summary>
    public class FxDecompiler
    {
        private static readonly Logger logger = LogManager.GetLogger(nameof(FxDecompiler));

        private readonly Dictionary<string, int> tableOfContents = new Dictionary<string, int>();

        private FxBinaryData binaryData;
        private FxMacroDefinitionTable macroDefinitionTable;
        private int doneOpcode;
        private int pushObjOpcode;
        private int popObjOpcode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FxDecompiler"/> class.
        /// </summary>
        /// <param name="macroDefinitionTable">The table of macro and symbol definitions.</param>
        public FxDecompiler(FxMacroDefinitionTable macroDefinitionTable)
        {
            this.macroDefinitionTable = macroDefinitionTable;
        }

        /// <summary>
        /// Decompiles an effect binary file to the specified output path.
        /// </summary>
        /// <param name="inputPath">The path to the effect binary file.</param>
        /// <param name="outputPath">The path to write to.</param>
        public void Decompile(
            string inputPath,
            string outputPath)

        {
            if (!File.Exists(inputPath))
            {
                throw new InvalidOperationException($"The input file {inputPath} does not exist.");
            }

            this.ReadBinaryData(inputPath);

            using var memoryStream = new MemoryStream(this.binaryData.Data);
            using var binaryReader = new BinaryReader(memoryStream);
            using var streamWriter = new StreamWriter(outputPath, false);
            using var textWriter = new IndentedTextWriter(streamWriter);

            this.BuildTableOfContents(binaryReader);

            // Get constants for known symbols
            this.doneOpcode = Utilities.GetFxSymbolValue(this.macroDefinitionTable.SymbolTable, "FxDone");
            this.pushObjOpcode = Utilities.GetFxSymbolValue(this.macroDefinitionTable.SymbolTable, "FxPushObj");
            this.popObjOpcode = Utilities.GetFxSymbolValue(this.macroDefinitionTable.SymbolTable, "FxPopObj");

            this.ProcessEffects(binaryReader, textWriter);
        }

        private void ReadBinaryData(string path)
        {
            using var fileStream = File.OpenRead(path);
            this.binaryData = new FxBinaryData(fileStream);
        }

        private void BuildTableOfContents(BinaryReader binaryReader)
        {
            for (int i = 0; i < this.binaryData.EffectCount; i++)
            {
                string fxName = binaryReader.ReadCString();
                int offset = binaryReader.ReadInt32();

                if (this.tableOfContents.ContainsKey(fxName))
                {
                    logger.Warn($"TOC already contains fx '{fxName}'; this may be a bug");
                    continue;
                }

                this.tableOfContents.Add(fxName, offset);
            }
        }

        private void ProcessEffects(BinaryReader binaryReader, IndentedTextWriter fileStream)
        {
            try
            {
                foreach (var kvp in this.tableOfContents)
                {
                    this.WriteHeader(fileStream);
                    this.ProcessEffect(binaryReader, fileStream, kvp.Key, kvp.Value);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return;
            }
        }

        private void WriteHeader(IndentedTextWriter fileStream)
        {
            fileStream.WriteLine("; Generated by FX Compiler. Comments will be lost.");
        }

        private void ProcessEffect(
            BinaryReader reader,
            IndentedTextWriter fileStream,
            string name,
            int offset)
        {
            logger.Info($"Processing '{name}'");

            reader.BaseStream.Position = offset;

            fileStream.WriteLine($"fxdef {name}");
            fileStream.Indent = 1;

            int lastOpcode = 0;
            try
            {
                while (true)
                {
                    int opcode = reader.ReadByte();
                    if (!this.macroDefinitionTable.MacroDefinitions.TryGetValue(opcode, out FxMacroDefinition macroDefinition))
                    {
                        throw new InvalidOperationException("Unable to map opcode to macro definition");
                    }

                    if (opcode == this.doneOpcode)
                    {
                        fileStream.Indent--;
                        break;
                    }

                    if (lastOpcode == this.pushObjOpcode)
                    {
                        fileStream.Indent++;
                    }

                    var lineBuilder = new StringBuilder();
                    lineBuilder.Append(macroDefinition.Name);
                    lineBuilder.Append(" ");
                    foreach (var line in macroDefinition.FxDefGroup)
                    {
                        string stringValue = line switch
                        {
                            DataDefinitionMacroLine primitiveMacroLine => primitiveMacroLine.Deserialize(reader, this.macroDefinitionTable.SymbolTable),
                            _ => throw new NotSupportedException()
                        };

                        lineBuilder.Append(stringValue);
                        lineBuilder.Append(" ");
                    }

                    if (opcode == this.popObjOpcode)
                    {
                        fileStream.Indent--;
                    }

                    fileStream.WriteLine(lineBuilder);
                    fileStream.Flush();

                    lastOpcode = opcode;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Exception processing {name}, last opcode was {lastOpcode}. Trace: {e}");
            }

            fileStream.WriteLine("fxdone" + Environment.NewLine);
        }
    }
}
namespace Giants.EffectCompiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Giants.BinTools.Macro;
    using NLog;

    /// <summary>
    /// Effect binary compiler.
    /// </summary>
    public class FxCompiler
    {
        private static readonly Logger logger = LogManager.GetLogger(nameof(FxCompiler));

        private FxMacroDefinitionTable macroDefinitionTable;
        private int doneOpcode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FxCompiler"/> class.
        /// </summary>
        /// <param name="macroDefinitionTable">The table of macro and symbol definitions.</param>
        public FxCompiler(FxMacroDefinitionTable macroDefinitionTable)
        {
            this.macroDefinitionTable = macroDefinitionTable;
        }

        /// <summary>
        /// Compiles a textual effect file to the specified output path.
        /// </summary>
        /// <param name="inputPath">The path to the effect file.</param>
        /// <param name="outputPath">The path to write to.</param>
        public void Compile(
            string inputPath,
            string outputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new InvalidOperationException($"The input file {inputPath} does not exist.");
            }

            // Get constants for known symbols
            this.doneOpcode = Utilities.GetFxSymbolValue(this.macroDefinitionTable.SymbolTable, "FxDone");

            using var streamReader = new StreamReader(inputPath);
            SerializedEffectData serializedEffectData = this.SerializeEffectData(streamReader);

            using var fileStream = new FileStream(outputPath, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);

            this.WriteHeader(binaryWriter, serializedEffectData);
            this.WriteTableOfContents(binaryWriter, serializedEffectData);
            this.WriteEffectData(binaryWriter, serializedEffectData);
        }

        private void WriteEffectData(BinaryWriter binaryWriter, SerializedEffectData serializedEffectData)
        {
            binaryWriter.Write(serializedEffectData.Data);
        }

        private void WriteTableOfContents(BinaryWriter binaryWriter, SerializedEffectData serializedEffectData)
        {
            foreach (var entry in serializedEffectData.TableOfContents)
            {
                binaryWriter.Write(Encoding.UTF8.GetBytes(entry.Name));
                binaryWriter.Write('\0');
                binaryWriter.Write(checked(entry.Offset + serializedEffectData.TableOfContentsSize));
            }
        }

        private void WriteHeader(BinaryWriter binaryWriter, SerializedEffectData serializedEffectData)
        {
            binaryWriter.Write(FxBinaryData.CurrentVersion);
            binaryWriter.Write(checked(serializedEffectData.Data.Length + serializedEffectData.TableOfContentsSize));
            binaryWriter.Write(serializedEffectData.TableOfContents.Count);
        }

        private void SerializeEffect(string[] tokens, StreamReader reader, BinaryWriter binaryWriter)
        {
            while (!reader.EndOfStream)
            {
                tokens = reader.ReadLine().Split(Utilities.SplitCharacters, StringSplitOptions.RemoveEmptyEntries);

                string macroName = tokens[0];
                if (macroName == "fxdone")
                {
                    binaryWriter.Write((byte)this.doneOpcode);
                    break;
                }

                FxMacroDefinition macroDefinition = this.macroDefinitionTable.MacroDefinitions
                    .Values
                    .FirstOrDefault(x => x.Name.Equals(macroName, StringComparison.OrdinalIgnoreCase) && x.FxDefGroup.Count() == tokens[1..].Length);

                binaryWriter.Write((byte)macroDefinition.Opcode);

                if (macroDefinition == null)
                {
                    throw new InvalidOperationException("Unknown macro '{macroName}'");
                }

                var parameters = new List<object>();
                int parameterIndex = 1;
                foreach (var line in macroDefinition.FxDefGroup)
                {
                    if (line is DataDefinitionMacroLine dataDefinitionMacroLine)
                    {
                        dataDefinitionMacroLine.Serialize(tokens[parameterIndex++], this.macroDefinitionTable.SymbolTable, binaryWriter);
                    }
                }
            }
        }

        private SerializedEffectData SerializeEffectData(StreamReader streamReader)
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);

            var tableOfContents = new List<ContentEntry>();
            int tableOfContentsSize = 0; // Keep running total of the final (serialized) size of the TOC

            while (!streamReader.EndOfStream)
            {
                string[] tokens = streamReader.ReadLine().Split(Utilities.SplitCharacters, StringSplitOptions.RemoveEmptyEntries);
                if (!tokens.Any())
                {
                    continue;
                }

                if (tokens[0] == "fxdef")
                {
                    var contentEntry = new ContentEntry()
                    {
                        Name = tokens[1],
                        Offset = checked((int)binaryWriter.BaseStream.Position)
                    };

                    tableOfContentsSize += contentEntry.Name.Length + 1 + sizeof(int);
                    tableOfContents.Add(contentEntry);

                    this.SerializeEffect(tokens, streamReader, binaryWriter);
                }
            }

            return new SerializedEffectData
            {
                Data = memoryStream.ToArray(),
                TableOfContents = tableOfContents,
                TableOfContentsSize = tableOfContentsSize,
            };
        }
    }
}

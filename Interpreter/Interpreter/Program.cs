using Antlr4.Runtime;
using System;
using System.IO;
using System.Collections.Generic;

namespace Interpreter
{
    internal class Program
    {
        private static MiniLanguageLexer SetupLexer(string text, string lexerErrorFilePath)
        {
            AntlrInputStream inputStream = new AntlrInputStream(text);
            MiniLanguageLexer lexer = new MiniLanguageLexer(inputStream);
            if (File.Exists(lexerErrorFilePath))
                File.WriteAllText(lexerErrorFilePath, string.Empty);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LexicalErrorListener(lexerErrorFilePath));
            return lexer;
        }

        private static MiniLanguageParser SetupParser(MiniLanguageLexer lexer, string parserErrorFilePath)
        {
            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);
            MiniLanguageParser parser = new MiniLanguageParser(commonTokenStream);
            if (File.Exists(parserErrorFilePath))
                File.WriteAllText(parserErrorFilePath, string.Empty);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new SyntaxErrorListener(parserErrorFilePath));
            return parser;
        }

        private static void PrintLexemes(MiniLanguageLexer lexer, string outputFilePath)
        {
            IEnumerable<IToken> tokens = lexer.GetAllTokens();
            lexer.Reset();
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (var token in tokens)
                {
                    writer.WriteLine($"<{lexer.Vocabulary.GetSymbolicName(token.Type)}, '{token.Text}', {token.Line}>");
                }
            }
        }
        private static void EvaluateProgram(MiniLanguageParser parser)
        {
            MiniLanguageParser.ProgramContext programContext = parser.program();

            MyVisitor visitor = new MyVisitor();
            visitor.Visit(programContext);
            parser.Reset();
            visitor.PrintGlobalVariables("../../globalVariables.txt");
            visitor.CallMain();
            visitor.PrintFunctionDetails("../../functionDetails.txt");
            Console.WriteLine("Execution complete.");
        }
        

        static void Main(string[] args)
        {
            string inputFile = File.ReadAllText("../../input.txt");
            string lexemsFile = "../../lexeme.txt";
            string lexerErrors = "../../lexerErrors.txt";
            string parserErrors = "../../parserErrors.txt";

            MiniLanguageLexer lexer = SetupLexer(inputFile, lexerErrors);
            PrintLexemes(lexer, lexemsFile);

            MiniLanguageParser parser = SetupParser(lexer, parserErrors);
            EvaluateProgram(parser);
        }
    }
}

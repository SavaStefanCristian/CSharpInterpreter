using Antlr4.Runtime;
using System.IO;

public class LexicalErrorListener : IAntlrErrorListener<int>
{
    private readonly string _errorFilePath;

    public LexicalErrorListener(string errorFilePath)
    {
        _errorFilePath = errorFilePath;
    }

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        WriteError($"Lexical error at line {line}:{charPositionInLine} - {msg}");
    }

    private void WriteError(string error)
    {
        using (StreamWriter writer = new StreamWriter(_errorFilePath, append: true))
        {
            writer.WriteLine(error);
        }
    }
}

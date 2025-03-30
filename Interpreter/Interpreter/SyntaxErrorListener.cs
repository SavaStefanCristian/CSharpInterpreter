using Antlr4.Runtime;
using System.IO;

public class SyntaxErrorListener : IAntlrErrorListener<IToken>
{
    private readonly string _errorFilePath;

    public SyntaxErrorListener(string errorFilePath)
    {
        _errorFilePath = errorFilePath;
    }

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        WriteError($"Syntax error at line {line}:{charPositionInLine} - {msg}");
    }

    private void WriteError(string error)
    {
        using (StreamWriter writer = new StreamWriter(_errorFilePath, append: true))
        {
            writer.WriteLine(error);
        }
    }
}

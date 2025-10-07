
namespace Itp.SimpleHL7.Tests;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void Lexer_BasicMessage_TokensCorrect()
    {
        var lexer = new HL7Lexer("MSH|^~\\&|EX1|ABC\rPID||a^b|00~10^11~20^21&22\r\n");

        Assert.AreEqual(HL7TokenType.StartOfMessage, lexer.Type);
        Assert.AreEqual("MSH|^~\\&", lexer.Value.ToString());
        lexer.AssertNext(HL7TokenType.FieldSeparator);
        lexer.AssertNext(HL7TokenType.Value, "EX1");
        lexer.AssertNext(HL7TokenType.FieldSeparator);
        lexer.AssertNext(HL7TokenType.Value, "ABC");
        lexer.AssertNext(HL7TokenType.EndOfSegment, "\r");
        lexer.AssertNext(HL7TokenType.Value, "PID");
        lexer.AssertNext(HL7TokenType.FieldSeparator);
        lexer.AssertNext(HL7TokenType.FieldSeparator);
        lexer.AssertNext(HL7TokenType.Value, "a");
        lexer.AssertNext(HL7TokenType.ComponentSeparator);
        lexer.AssertNext(HL7TokenType.Value, "b");
        lexer.AssertNext(HL7TokenType.FieldSeparator);
        lexer.AssertNext(HL7TokenType.Value, "00");
        lexer.AssertNext(HL7TokenType.RepeatSeparator);
        lexer.AssertNext(HL7TokenType.Value, "10");
        lexer.AssertNext(HL7TokenType.ComponentSeparator);
        lexer.AssertNext(HL7TokenType.Value, "11");
        lexer.AssertNext(HL7TokenType.RepeatSeparator);
        lexer.AssertNext(HL7TokenType.Value, "20");
        lexer.AssertNext(HL7TokenType.ComponentSeparator);
        lexer.AssertNext(HL7TokenType.Value, "21");
        lexer.AssertNext(HL7TokenType.SubcomponentSeparator);
        lexer.AssertNext(HL7TokenType.Value, "22");
        lexer.AssertNext(HL7TokenType.EndOfSegment, "\r\n");
        lexer.AssertNext(HL7TokenType.EndOfMessage);
        Assert.IsFalse(lexer.MoveNext());
    }

    [TestMethod]
    public void Lexer_MoveToField()
    {
        var lexer = new HL7Lexer("MSH|^~\\&|EX1^ABC|123\rPID||a^b|00~10^11~20^21&22\r\n");

        Assert.IsTrue(lexer.MoveToNextField());
        lexer.AssertNext(HL7TokenType.Value, "EX1");
        Assert.IsTrue(lexer.MoveToNextField());
        lexer.AssertNext(HL7TokenType.Value, "123");
    }

    [TestMethod]
    public void Lexer_MoveToSegment()
    {
        var lexer = new HL7Lexer("MSH|^~\\&|EX1^ABC|123\rPID||a^b|00~10^11~20^21&22");
        Assert.IsTrue(lexer.MoveToNextSegment());
        lexer.AssertNext(HL7TokenType.Value, "PID");
        Assert.IsFalse(lexer.MoveToNextSegment());
        Assert.AreEqual(HL7TokenType.EndOfMessage, lexer.Type);
    }
}

internal static class HL7LexerExtensions
{
    public static void AssertNext(this ref HL7Lexer lexer, HL7TokenType tokenType, string expectedValue)
    {
        Assert.IsTrue(lexer.MoveNext());
        Assert.AreEqual(tokenType, lexer.Type);
        Assert.AreEqual(expectedValue, lexer.Value.ToString());
    }

    public static void AssertNext(this ref HL7Lexer lexer, HL7TokenType tokenType)
    {
        Assert.IsTrue(lexer.MoveNext());
        Assert.AreEqual(tokenType, lexer.Type);
    }
}

namespace Itp.SimpleHL7.Tests;

using System.Text;

[TestClass]
public sealed class EscapeTests
{
    private static readonly HL7Separators DefaultSeparators = new('|', '^', '&', '~', '\\');

    [TestMethod]
    public void EncodeString_NoSeparators_ReturnsOriginal()
    {
        var input = "Hello World";
        var result = DefaultSeparators.EncodeString(input);
        
        Assert.AreEqual(input, result.ToString());
    }

    [TestMethod]
    public void EncodeString_FieldSeparator_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("field|separator");
        Assert.AreEqual("field\\F\\separator", result.ToString());
    }

    [TestMethod]
    public void EncodeString_ComponentSeparator_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("comp^separator");
        Assert.AreEqual("comp\\S\\separator", result.ToString());
    }

    [TestMethod]
    public void EncodeString_SubcomponentSeparator_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("sub&component");
        Assert.AreEqual("sub\\T\\component", result.ToString());
    }

    [TestMethod]
    public void EncodeString_RepeatSeparator_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("repeat~separator");
        Assert.AreEqual("repeat\\R\\separator", result.ToString());
    }

    [TestMethod]
    public void EncodeString_EscapeSeparator_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("escape\\character");
        Assert.AreEqual("escape\\E\\character", result.ToString());
    }

    [TestMethod]
    public void EncodeString_CarriageReturn_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("line\rbreak");
        Assert.AreEqual("line\\.br\\break", result.ToString());
    }

    [TestMethod]
    public void EncodeString_Newline_EncodesCorrectly()
    {
        var result = DefaultSeparators.EncodeString("line\nbreak");
        Assert.AreEqual("line\\.br\\break", result.ToString());
    }

    [TestMethod]
    public void EncodeString_CarriageReturnNewline_EncodesAsOne()
    {
        var result = DefaultSeparators.EncodeString("line\r\nbreak");
        Assert.AreEqual("line\\.br\\break", result.ToString());
    }

    [TestMethod]
    public void EncodeString_MultipleSeparators_EncodesAll()
    {
        var result = DefaultSeparators.EncodeString("field|comp^sub&rep~esc\\");
        Assert.AreEqual("field\\F\\comp\\S\\sub\\T\\rep\\R\\esc\\E\\", result.ToString());
    }

    [TestMethod]
    public void DecodeString_NoEscapes_ReturnsOriginal()
    {
        var input = "Hello World";
        var result = DefaultSeparators.DecodeString(input, Encoding.UTF8);
        
        Assert.AreEqual(input, result.ToString());
    }

    [TestMethod]
    public void DecodeString_FieldEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("field\\F\\separator", Encoding.UTF8);
        Assert.AreEqual("field|separator", result.ToString());
    }

    [TestMethod]
    public void DecodeString_ComponentEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("comp\\S\\separator", Encoding.UTF8);
        Assert.AreEqual("comp^separator", result.ToString());
    }

    [TestMethod]
    public void DecodeString_SubcomponentEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("sub\\T\\component", Encoding.UTF8);
        Assert.AreEqual("sub&component", result.ToString());
    }

    [TestMethod]
    public void DecodeString_RepeatEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("repeat\\R\\separator", Encoding.UTF8);
        Assert.AreEqual("repeat~separator", result.ToString());
    }

    [TestMethod]
    public void DecodeString_EscapeEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("escape\\E\\character", Encoding.UTF8);
        Assert.AreEqual("escape\\character", result.ToString());
    }

    [TestMethod]
    public void DecodeString_LineBreakEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("line\\.br\\break", Encoding.UTF8);
        Assert.AreEqual($"line{Environment.NewLine}break", result.ToString());
    }

    [TestMethod]
    public void DecodeString_HexEscape_DecodesCorrectly()
    {
        var result = DefaultSeparators.DecodeString("test\\X41\\end", Encoding.UTF8);
        Assert.AreEqual("testAend", result.ToString());
    }

    [TestMethod]
    public void DecodeString_MultipleEscapes_DecodesAll()
    {
        var result = DefaultSeparators.DecodeString("field\\F\\comp\\S\\sub\\T\\rep\\R\\esc\\E\\", Encoding.UTF8);
        Assert.AreEqual("field|comp^sub&rep~esc\\", result.ToString());
    }

    [TestMethod]
    public void EncodeDecodeRoundTrip_PreservesOriginal()
    {
        var original = "field|comp^sub&rep~esc\\\r\ntest";
        var encoded = DefaultSeparators.EncodeString(original);
        var decoded = DefaultSeparators.DecodeString(encoded, Encoding.UTF8);
        
        Assert.AreEqual(original, decoded.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void DecodeString_MissingClosingEscape_ThrowsException()
    {
        DefaultSeparators.DecodeString("test\\F", Encoding.UTF8);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void DecodeString_InvalidEscapeCode1_ThrowsException()
    {
        DefaultSeparators.DecodeString("test\\Z\\end", Encoding.UTF8);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void DecodeString_InvalidEscapeCode2_ThrowsException()
    {
        DefaultSeparators.DecodeString(@"t\r", Encoding.UTF8);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void DecodeString_InvalidEscapeCode3_ThrowsException()
    {
        DefaultSeparators.DecodeString(@"t\rasdf\asdf", Encoding.UTF8);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void DecodeString_InvalidHexEscape_ThrowsException()
    {
        DefaultSeparators.DecodeString("test\\XZZ\\end", Encoding.UTF8);
    }

    [TestMethod]
    public void DecodeString_UnknownDotCommand_ReturnsAsIs()
    {
        var result = DefaultSeparators.DecodeString("test\\.xy\\end", Encoding.UTF8);
        Assert.AreEqual("test\\.xy\\end", result.ToString());
    }
}

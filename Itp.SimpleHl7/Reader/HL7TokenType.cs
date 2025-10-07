namespace Itp.SimpleHL7;

public enum HL7TokenType
{
    StartOfMessage,
    EndOfMessage,
    FieldSeparator,
    ComponentSeparator,
    SubcomponentSeparator,
    RepeatSeparator,
    EndOfSegment,
    Value,
}

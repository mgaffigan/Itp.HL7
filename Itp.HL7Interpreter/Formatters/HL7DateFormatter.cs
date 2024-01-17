namespace Itp.HL7Interpreter.Formatters;

public class HL7DateFormatter : IFormatter
{
    public static IFormatter Instance { get; } = new HL7DateFormatter();

    public object Decode(string input)
    {
        if (input.Length < 4 /*|| input.Length == 4 ? false : input.Length != 6)*/)
        {
            throw new ArgumentOutOfRangeException("Date must be 4 characters");
        }
        int num = 1;
        int num1 = 1;
        int num2 = int.Parse(input.Substring(0, 4));
        if (input.Length > 4)
        {
            num = int.Parse(input.Substring(4, 2));
        }
        if (input.Length > 6)
        {
            num1 = int.Parse(input.Substring(6, 2));
        }
        DateTime dateTime = new DateTime(num2, num, num1);
        if (dateTime < new DateTime(1800, 1, 1))
        {
            dateTime = new DateTime();
        }
        return dateTime;
    }

    public string Encode(object input)
    {
        if (input is DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }
        else if (input is DateTimeOffset dto)
        {
            return dto.ToString("yyyyMMdd");
        }
        else
        {
            return input.ToString();
        }
    }
}
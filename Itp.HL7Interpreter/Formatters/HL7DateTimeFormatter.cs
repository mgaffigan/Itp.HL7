namespace Itp.HL7Interpreter.Formatters;

public class HL7DateTimeFormatter : IFormatter
{
    public static IFormatter Instance { get; } = new HL7DateTimeFormatter();

    public object Decode(string value)
    {
        int iRep = value.IndexOf('\u005E');
        if (iRep > 0)
        {
            value = value.Substring(0, iRep);
        }
        int year = int.Parse(value.Substring(0, 4));
        int month = 1, day = 1, hour = 0, minute = 0, second = 0, ms = 0, offsetHours = 0;
        if (value.Length > 4)
        {
            month = int.Parse(value.Substring(4, 2));
        }
        if (value.Length > 6)
        {
            day = int.Parse(value.Substring(6, 2));
        }
        if (value.Length > 8)
        {
            hour = int.Parse(value.Substring(8, 2));
            minute = int.Parse(value.Substring(10, 2));
        }
        if (value.Length > 12)
        {
            second = int.Parse(value.Substring(12, 2));
        }
        if (value.Length > 14)
        {
            string str = value.Substring(14).Replace(".", "");
            int num9 = str.IndexOf('+');
            if (num9 < 0)
            {
                num9 = str.IndexOf('-');
            }
            if (num9 >= 0)
            {
                offsetHours = int.Parse(str.Substring(num9, 3));
                str = str.Substring(0, num9);
            }
            if (str.Length > 0)
            {
                ms = (int)((double)int.Parse(str) * Math.Pow(10, (double)(3 - str.Length)));
            }
        }
        var result = new DateTimeOffset(year, month, day, hour, minute, second, ms, new TimeSpan(offsetHours, 0, 0));
        if (result < new DateTimeOffset(1800, 1, 1, 0, 0, 0, TimeSpan.Zero))
        {
            result = new DateTimeOffset();
        }
        return result;
    }

    public string Encode(object input)
    {
        string str;
        if (!(input is DateTime))
        {
            str = (!(input is DateTimeOffset) ? input.ToString() : ((DateTimeOffset)input).ToString("yyyyMMddHHmmss.ffffzz00"));
        }
        else
        {
            str = ((DateTime)input).ToUniversalTime().ToString("yyyyMMddHHmmss+0000");
        }
        return str;
    }
}
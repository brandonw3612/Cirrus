namespace Cirrus.Extensions;

public static class StringExtensions
{
    public static string TrimAdvanced(this string str)
    {
        var left = 0;
        while (left < str.Length)
        {
            if (str[left] == ' ' || str[left] == '\n' || str[left] == '\r') left++;
            else break;
        }
        if (left == str.Length) return string.Empty;
        var right = str.Length - 1;
        while (right > left)
        {
            if (str[right] == ' ' || str[right] == '\n' || str[right] == '\r') right--;
            else break;
        }
        return str[left..(right + 1)];
    }
}
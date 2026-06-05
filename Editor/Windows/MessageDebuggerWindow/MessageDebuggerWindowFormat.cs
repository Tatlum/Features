using ErmineGames.Features;
using ErmineGames.Utils;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace ErminGames.Features.Editor
{
    public partial class MessageDebuggerWindow
    {
        private static string FormatRecordName(JournalRecord record)
        {
            var typeName = record.DeclaringType;
            var ending = record.ContentType switch
            {
                JournalContentType.Message => "Message",
                JournalContentType.Request => "Request",
                _ => string.Empty
            };

            if (typeName.EndsWith(ending))
            {
                typeName = typeName[..typeName.LastIndexOf(ending, StringComparison.Ordinal)];
            }

            return ObjectNames.NicifyVariableName(typeName);
        }

        private static string FormatTime(DateTime time)
        {
            return time == default ? string.Empty : time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private static string FormatReflectedData(ReflectionUtils.ReflectedData data, bool isRichFormat)
        {
            var sb = new StringBuilder();
            FormatReflectedMember(data, isRichFormat, ref sb, 0);
            return sb.ToString();
        }

        private static void FormatReflectedMember(ReflectionUtils.ReflectedData data, bool isRichFormat,
            ref StringBuilder sb, int indent)
        {
            var indentString = new string(' ', indent * 4);
            sb.Append(indentString);
            sb.Append(WrapName(data.MemberName, data.MemberType, isRichFormat));

            if (data.MemberValueType == ReflectionUtils.ReflectedData.ReflectedMemberValueType.Simple)
            {
                sb.AppendLine($": {WrapValue(data.SimpleValue, isRichFormat)}");
            }
            else
            {
                sb.AppendLine();

                foreach (var subData in data.ComplexValue)
                {
                    FormatReflectedMember(subData, isRichFormat, ref sb, indent + 1);
                }
            }
        }

        private static string WrapName(string value, MemberTypes memberType, bool isRichFormat)
        {
            return isRichFormat && memberType == MemberTypes.TypeInfo ? $"<b>{value}</b>" : value;
        }

        private static string WrapValue(string value, bool isRichFormat)
        {
            return isRichFormat ? $"<b>{value}</b>" : value;
        }
    }
}

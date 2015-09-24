using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Konamiman.NestorBugs.CrossCutting.Misc
{
    public static class EnumExtensions
    {
        // CODE FROM: http://stackoverflow.com/questions/1415140/c-sharp-enums-can-my-enums-have-friendly-names/1415168#1415168
        public static string GetDisplayText(this Enum enumValue)
        {
            Type type = enumValue.GetType();

            MemberInfo[] memInfo = type.GetMember(enumValue.ToString());
            if(memInfo != null && memInfo.Length > 0) {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayTextAttribute), false);
                if(attrs != null && attrs.Length > 0) {
                    return ((DisplayTextAttribute)attrs[0]).Text;
                }
            }
            return enumValue.ToString();
        }
    }
}

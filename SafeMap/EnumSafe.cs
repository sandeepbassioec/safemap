using System;

namespace SafeMap
{
    /// <summary>
    /// EnumSafe
    /// This class solves a very common problem:
    /// Developers keep writing repeated code like:
    ///
    ///   Enum.TryParse<MyEnum>(value, true, out var result);
    ///
    /// And then again do null checks or fallback logic.
    ///
    /// This helper wraps all enum conversion into safe, reusable methods,
    /// fully compatible with SafeMapX pattern.
    /// </summary>
    public static class EnumSafe
    {
        public static SafeValueStruct<TEnum> Parse<TEnum>(string? value, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value)) 
                return new SafeValueStruct<TEnum>(null, false);
            
            if (Enum.TryParse<TEnum>(value.Trim(), ignoreCase, out var res)) 
                return new SafeValueStruct<TEnum>(res, true);
            
            return new SafeValueStruct<TEnum>(null, false);
        }

        public static SafeValueStruct<TEnum> Convert<TEnum>(object? raw)
            where TEnum : struct, Enum
        {
            if (raw == null) return new SafeValueStruct<TEnum>(null, false);
            
            try
            {
                var typeCode = Type.GetTypeCode(raw.GetType());
            
                if (typeCode == TypeCode.Byte || typeCode == TypeCode.Int16 || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64)
                {
                    var number = System.Convert.ToInt32(raw);
                
                    if (Enum.IsDefined(typeof(TEnum), number)) 
                        return new SafeValueStruct<TEnum>((TEnum)(object)number, true);
                    
                    return new SafeValueStruct<TEnum>(null, false);
                }

                return Parse<TEnum>(raw.ToString());
            }
            catch 
            { 
                return new SafeValueStruct<TEnum>(null, false); 
            }
        }

        public static SafeValueStruct<TEnum> FromInt<TEnum>(int value) where TEnum : struct, Enum
        {
            if (Enum.IsDefined(typeof(TEnum), value)) 
                return new SafeValueStruct<TEnum>((TEnum)(object)value, true);
            
            return new SafeValueStruct<TEnum>(null, false);
        }
    }
}
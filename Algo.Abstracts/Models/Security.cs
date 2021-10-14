using System;

namespace Algo.Abstracts.Models
{
    public class Security
    {
        public Security(string code, string @class)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Value cannot be null or empty.", nameof(code));
            if (string.IsNullOrEmpty(@class))
                throw new ArgumentException("Value cannot be null or empty.", nameof(@class));

            Code = code;
            Class = @class;
        }

        public string Code { get; }
        public string Class { get; }
        public decimal LotSize { get; set; } = 1;

        public override string ToString()
        {
            return $"{Class}.{Code}";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Security;

            return other?.Code == Code && other?.Class == Class;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode() ^ Class.GetHashCode();
        }
    }
}
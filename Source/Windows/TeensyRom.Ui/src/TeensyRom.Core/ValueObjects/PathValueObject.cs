using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.ValueObjects
{
    public abstract class PathValueObject<T> : IEquatable<T>
    where T : PathValueObject<T>
    {
        public string Value { get; protected set; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value) || Value == "." || Value == "..";

        protected PathValueObject(string path)
        {  
            Value = path;
        }

        public abstract bool Equals(T? other);
        public abstract override bool Equals(object? obj);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }
}

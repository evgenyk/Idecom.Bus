using System;

namespace Idecom.Bus.Utility
{
    public class ShortGuid
    {
        public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

        private Guid _guid;
        private string _value;

        public ShortGuid(string value)
        {
            _value = value;
            _guid = Decode(value);
        }

        public ShortGuid(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value == _guid) return;
                _guid = value;
                _value = Encode(value);
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                _guid = Decode(value);
            }
        }


        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShortGuid)
                return _guid.Equals(((ShortGuid) obj)._guid);
            if (obj is Guid)
                return _guid.Equals((Guid) obj);
            if (obj is string)
                return _guid.Equals(new ShortGuid((string) obj)._guid);
            return false;
        }

        public static bool TryParse(string guid, out ShortGuid shortGuid)
        {
            var parsed = Empty;
            try { parsed = new ShortGuid(guid); }
            catch
            {
                shortGuid = parsed;
                return false;
            }

            shortGuid = parsed;
            return true;
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        public static string Encode(string value)
        {
            var guid = new Guid(value);
            return Encode(guid);
        }

        public static string Encode(Guid guid)
        {
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded.Substring(0, 22);
        }

        public static Guid Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Guid.Empty;

            value = value.Replace("_", "/").Replace("-", "+");
            var buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            if ((object) x == null) return (object) y == null;
            return x._guid == y._guid;
        }

        public static bool operator !=(ShortGuid x, ShortGuid y)
        {
            return !(x == y);
        }

        public static implicit operator string(ShortGuid shortGuid)
        {
            return shortGuid._value;
        }

        public static implicit operator Guid(ShortGuid shortGuid)
        {
            return shortGuid._guid;
        }

        public static implicit operator ShortGuid(string shortGuid)
        {
            return new ShortGuid(shortGuid);
        }

        public static implicit operator ShortGuid(Guid guid)
        {
            return new ShortGuid(guid);
        }
    }
}
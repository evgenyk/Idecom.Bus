using System;
using System.Net;

namespace Idecom.Bus.Addressing
{
    [Serializable]
    public class Address
    {
        private readonly string _datacenterLowerCased;
        private readonly string _queueLowerCased;

        public Address(string queueName, string datacenter = null)
        {
            Queue = queueName;
            _queueLowerCased = queueName.ToLower();
            Datacenter = datacenter;
            if (Datacenter != null) _datacenterLowerCased = Datacenter.ToLower();
        }

        public Address(string queueName) : this(queueName, null)
        {
        }

        private bool LocalDatacenter
        {
            get { return Datacenter == null; }
        }

        public string Datacenter { get; private set; }
        public string Queue { get; private set; }

        public static Address Parse(string destination)
        {
            if (string.IsNullOrEmpty(destination))
            {
                throw new ArgumentException("Invalid destination address specified", "destination");
            }

            string[] arr = destination.Split('@');

            string queue = arr[0];
            string datacenter = null;

            if (String.IsNullOrWhiteSpace(queue))
            {
                throw new ArgumentException("Invalid destination address specified", "destination");
            }

            if (arr.Length != 2) return new Address(queue);
            if (arr[1] != "." && arr[1].ToLower() != "localhost" && arr[1] != IPAddress.Loopback.ToString())
                datacenter = arr[1];

            return new Address(queue, datacenter);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return ((_queueLowerCased != null ? _queueLowerCased.GetHashCode() : 0)*397) ^ (_datacenterLowerCased != null ? _datacenterLowerCased.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            if (LocalDatacenter)
                return Queue;

            return Queue + "@" + Datacenter;
        }

        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Address)) return false;
            return Equals((Address) obj);
        }

        private bool Equals(Address other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (!LocalDatacenter && !other._datacenterLowerCased.Equals(_datacenterLowerCased))
                return false;

            return other._queueLowerCased.Equals(_queueLowerCased);
        }
    }
}
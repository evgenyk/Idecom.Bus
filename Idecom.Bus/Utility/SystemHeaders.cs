namespace Idecom.Bus.Utility
{
    using System;

    public static class SystemHeaders
    {
        public const string SagaIdPrefix = "_SAGAID_";

        public static string SagaIdHeaderKey(Type sagaDatatype)
        {
            var sagaId = string.Format("{0}{1}", SagaIdPrefix, sagaDatatype).ToUpperInvariant();
            return sagaId;
        }

        internal class CallContext
        {
            internal const string AmbientContext = "_amb_ctxt_";
            internal const string ParentContext = "_amb_ctxt_";
        }
    }
}
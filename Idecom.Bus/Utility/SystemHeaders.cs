namespace Idecom.Bus.Utility
{
    using System;

    public static class SystemHeaders
    {
        public const string SagaIdPrefix = "_SAGAID_";

        /// <summary>
        ///     TODO: Implement saga data name override with an attribute
        /// </summary>
        /// <param name="sagaDatatype"></param>
        /// <returns></returns>
        public static string SagaIdHeaderKey(Type sagaDatatype)
        {
            var sagaId = string.Format("{0}{1}", SagaIdPrefix, sagaDatatype).ToUpperInvariant();
            return sagaId;
        }
    }
}
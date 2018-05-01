using Echoes.Data.Contextual;
using Echoes.Data.Entity;
using Echoes.DataStructure.Contextual;
using Echoes.DataStructure.Entity;
using System;

namespace Echoes.DataAccess
{
    public static class DataFactory
    {
        /// <summary>
        /// Create an intstance of an interface for entities
        /// </summary>
        /// <typeparam name="T">The interface to create an instance of</typeparam>
        /// <returns>null for non-interfaces, a new instance of the interface for actual interfaces</returns>
        public static T Create<T>() where T : class
        {
            var TType = typeof(T);

            if (!TType.IsInterface)
                return default(T);

            if (TType == typeof(IThing))
                return new Thing() as T;

            if (TType == typeof(IPersona))
                return new Persona() as T;

            if (TType == typeof(IPlace))
                return new Place() as T;

            if (TType == typeof(IAkashicEntry))
                return new AkashicEntry() as T;

            if (TType == typeof(IDescriptor))
                return new Descriptor() as T;

            if (TType == typeof(IVerb))
                return new Verb() as T;


            throw new InvalidOperationException();
        }
    }
}

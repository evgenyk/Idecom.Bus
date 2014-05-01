namespace Idecom.Bus.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class InterfaceImplementor
    {
        private static readonly Dictionary<Type, Type> ImplementationCache = new Dictionary<Type, Type>();
        private static readonly object SyncRoot = new object();

        private static readonly ModuleBuilder ModuleBuilder;

        static InterfaceImplementor()
        {
            
            lock (SyncRoot)
            {
                if (ModuleBuilder != null) return;

                var assemblyName = new AssemblyName(String.Format("GeneratedInterfaceImplementation_{0}", Guid.NewGuid().ToString("N")));
                var appDomain = AppDomain.CurrentDomain;
                var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                ModuleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            }
        }

        public static Type ImplementInterface(Type @interface)
        {
            if (!@interface.IsInterface)
                throw new Exception(string.Format("Could not implement {0} as it is not an interface", @interface.Name));

            if (ImplementationCache.ContainsKey(@interface)) { return ImplementationCache[@interface]; }

            var typeBuilder = ModuleBuilder.DefineType(string.Format("{0}_{1}", @interface.Name, Guid.NewGuid().ToString("N")), TypeAttributes.Serializable | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);

            typeBuilder.AddInterfaceImplementation(@interface);

            var props = GetAllProperties(@interface);
            foreach (PropertyInfo prop in props)
                DefineProperty(typeBuilder, prop);

            var newType = typeBuilder.CreateType();

            lock (SyncRoot)
                ImplementationCache.Add(@interface, newType);

            return newType;
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            var allPropertiesWithInheritance = new List<PropertyInfo>(type.GetProperties());
            foreach (Type interfaceType in type.GetInterfaces())
                allPropertiesWithInheritance.AddRange(GetAllProperties(interfaceType));

            var result = allPropertiesWithInheritance.Select(x => x.Name).Distinct().Select(x => allPropertiesWithInheritance.First(y => y.Name.Equals(x)));
            return result;
        }

        private static void DefineProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.GetMethod.ReturnType;

            var field = typeBuilder.DefineField(String.Format("_{0}", propertyName), typeof(string), FieldAttributes.Private);
            var prop = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, typeof(string), null);
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual;


            //Getter
            var get = typeBuilder.DefineMethod(String.Format("get_{0}", propertyName), methodAttributes, propertyType, Type.EmptyTypes);
            var getGen = get.GetILGenerator();
            getGen.Emit(OpCodes.Ldarg_0);
            getGen.Emit(OpCodes.Ldfld, field);
            getGen.Emit(OpCodes.Ret);

            //Setter
            var getterName = String.Format("set_{0}", propertyName);
            var set = typeBuilder.DefineMethod(getterName, methodAttributes, null, new[] { propertyType });

            var setGen = set.GetILGenerator();
            setGen.Emit(OpCodes.Ldarg_0);
            setGen.Emit(OpCodes.Ldarg_1);
            setGen.Emit(OpCodes.Stfld, field);
            setGen.Emit(OpCodes.Ret);

            prop.SetGetMethod(get);
            prop.SetSetMethod(set);
        }
    }
}
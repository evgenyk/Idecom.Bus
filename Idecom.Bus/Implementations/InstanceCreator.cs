namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Interfaces;

    public class InstanceCreator : IInstanceCreator
    {
        public object CreateInstanceOf(Type type)
        {
            var newType = type;

            if (type.IsInterface)
                newType = ImplementInterface(type);

            var result = Activator.CreateInstance(newType);
            return result;
        }

        public T CreateInstanceOf<T>()
        {
            return (T) CreateInstanceOf(typeof (T));
        }

        private Type ImplementInterface(Type type)
        {
            var assemblyName = new AssemblyName(string.Format("GeneratedInterfaceImplementation_{0}", Guid.NewGuid().ToString("N")));
            var appDomain = AppDomain.CurrentDomain;
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType("AmAnInterface2", TypeAttributes.Serializable | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);

            typeBuilder.AddInterfaceImplementation(type);

            var props = GetAllProperties(type);
            foreach (PropertyInfo prop in props)
                DefineProperty(typeBuilder, prop);

            var newType = typeBuilder.CreateType();
            return newType;
        }

        private IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            var allPropertiesWithInheritance = new List<PropertyInfo>(type.GetProperties());
            foreach (Type interfaceType in type.GetInterfaces())
                allPropertiesWithInheritance.AddRange(GetAllProperties(interfaceType));

            var result = allPropertiesWithInheritance.Select(x => x.Name).Distinct().Select(x => allPropertiesWithInheritance.First(y => y.Name.Equals(x)));
            return result;
        }

        private void DefineProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            var propertyType = propertyInfo.GetMethod.ReturnType;

            var field = typeBuilder.DefineField(string.Format("_{0}", propertyName), typeof (string), FieldAttributes.Private);
            var prop = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, typeof (string), null);
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual;


            //Getter
            var get = typeBuilder.DefineMethod(string.Format("get_{0}", propertyName), methodAttributes, propertyType, Type.EmptyTypes);
            var getGen = get.GetILGenerator();
            getGen.Emit(OpCodes.Ldarg_0);
            getGen.Emit(OpCodes.Ldfld, field);
            getGen.Emit(OpCodes.Ret);

            //Setter
            var getterName = string.Format("set_{0}", propertyName);
            var set = typeBuilder.DefineMethod(getterName, methodAttributes, null, new[] {propertyType});

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
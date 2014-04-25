using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Idecom.Bus.Interfaces;

namespace Idecom.Bus.Implementations
{
    public class InstanceCreator: IInstanceCreator
    {
        public object CreateInstanceOf(Type type)
        {
            var newType = type;

            if (type.IsInterface)
                newType = ImplementInterface(type);

            var result = Activator.CreateInstance(newType);
            return result;
        }

        private Type ImplementInterface(Type type)
        {
            var assemblyName = new AssemblyName(string.Format("GeneratedInterfaceImplementation_{0}", Guid.NewGuid().ToString("N")));
            AppDomain appDomain = AppDomain.CurrentDomain;
            AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            TypeBuilder typeBuilder = moduleBuilder.DefineType("AmAnInterface2", TypeAttributes.Serializable | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);

            typeBuilder.AddInterfaceImplementation(type);

            IEnumerable<PropertyInfo> props = GetAllProperties(type);
            foreach (PropertyInfo prop in props)
                DefineProperty(typeBuilder, prop);

            Type newType = typeBuilder.CreateType();
            return newType;
        }

        public T CreateInstanceOf<T>()
        {
            return (T) CreateInstanceOf(typeof (T));
        }

        private IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            var allPropertiesWithInheritance = new List<PropertyInfo>(type.GetProperties());
            foreach (Type interfaceType in type.GetInterfaces())
                allPropertiesWithInheritance.AddRange(GetAllProperties(interfaceType));

            IEnumerable<PropertyInfo> result = allPropertiesWithInheritance.Select(x => x.Name).Distinct().Select(x => allPropertiesWithInheritance.First(y => y.Name.Equals(x)));
            return result;
        }

        private void DefineProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            string propertyName = propertyInfo.Name;
            Type propertyType = propertyInfo.GetMethod.ReturnType;

            FieldBuilder field = typeBuilder.DefineField(string.Format("_{0}", propertyName), typeof (string), FieldAttributes.Private);
            PropertyBuilder prop = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, typeof (string), null);
            const MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual;


            //Getter
            MethodBuilder get = typeBuilder.DefineMethod(string.Format("get_{0}", propertyName), methodAttributes, propertyType, Type.EmptyTypes);
            ILGenerator getGen = get.GetILGenerator();
            getGen.Emit(OpCodes.Ldarg_0);
            getGen.Emit(OpCodes.Ldfld, field);
            getGen.Emit(OpCodes.Ret);

            //Setter
            string getterName = string.Format("set_{0}", propertyName);
            MethodBuilder set = typeBuilder.DefineMethod(getterName, methodAttributes, null, new[] {propertyType});

            ILGenerator setGen = set.GetILGenerator();
            setGen.Emit(OpCodes.Ldarg_0);
            setGen.Emit(OpCodes.Ldarg_1);
            setGen.Emit(OpCodes.Stfld, field);
            setGen.Emit(OpCodes.Ret);

            prop.SetGetMethod(get);
            prop.SetSetMethod(set);
        }
    }
}
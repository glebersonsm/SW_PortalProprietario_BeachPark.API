using NHibernate.Proxy;
using SW_PortalProprietario.Domain.Entities.Core;
using System.Reflection;

namespace SW_Utils.Extensions
{
    public static class ReflectionExtension
    {
        public static Type FieldOrPropertyType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Property:
                    {
                        return ((PropertyInfo)info).PropertyType;
                    }
                case MemberTypes.Field:
                    {
                        return ((FieldInfo)info).FieldType;
                    }
                default:
                    throw new Exception($"Member Type {info.MemberType} inesperado");
            }
        }

        public static object? FieldOrPropertyValueDefault(this MemberInfo info, object instance)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Property:
                    {
                        var result = ((PropertyInfo)info).GetValue(instance, new object[0]);
                        if (result is not null)
                        {
                            if (result is INHibernateProxy proxy)
                            {
                                var baseResult = proxy.GetType().GetProperties().FirstOrDefault(c =>
                                c.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase));
                                if (baseResult is not null)
                                {
                                    result = baseResult.GetValue(result);
                                }
                            }
                            else if (result is EntityBaseCore entityBase)
                            {
                                result = entityBase.Id;
                            }

                        }
                        return result;
                    }
                case MemberTypes.Field:
                    {
                        var result = ((FieldInfo)info).GetValue(instance);
                        if (result is not null)
                        {
                            if (result is INHibernateProxy proxy)
                            {
                                var baseResult = proxy.GetType().GetProperties().FirstOrDefault(c =>
                                c.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase));
                                if (baseResult is not null)
                                {
                                    result = baseResult.GetValue(result);
                                }
                            }
                            else if (result is EntityBaseCore entityBase)
                            {
                                result = entityBase.Id;
                            }
                        }
                        return result;
                    }
                default:
                    throw new Exception($"Member Type {info.MemberType} inesperado");
            }
        }

        public static object? FieldOrPropertyItemType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Property:
                    {
                        var result = ((PropertyInfo)info).PropertyType.GenericTypeArguments.Any() ? $"{((PropertyInfo)info).PropertyType.GenericTypeArguments.First()}" : ((PropertyInfo)info).PropertyType.AssemblyQualifiedName;
                        return result;
                    }
                case MemberTypes.Field:
                    {
                        var result = ((FieldInfo)info).FieldType.GenericTypeArguments.Any() ? $"{((FieldInfo)info).FieldType.GenericTypeArguments.First()}" : ((FieldInfo)info).FieldType.AssemblyQualifiedName;
                        return result;
                    }
                default:
                    throw new Exception($"Member Type {info.MemberType} inesperado");
            }
        }
    }
}


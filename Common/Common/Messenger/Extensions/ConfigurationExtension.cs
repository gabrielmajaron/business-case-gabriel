using System;
using System.Linq.Expressions;
using Messenger.Extensions.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Messenger.Extensions
{
    public static class ConfigurationExtension
    {
        public static T GetSettings<T>(this IConfiguration configuration)
            where T : new()
        {
            try
            {
                return configuration.Get<T>();
            }
            catch (Exception ex)
            {
                throw new SecretsNotFoundException(ex); 
            }
        }
        
        public static string GetRequired<T>(this IConfiguration configuration, Expression<Func<T, string>> expression)
            where T : new()
        {
            (var name, var value) = GetNameAndValue(configuration, expression);

            var isMissing = value is string
                ? string.IsNullOrWhiteSpace(value as string)
                : value == null;

            if(isMissing)
                throw new MissingConfigurationException(name);
            
            return value.ToString();
        }

        public static string GetOptional<T>(this IConfiguration configuration, Expression<Func<T, string>> expression)
            where T : new()
        {            
            (var name, var value) = GetNameAndValue(configuration, expression);
            
            return (string)value;
        }
        
        public static bool GetOptional<T>(this IConfiguration configuration, Expression<Func<T, bool>> expression)
            where T : new()
        {            
            (var name, var value) = GetNameAndValue(configuration, expression);
            
            return (bool)value;
        }

        private static (string, object) GetNameAndValue<T>(IConfiguration configuration, Expression<Func<T, string>> expression)
            where T : new()
        {
            var name = ((MemberExpression)expression.Body).Member.Name;
            var function = expression.Compile();
            var value = function(configuration.GetSettings<T>());

            return(name, value);
        }
        
        private static (string, object) GetNameAndValue<T>(IConfiguration configuration, Expression<Func<T, bool>> expression)
            where T : new()
        {
            var name = ((MemberExpression)expression.Body).Member.Name;
            var function = expression.Compile();
            var value = function(configuration.GetSettings<T>());

            return(name, value);
        }
    }
}
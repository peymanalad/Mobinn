using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.Extensions
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderByMember, bool ascending = true)
        {
            var type = typeof(T);
            var property = type.GetProperty(orderByMember);
            if (property == null)
            {
                // Handle the case where the property does not exist
                throw new ArgumentException($"Property {orderByMember} not found in {type}");
            }

            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var resultMethod = ascending ? "OrderBy" : "OrderByDescending";
            var orderByMethod = typeof(Queryable).GetMethods().Single(
                    method => method.Name == resultMethod &&
                              method.IsGenericMethodDefinition &&
                              method.GetGenericArguments().Length == 2 &&
                              method.GetParameters().Length == 2)
                .MakeGenericMethod(type, property.PropertyType);

            return (IOrderedQueryable<T>)orderByMethod.Invoke(null, new object[] { query, orderByExpression });
        }
    }
}

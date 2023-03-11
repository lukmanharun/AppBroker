using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Infrastructure.Services
{
    public static class DataGridQueryrable
    {
        #region Data Grid
        /// <summary>
        /// Data for grid data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Queryrable"></param>
        /// <param name="Field"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public static IQueryable<T> DataTableGridAsQueryrable<T>(this IQueryable<T> Queryrable, IFormCollection form) where T : class
        {
            var QueryrableResult = Queryrable;

            var searchValue = form["search[value]"].FirstOrDefault() ?? "".ToLower();
            var sortDirection = form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var indexSort = form["order[0][column]"].FirstOrDefault()??"0";
            var colSorting = form[$"columns[{indexSort}][data]"].FirstOrDefault();
            //var FormField = form["Field"].FirstOrDefault();
            //var Field = string.IsNullOrEmpty(FormField) ? new List<string>() : FormField.Split(",").ToList();
            //Field = new List<string> { "FirstName", "LastName" };
            var Field = typeof(T).GetProperties().Select(s => s.Name).ToList();
            var start = form["start"].FirstOrDefault() ?? "0";
            var length = form["length"].FirstOrDefault() ?? "5";

            var take = Convert.ToInt32(length);
            var skip = Convert.ToInt32(start);

            QueryrableResult = QueryrableResult.SearchGrid<T>(searchValue, Field);
            bool isDesc = (sortDirection is null ? "" : sortDirection.ToLower()) == "desc" ? true : false;
            QueryrableResult = QueryrableResult.OrderByProperty<T>(colSorting, isDesc);
            QueryrableResult = QueryrableResult.Take(take).Skip(skip);
            return QueryrableResult;
        }

        /// <summary>
        /// Searching data grid process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="searchValue"></param>
        /// <param name="Field"></param>
        /// <returns></returns>
        private static IQueryable<T> SearchGrid<T>(this IQueryable<T> source, string searchValue, List<string> Field)
        {
            searchValue = string.IsNullOrEmpty(searchValue) ? "" : searchValue.Trim().ToLower();
            var expressionConstant = Expression.Constant(searchValue);

            if (string.IsNullOrEmpty(searchValue)) return source;
            // T is a compile-time placeholder for the element type of the query.
            Type elementType = typeof(T);

            // Get all the string properties on this specific type.
            PropertyInfo[] stringProperties =
                elementType.GetProperties()
                    .Where(x => Field.Any(d => d == x.Name))
                    .ToArray();
            if (!stringProperties.Any()) { return source; }

            // Create a parameter for the expression tree:
            // the 'x' in 'x => x.PropertyName.Contains("term")'
            // The type of this parameter is the query's element type
            ParameterExpression parameter = Expression.Parameter(elementType);

            List<Expression> buildExpression = new List<Expression>();
            foreach (var item in Field)
            {
                if (stringProperties.Any(s => s.Name == item && s.PropertyType == typeof(string)))
                {
                    var contains = Expression.Call(Expression.PropertyOrField(parameter, item), "Contains", null, expressionConstant);
                    buildExpression.Add(contains);
                }
                else
                {
                    var contains = Expression.Call(Expression.Call(Expression.PropertyOrField(parameter, item), "ToString", null), "Contains", null, expressionConstant);
                    buildExpression.Add(contains);
                }
            }

            // Combine all the resultant expression nodes using ||
            Expression body = buildExpression
                .Aggregate(
                    (prev, current) => Expression.Or(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }

        /// <summary>
        /// Order Grid Data
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="Queryrable"></param>
        /// <param name="propertyName"></param>
        /// <param name="descending"></param>
        /// <param name="anotherLevel"></param>
        /// <returns></returns>
        private static IQueryable<TModel> OrderByProperty<TModel>(this IQueryable<TModel> Queryrable, string propertyName, bool descending, bool anotherLevel = false)
        {
            ParameterExpression param = Expression.Parameter(typeof(TModel), string.Empty); // I don't care about some naming
            MemberExpression property = Expression.PropertyOrField(param, propertyName);
            LambdaExpression sort = Expression.Lambda(property, param);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(TModel), property.Type },
                Queryrable.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<TModel>)Queryrable.Provider.CreateQuery<TModel>(call);
        }

        #endregion 

    }
}

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using DynamicExpresso;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Infrastructure.Services
{
    public static class DataGridQueryrable
    {
        #region Data Grid
        /// <summary>
        /// T is projection
        /// Queryrable should not in memory or already ToList/ToListAsync
        /// IgnorePropertySearch for ignore search property on server side
        /// TransformProperty for actual property on database Key is new projection property value is actual property in database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Queryrable"></param>
        /// <param name="form"></param>
        /// <param name="IgnorePropertySearch"></param>
        /// <param name="TransformProperty"></param>
        /// <returns></returns>
        public static async Task<GridDataTable<T>> DataTableGridServerSide<T>(this IQueryable<T> Queryrable
            , IFormCollection form,string IgnorePropertySearch = "",Dictionary<string,string>? TransformProperty = null ) where T : class
        {
            TransformProperty ??= new Dictionary<string,string>();
            var listIgnoreField = IgnorePropertySearch.Split(",");
            var searchValue = form["search[value]"].FirstOrDefault() ?? "".ToLower();
            var sortDirection = form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var indexSort = form["order[0][column]"].FirstOrDefault() ?? "0";
            var colSorting = form[$"columns[{indexSort}][data]"].FirstOrDefault()??"";
            var Field = typeof(T).GetProperties()
                .Where(s=> !listIgnoreField.Any(d=>d == s.Name) && !TransformProperty.Any(sd=>sd.Key == s.Name) )
                .Select(s => s.Name).ToList();
            var start = form["start"].FirstOrDefault() ?? "0";
            var length = form["length"].FirstOrDefault() ?? "5";
            var draw = form["draw"].FirstOrDefault() ?? "1";
            var take = Convert.ToInt32(length);
            var skip = Convert.ToInt32(start);

            if(!string.IsNullOrEmpty(searchValue)) Queryrable = Queryrable.SearchGrid<T>(searchValue, Field);
            
            IQueryable<T> queryOri = Queryrable;
            var rowCount = await queryOri.CountAsync();

            bool isDesc = (sortDirection is null ? "" : sortDirection.ToLower()) == "desc" ? true : false;
            if(!string.IsNullOrEmpty(colSorting))
            {
                if(TransformProperty != null) colSorting = TransformProperty.Where(s => s.Key.ToLower() == colSorting.ToLower())
                        .Select(s => s.Value).FirstOrDefault() ?? colSorting;
                Queryrable = Queryrable.OrderByProperty<T>(colSorting, isDesc);
            }

            Queryrable = Queryrable.Skip(skip).Take(take);
            var data = await Queryrable.ToListAsync();

            return new GridDataTable<T>
            {
                draw = Convert.ToInt16(draw),
                recordsTotal = data.Count(),
                recordsFiltered = rowCount,
                data = data
            };
        }
        private static Dictionary<string, int> DictionaryMonthENG()
        {
            return new Dictionary<string, int>
            {
                {"jan",1},
                {"feb",2},
                {"mar",3},
                {"apr",4},
                {"mey",5},
                {"jun",6},
                {"jul",7},
                {"aug",8},
                {"sep",9},
                {"okt",10},
                {"nov",11},
                {"dec",12}
            };
        }

        private static Expression? BuildExpressionSearchDate(ParameterExpression parameter,string propertyItem,string search)
        {
            List<Expression> buildExpression = new List<Expression>();
            bool IsValid = false;
            List<KeyValuePair<string, int>> listMonth = new List<KeyValuePair<string, int>>();
            int year = 0;
            int day = 0;
            int DayOrYear = 0;
            //Spliting search value
            var sr = search.Split(" ");
            if (sr.Count() > 3) return null;

            if(sr.Count()==3)
            {
                //Format should dd MMMM yyyy
                int.TryParse(sr[2], out year);
                int.TryParse(sr[0], out day);
                if (day == 0 || year == 0) return null;
                listMonth = DictionaryMonthENG().Where(s => s.Key.Contains(sr[1])).ToList();
                IsValid = listMonth.Count() == 1;
            }
            else if(sr.Count() == 2)
            {
                //Format should dd MMMM or MMMM yyyy
                int.TryParse(sr[0], out day);
                int.TryParse(sr[1], out year);
                if (day == 0 && year == 0) return null;
                listMonth = DictionaryMonthENG().Where(s => s.Key.ToLower() == sr[0].ToLower() || s.Key.Contains(sr[1])).ToList();
                IsValid = (listMonth.Where(s => s.Key.Contains(sr[0])).Count() == 1
                    || listMonth.Where(s => s.Key.Contains(sr[1])).Count() > 0)
                    ? true:false;
            }
            else if(sr.Count() == 1)
            {
                //Format should dd or MMMM or yyyy
                int.TryParse(sr[0], out DayOrYear);
                if (DayOrYear == 0) return null;
                listMonth = DictionaryMonthENG().Where(s => s.Key.Contains(sr[0])).ToList();
                IsValid = listMonth.Count() > 0;
            }
            if(!IsValid) return null;
            var param = Expression.Property(parameter, propertyItem);
            
            if(day > 0 && year > 0)
            {
                //dd MMMM yyy
                var resmonth = Expression.Equal(Expression.Property(param, "Month"), Expression.Constant(listMonth[0].Value));
                var resDay = Expression.Equal(Expression.Property(param, "Day"), Expression.Constant(day));
                var resYear = Expression.Equal(Expression.Property(param, "Year"), Expression.Constant(year));
                Expression res = new Expression[] 
                {
                    resDay,resmonth,resYear
                }.Aggregate((prev, current) => Expression.And(prev, current));
                return res;
            }
            List<BinaryExpression> listExpressionMonth = new List<BinaryExpression>();
            foreach (var item in listMonth)
            {
                listExpressionMonth.Add(Expression.Equal(Expression.Property(param, "Month"), Expression.Constant(item.Value)));
            }
            BinaryExpression expresionMonth = listExpressionMonth.Aggregate((prev, current) => Expression.Or(prev, current));

            if (DayOrYear > 0)
            {
                //Pattern dd or yyyy
                var propDay = Expression.Property(param, "Day");
                var resDay = Expression.Equal(propDay, Expression.Constant(DayOrYear));
                var propYear = Expression.Property(param, "Year");
                var resYear = Expression.Equal(propYear, Expression.Constant(DayOrYear));
                BinaryExpression result = Expression.Or(resDay, resYear);
                return result;
            }
            else if (day > 0)
            {
                //Pattern dd MMMM
                var res = Expression.Equal(Expression.Property(param, "Day"), Expression.Constant(day));
                Expression result = new Expression[]
                {
                        expresionMonth,res
                }.Aggregate((prev, current) => Expression.And(prev, current));
                return result;
            }
            else if (year > 0)
            {
                //Pattern MMMM yyyy
                var res = Expression.Equal(Expression.Property(param, "Year"), Expression.Constant(year));
                Expression result = new Expression[]
                {
                        expresionMonth,res
                }.Aggregate((prev, current) => Expression.And(prev, current));
                return result;
            }
            return null;
        }
        private static Expression? ExpressionLike(ConstantExpression constant, ParameterExpression parameter,string item,bool IsStringType)
        {
            var efLike = typeof(DbFunctionsExtensions).GetMethod("Like",
                BindingFlags.Static | BindingFlags.Public, null,
                new[]
                {
                    typeof(DbFunctions),typeof(string),typeof(string)
                }, null);
            if (efLike is null) return null;
            // We make a pattern for the search
            if(IsStringType)
            {
                Expression expr = Expression.Call(efLike,
                Expression.Property(null, typeof(EF), nameof(EF.Functions)), Expression.PropertyOrField(parameter, item)
                , constant);
                return expr;
            }
            MemberExpression param = Expression.PropertyOrField(parameter, item);
            MethodCallExpression methodCallExpressionToString = Expression.Call(param, "ToString", null);
            var propFunc = Expression.Property(null, typeof(EF), nameof(EF.Functions));
            MethodCallExpression EfLikeFunc = Expression.Call(efLike, propFunc, methodCallExpressionToString, constant);
            return EfLikeFunc;
        }
        private static IQueryable<T> SearchGrid<T>(this IQueryable<T> source, string searchValue, List<string> Field)
        {
            searchValue = $"{searchValue?.Trim().ToLower()}" ?? "";
            ConstantExpression expressionConstant = Expression.Constant($"{searchValue}%");
            // T is a compile-time placeholder for the element type of the query.
            Type elementType = typeof(T);

            // Get all the string properties on this specific type.
            PropertyInfo[] stringProperties =
                elementType.GetProperties()
                    .Where(x => Field.Any(d => d == x.Name))
                    .ToArray();
            if (!stringProperties.Any()) { return source; }
            ParameterExpression parameter = Expression.Parameter(elementType);

            List<Expression> buildExpression = new List<Expression>();
            foreach (var item in Field)
            {
                if (stringProperties.Any(s => s.Name == item && s.PropertyType == typeof(string)))
                {
                    var expr = ExpressionLike(constant: expressionConstant, parameter, item,true);
                    if (expr is null) continue;
                    buildExpression.Add(expr);
                }
                else if (stringProperties.Any(s => s.Name == item && (s.PropertyType == typeof(DateTime) || s.PropertyType == typeof(DateTime?))))
                {
                    var buildExpDate = BuildExpressionSearchDate(parameter, item, searchValue);
                    if (buildExpDate is null) continue;
                    buildExpression.Add(buildExpDate);
                }
                else
                {
                    var expr = ExpressionLike(constant: expressionConstant, parameter, item, false);
                    if (expr is null) continue;
                    buildExpression.Add(expr);
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

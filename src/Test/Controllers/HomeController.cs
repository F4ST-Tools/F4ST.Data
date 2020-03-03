using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using F4ST.Common.Containers;
using F4ST.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Test.Data;
using Test.Models;

namespace Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceProvider _provider;

        public HomeController(ILogger<HomeController> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task<IActionResult> Index()
        {
            using (var rep = _provider.GetRepository("DB"))
            {
                //var count = await rep.Count<TestEntity>();

                //if (count == 0)
                /*{
                    var item = new TestEntity()
                    {
                        Name = "test",
                        Family = "test"
                    };

                    await rep.Add(item);
                    await rep.SaveChanges();

                    item.Family = "test 2";
                    await rep.Update(item);
                    await rep.SaveChanges();
                }
                
                var items = await rep.Find<TestEntity>(t => t.Name == "test");*/
                var aa = 3;

                var bb = "asd";

                //var cc = ResolveMemberExpression(b);

                var a=await rep.Find<TestEntity>(c => c.Id== 1 /*&& c.BankTitle.Contains(bb)*/);

            }

            return View();
        }


        private static KeyValuePair<Type, object>[] ResolveArgs<T>(Expression<Func<T, object>> expression)
        {
            var body = (System.Linq.Expressions.MethodCallExpression)expression.Body;
            var values = new List<KeyValuePair<Type, object>>();

            foreach (var argument in body.Arguments)
            {
                var exp = ResolveMemberExpression(argument);
                var type = argument.Type;

                var value = GetValue(exp);

                values.Add(new KeyValuePair<Type, object>(type, value));
            }

            return values.ToArray();
        }

        public static MemberExpression ResolveMemberExpression(Expression expression)
        {

            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                return (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }

        private static object GetValue(MemberExpression exp)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                    .GetType()
                    .GetField(exp.Member.Name)
                    .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return GetValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public static class ExtensionToExpression
    {
        public static Expression<Func<TTo, bool>> Converter<TFrom, TTo>(this Expression<Func<TFrom, bool>> expression, TTo type) where TTo : TFrom
        {
            // here we get the expression parameter the x from (x) => ....
            var parameterName = expression.Parameters.First().Name;
            // create the new parameter from the correct type
            ParameterExpression parameter = Expression.Parameter(typeof(TTo), parameterName);
            // asigne to allow the visit from or visitor
            Expression body = new ConvertVisitor(parameter).Visit(expression.Body);
            // recreate the expression
            return Expression.Lambda<Func<TTo, bool>>(body, parameter);
        }
    }

    public class ConvertVisitor : ExpressionVisitor
    {
        private ParameterExpression Parameter;

        public ConvertVisitor(ParameterExpression parameter)
        {
            Parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression item)
        {
            // we just check the parameter to return the new value for them
            if (!item.Name.Equals(Parameter.Name))
                return item;
            return Parameter;
        }
    }
}

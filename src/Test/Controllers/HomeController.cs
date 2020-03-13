using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                {
                    var item = new TestEntity()
                    {
                        BankTitle = "bank 1",
                        CountryId = 1,
                        IsActive = true
                    };

                    await rep.Add(item);
                    await rep.SaveChanges();

                    /*item.Family = "test 2";
                    await rep.Update(item);
                    await rep.SaveChanges();*/
                }
                
                //var items = await rep.Find<TestEntity>(t => t.Name == "test");*/
                //var aa = 3;

                //var bb = "asd";

                
                //var cc = ResolveMemberExpression(b);
                var items = new List<int>{1,2,4};

                //var a=await rep.Find<TestEntity>(c => items.Contains(c.Id) /*&& c.BankTitle.Contains(bb)*/);
                var b=await rep.Find<TestEntity, CountryEntity>(c => items.Contains(c.Id),c=>c.CountryId,
                    c=>c.Country,o=>o.Id,1,10,false);

            }

            return View();
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

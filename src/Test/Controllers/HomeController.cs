using System;
using System.Diagnostics;
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
            using (var rep = _provider.GetRepository("RavenDB"))
            {
                //var count = await rep.Count<TestEntity>();

                //if (count == 0)
                {
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



                var items = await rep.Find<TestEntity>(t => t.Name == "test");
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
}

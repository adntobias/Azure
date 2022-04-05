using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APICall.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace APICall.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var quote = GetQuote().GetAwaiter().GetResult();
            return View(quote);
        }

        [Route("GetQuote")]
        public async Task<Quote> GetQuote(){
            var api = "http://api.quotable.io/random";
            var result = "";
            
            using(HttpClient client = new HttpClient()) 
            {
                var res = await client.GetAsync(api);
                
                res.EnsureSuccessStatusCode();
                
                result = await res.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Quote>(result);
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
}

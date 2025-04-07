// using Microsoft.AspNetCore.Mvc;
// using UI;
//
// namespace UEconomy.UI.Controllers;
//
// public class HomeController : Controller
// {
//     private readonly GameService gameService;
//
//     public HomeController(GameService gameService)
//     {
//         this.gameService = gameService;
//     }
//
//     public IActionResult Index()
//     {
//         return View(gameService.GetGame());
//     }
//
//     public IActionResult Province(int id)
//     {
//         var province = gameService.GetGame().GetProvinceById(id);
//         return View(province);
//     }
// }
using BetFlowSystems.Models;
using BetFlowSystems.Models.DTOs.Simulation;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BetFlowSystems.Controllers
{
    [Authorize(Roles ="Admin")]
    public class SimulationController : Controller
    {
        private readonly UserSimulationService _simulationService;

        public SimulationController(UserSimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        public IActionResult Index()
        {
            return View(new SimulationDto());
        }

        [HttpPost]
        public async Task<IActionResult> Execute(SimulationDto model)
        {
            Result? result = null;

            switch (model.ActionType)
            {
                case "Deposit":
                    result = await _simulationService.Deposit(model.AccountID, model.Amount);
                    break;

                case "Withdraw":
                    result = await _simulationService.Withdrawal(model.AccountID, model.Amount);
                    break;

                case "Zero":
                    result = await _simulationService.ZeroAccount(model.AccountID);
                    break;

                case "RandomBet":
                    result = await _simulationService.GenerateRandomBet(model.AccountID);
                    break;
            }

            if (result != null)
            {
                model.ResultMessage = result.Success
                    ? "Operation Successful"
                    : $"Error: {result.ErrorMessage}";

                return View("Index", model);
            }

            return View();
        }
    }
}

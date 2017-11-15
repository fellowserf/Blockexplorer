﻿using System;
using System.Threading.Tasks;
using Blockexplorer.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blockexplorer.Controllers
{
    public class ApiController : Controller
    {
        readonly IApiService _apiService;
	    const decimal Burn = 8800096.6m;


		public ApiController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // [HttpGet]
        public async Task<ActionResult> GetInfo()
        {
            try
            {
                var info = await _apiService.GetInfo();
                if (info == null || !string.IsNullOrWhiteSpace(info.Errors))
                    return StatusCode(StatusCodes.Status500InternalServerError);
	            info.MoneySupply = info.MoneySupply - Burn;
                return Json(new { info = info });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // [HttpGet]
        public async Task<ActionResult> GetStakingInfo()
        {
            try
            {
                var info = await _apiService.GetStakingInfo();
                if (info == null || !string.IsNullOrWhiteSpace(info.Errors))
                    return StatusCode(StatusCodes.Status500InternalServerError);
                return Json(new { info = info });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // [HttpGet]
        public async Task<ActionResult> MoneySupplyJson()
        {
            try
            {
                var info = await _apiService.GetInfo();
                if (info == null || !string.IsNullOrWhiteSpace(info.Errors))
                    return StatusCode(StatusCodes.Status500InternalServerError);
				var withBurn = info.MoneySupply - Burn;

				return Json(new { moneySupply = Convert.ToInt32(withBurn) });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // [HttpGet]
        public async Task<string> MoneySupplyString()
        {
            try
            {
                var info = await _apiService.GetInfo();
                if (info == null || !string.IsNullOrWhiteSpace(info.Errors))
                    return "-1";
	            var withBurn = info.MoneySupply - 8800096.6m;
				return Convert.ToInt32(withBurn).ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }
    }
}

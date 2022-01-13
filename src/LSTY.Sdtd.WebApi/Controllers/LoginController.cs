using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.DataAnnotations;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// 登录
    /// </summary>
    [Route("[controller]/[action]")]
    public class LoginController : ApiControllerBase
    {
        /// <summary>
        /// 检查权限
        /// </summary>
        [HttpPost]
        [SucceededResponseType(typeof(Response))]
        public IResponse Check()
        {
            return SucceededResult();
        }
    }
}

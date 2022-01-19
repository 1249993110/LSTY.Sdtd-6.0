using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using IceCoffee.DataAnnotations;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// 认证
    /// </summary>
    [Route("[controller]/[action]")]
    public class AuthenticateController : ApiControllerBase
    {
        /// <summary>
        /// 检查凭证
        /// </summary>
        [HttpPost]
        [SucceededResponseType(typeof(Response))]
        public IResponse CheckToken()
        {
            return SucceededResult();
        }
    }
}

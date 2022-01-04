using IceCoffee.AspNetCore;
using IceCoffee.AspNetCore.Controllers;
using IceCoffee.AspNetCore.Models.Primitives;
using LSTY.Sdtd.Data.Entities;
using LSTY.Sdtd.Data.IRepositories;
using LSTY.Sdtd.Services;
using LSTY.Sdtd.Services.HubReceivers;
using LSTY.Sdtd.Services.Managers;
using LSTY.Sdtd.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

namespace LSTY.Sdtd.WebApi.Controllers
{
    /// <summary>
    /// ItemIcons
    /// </summary>
    [Route("[controller]")]
    public class ItemIconsController : ApiControllerBase
    {
        private readonly ServerManageHubReceiver _serverManageHub;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemIconsController(SignalRManager signalRManager, IWebHostEnvironment webHostEnvironment)
        {
            _serverManageHub = signalRManager.ServerManageHub;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 获取 ItemIcons
        /// </summary>
        /// <remarks>
        /// e.g. /api/ItemIcons/airConditioner__00FF00.png 颜色是可选的
        /// </remarks>
        /// <param name="iconName">图标名称，可带颜色，格式见例子</param>
        /// <returns></returns>
        [HttpGet("{iconName}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string iconName)
        {
            if (iconName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false)
            {
                return BadRequest("Invalid icon name");
            }

            int len = iconName.Length;
            string iconColor = null;
            if (len > 12 && iconName[len - 11] == '_')
            {
                iconColor = iconName.Substring(len - 10, 6);
                iconName = iconName.Substring(0, len - 12) + ".png";
            }

            byte[] data = null;
            // color 不为空时才查找本地，因为已经在 UseStaticFiles 中间件中查找过
            if (iconColor != null)
            {
                string localPath = Path.Combine(_webHostEnvironment.WebRootPath, "ItemIcons", iconName);
                if (System.IO.File.Exists(localPath))
                {
                    data = System.IO.File.ReadAllBytes(localPath);
                }
                else
                {
                    data = await _serverManageHub.GetItemIcon(iconName);
                }
            }
            else
            {
                data = await _serverManageHub.GetItemIcon(iconName);
            }

            if (data == null)
            {
                return NotFound("Icon not found");
            }

            if (iconColor == null)
            {
                return base.File(data, "image/png");
            }
            else
            {
                int r, g, b;
                try
                {
                    r = Convert.ToInt32(iconColor.Substring(0, 2), 16);
                    g = Convert.ToInt32(iconColor.Substring(2, 2), 16);
                    b = Convert.ToInt32(iconColor.Substring(4, 2), 16);
                }
                catch
                {
                    return BadRequest("Invalid icon color");
                }

                using (var skImage = SKBitmap.Decode(data))
                {
                    int width = skImage.Width;
                    int height = skImage.Height;

                    for (int i = 0; i < width; ++i)
                    {
                        for (int j = 0; j < height; ++j)
                        {
                            var skColor = skImage.GetPixel(i, j);

                            skImage.SetPixel(i, j, new SKColor(
                                (byte)(skColor.Red * r / 255),
                                (byte)(skColor.Green * g / 255),
                                (byte)(skColor.Blue * b / 255),
                                skColor.Alpha));
                        }
                    }

                    var  stream = new MemoryStream();
                    using (SKManagedWStream wstream = new SKManagedWStream(stream))
                    {
                        skImage.PeekPixels().Encode(wstream, SKEncodedImageFormat.Png, 100);
                    }

                    stream.Position = 0;
                    return base.File(stream, "image/png");
                }
            }
        }
    }
}

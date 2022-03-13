using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.Text;

namespace ImageServiceDemo.Controllers
{
  [ApiController]
  public class DemoController : ControllerBase
  {
    [HttpGet]
    [Route("/source")]
    public async Task Get()
    {
      var res = new byte[1000];
      var boundary = Guid.NewGuid().ToString();
      var generator = new Random();
      var map = new SKBitmap(250, 250);
      var canvas = new SKCanvas(map);
      var inPaint = new SKPaint
      {
        Color = SKColors.Black,
        Style = SKPaintStyle.Fill,
        FilterQuality = SKFilterQuality.High
      };
      var exPaint = new SKPaint
      {
        Color = SKColors.White,
        Style = SKPaintStyle.Fill,
        FilterQuality = SKFilterQuality.Low
      };

      Response.ContentType = "multipart/x-mixed-replace;boundary=" + boundary;

      var outputStream = Response.Body;
      var cancellationToken = Request.HttpContext.RequestAborted;

      async Task drawShapes()
      {
        using (var image = SKImage.FromBitmap(map))
        {
          var pos = generator.Next(50, 150);
          canvas.DrawRect(0, 0, 250, 250, exPaint);
          canvas.DrawCircle(pos, pos, 20, inPaint);
          res = image.Encode(SKEncodedImageFormat.Webp, 100).ToArray();
        }

        var header = $"--{ boundary }\r\nContent-Type: image/webp\r\nContent-Length: { res.Length }\r\n\r\n";
        var headerData = Encoding.ASCII.GetBytes(header);

        await outputStream.WriteAsync(headerData);
        await outputStream.WriteAsync(res);
        await outputStream.WriteAsync(Encoding.ASCII.GetBytes("\r\n"));
      }

      await Task.Run(async () =>
      {
        try
        {
          //while (true)
          {
            await drawShapes();
            await drawShapes();
          }
        }
        catch (TaskCanceledException) { }
      });
    }

  }
}
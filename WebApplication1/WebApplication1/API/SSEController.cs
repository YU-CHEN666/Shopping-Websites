using Microsoft.AspNetCore.Mvc;
using WebApplication1.Service;

namespace WebApplication1.API
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SSEController : ControllerBase
	{
		private readonly ILogger<SSEController> _logger;
		public SSEController(ILogger<SSEController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public async Task CleanupNotice(CancellationToken disconnectToken)
		{
			Response.ContentType = "text/event-stream";
			Response.Headers.Append("Cache-Control", "no-cache");
			using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(2)))
			{
				try
				{
					while (await timer.WaitForNextTickAsync(disconnectToken))
					{
						TimeSpan cleanTimeInterval = DateTime.Now - CleanTime.GetTime();
						if (cleanTimeInterval <= TimeSpan.FromSeconds(2))
						{
							await Response.WriteAsync("data:clean\n\n", disconnectToken);
							await Response.Body.FlushAsync(disconnectToken);
							_logger.LogInformation("{infoTime}:SSE通知，匿名購物車已清理", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
						}
					}
				}
				catch(Exception ex) when (ex is OperationCanceledException || disconnectToken.IsCancellationRequested)
				{

				}
			}
		}
	}	
}

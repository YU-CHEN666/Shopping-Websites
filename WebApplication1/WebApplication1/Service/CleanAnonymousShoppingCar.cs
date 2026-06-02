using Microsoft.Extensions.Options;
namespace WebApplication1.Service
{
	//背景服務-定期清理匿名購物車
	public class CleanAnonymousShoppingCar : BackgroundService
	{
		private readonly ILogger<CleanAnonymousShoppingCar> _logger;
		private readonly IOptionsMonitor<PeriodicTimerSettings> _optionsMonitor;
		private IDisposable? _changeEventRemoveLambdaControl;
		private readonly ShoppingCarManage _shoppingCarManage;
		
		public CleanAnonymousShoppingCar(ILogger<CleanAnonymousShoppingCar> logger, IOptionsMonitor<PeriodicTimerSettings> optionsMonitor, ShoppingCarManage shoppingCarManage)
		{
			_logger = logger;
			_optionsMonitor = optionsMonitor;
			_shoppingCarManage = shoppingCarManage;
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				TimeSpan interval = _optionsMonitor.CurrentValue.Interval;
				using(CancellationTokenSource changeToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
				{
					_changeEventRemoveLambdaControl = _optionsMonitor.OnChange((settings) =>
					{
						_logger.LogInformation("{infoTime}:偵測到定時器時間修改為{newInterval}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), settings.Interval);
						changeToken.Cancel();
					});
					try
					{
						using(PeriodicTimer timer = new PeriodicTimer(interval))
						{
							while(await timer.WaitForNextTickAsync(changeToken.Token))
							{
								_logger.LogInformation("{missionStartTime}:開始清理匿名購物車定期任務", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
								ClearnState result = _shoppingCarManage.ClearnAnonymousCar(stoppingToken);
								switch(result)
								{
									case ClearnState.Success:
										_logger.LogInformation("{missionStateTime}:定期清理匿名購物車任務完成", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
										CleanTime.SetTime(DateTime.Now);
										continue;
									case ClearnState.StopTokenStop:
										_logger.LogInformation("{missionStateTime}:應用程式結束，中止定期清理匿名購物車任務", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
										continue;
									case ClearnState.Fail:
										_logger.LogWarning("{warningTime}:定期清理匿名購物車任務失敗", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
										continue;
								}
							}
						}
					}
					catch(OperationCanceledException)
					{
						_changeEventRemoveLambdaControl.Dispose();
						if(stoppingToken.IsCancellationRequested)
						{
							_logger.LogInformation("{infoTime}:偵測到應用程式結束，結束服務", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
						}
					}
				}
			}
		}

		public override void Dispose()
		{
			_changeEventRemoveLambdaControl.Dispose();
			base.Dispose();
		}

	}

	public class PeriodicTimerSettings
	{
		public TimeSpan Interval { get; set; }
	}

	internal static class CleanTime
	{
		static private DateTime _happenTime { get;  set; } = DateTime.Now;
		static private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		internal static DateTime GetTime()
		{
			_rwLock.EnterReadLock();
			DateTime time = _happenTime;
			_rwLock.ExitReadLock();
			return time;
		}

		internal static void SetTime(DateTime time)
		{
			_rwLock.EnterWriteLock();
			_happenTime = time;
			_rwLock.ExitWriteLock();
		}
	}

}

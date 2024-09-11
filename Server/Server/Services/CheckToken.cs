using Server.Data;

namespace Server.Services
{
    public class CheckToken : IHostedService, IDisposable
    {
        private Timer? _timer;

        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _expirationTime = TimeSpan.FromHours(1);

        private ApplicationDbContext? db;

        public void AddContext(ApplicationDbContext db)
        {
            this.db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Work, null, TimeSpan.Zero,
                _checkInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Work(object? state)
        {
            if (db == null)
            {
                return;
            }
            var tokens = db.Tokens.Where(token => token.active == true).ToList();
            foreach (var token in tokens.Where(token =>
                         DateTime.Now - token.created_at > _expirationTime))
            {
                token.active = false;
            }
            db.SaveChanges();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

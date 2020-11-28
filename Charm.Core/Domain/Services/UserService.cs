using System;
using System.Threading.Tasks;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelegramUser = Telegram.Bot.Types.User;

namespace Charm.Core.Domain.Services
{
    public class UserService
    {
        public UserService(ILogger<UserService> logger, CharmDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public string UserInfo => $"@{_charmUser.Name} ({_charmUser.Id})";

        private TelegramUser _telegramUser = null!;

        public TelegramUser TelegramUser
        {
            get => _telegramUser ?? throw new Exception("Telegram user not yet initialized!");
            set => _telegramUser = value;
        }


        private User _charmUser = null!;

        public User CharmUser
        {
            get => _charmUser ?? throw new Exception("Charm user not yet initialized!");
            set => _charmUser = value;
        }


        private readonly ILogger<UserService> _logger;
        private readonly CharmDbContext _context;

        // Warning! Clears ChangeTracker, should be called at the beggining of telegram request!
        public async Task Initialize(TelegramUser telegramUser)
        {
            _logger.LogDebug("Initializing...");

            _telegramUser = telegramUser;
            _charmUser = await _context.Users.SingleOrDefaultAsync(e => e.Id.Equals(_telegramUser.Id));

            if (_charmUser is not null)
            {
                _logger.LogDebug($"User found: {_charmUser.Id} ({_charmUser.Name})");
                await ChangeNameIfNecessary();
            }
            else
            {
                await CreateNewCharmUser();
            }

            _context.ChangeTracker.Clear();
            _logger.LogDebug("UserService was initialized, ef change tracker cleared.");
        }

        private async Task ChangeNameIfNecessary()
        {
            if (_charmUser.Name != _telegramUser.FirstName)
            {
                _logger.LogDebug("User name changed, updating..");
                _charmUser.Name = _telegramUser.FirstName;
                await _context.SaveChangesAsync();
                _logger.LogDebug("User name successfully updated");
            }
        }

        private async Task CreateNewCharmUser()
        {
            _charmUser = new User {Id = _telegramUser.Id, Name = _telegramUser.FirstName};
            _context.Users.Add(_charmUser);
            await _context.SaveChangesAsync();
            _logger.LogDebug($"New user was registered: {_charmUser.Id} ({_charmUser.Name})");
        }
    }
}
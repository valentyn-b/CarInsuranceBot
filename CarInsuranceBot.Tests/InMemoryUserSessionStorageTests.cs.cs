using CarInsuranceBot.API.Application.Enums;
using CarInsuranceBot.API.Core.Entities;
using CarInsuranceBot.API.Infrastructure.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CarInsuranceBot.Tests
{
    public class InMemoryUserSessionStorageTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly InMemoryUserSessionStorage _storage;

        public InMemoryUserSessionStorageTests()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            _storage = new InMemoryUserSessionStorage(_memoryCache);
        }

        [Fact]
        public void GetSession_WhenChatIdIsNew_ShouldReturnNewSessionWithDefaultState()
        {
            // Arrange
            long newChatId = 12345;

            // Act
            var session = _storage.GetSession(newChatId);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(UserState.New, session.State);
            Assert.Null(session.Passport);
            Assert.Null(session.Vehicle);
        }

        [Fact]
        public void SaveSession_ShouldPersistDataCorrectly()
        {
            // Arrange
            long chatId = 999;
            var newSession = new UserSession
            {
                State = UserState.WaitingForPassport,
                TelegramName = "TestUser"
            };

            // Act
            _storage.SaveSession(chatId, newSession);
            var retrievedSession = _storage.GetSession(chatId);

            // Assert
            Assert.NotNull(retrievedSession);
            Assert.Equal(UserState.WaitingForPassport, retrievedSession.State);
            Assert.Equal("TestUser", retrievedSession.TelegramName);
        }

        [Fact]
        public void GetSession_ShouldNotMixUpDifferentUsers()
        {
            // Arrange
            long user1Id = 111;
            long user2Id = 222;

            var user1Session = new UserSession { State = UserState.ConfirmingPassport };

            // Act
            _storage.SaveSession(user1Id, user1Session);

            var retrievedUser1 = _storage.GetSession(user1Id);
            var retrievedUser2 = _storage.GetSession(user2Id);

            // Assert
            Assert.Equal(UserState.ConfirmingPassport, retrievedUser1.State);
            Assert.Equal(UserState.New, retrievedUser2.State);
        }
    }
}
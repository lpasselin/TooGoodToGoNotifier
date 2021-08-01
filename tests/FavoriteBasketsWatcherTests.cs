﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Tests
{
    [TestFixture]
    public class FavoriteBasketsWatcherTests
    {
        private Mock<ILogger<FavoriteBasketsWatcher>> _loggerMock;
        private Mock<ITooGoodToGoService> _tooGoodToGoServiceMock;
        private Mock<IEmailNotifier> _emailNotifierMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<FavoriteBasketsWatcher>>();
            _tooGoodToGoServiceMock = new Mock<ITooGoodToGoService>();
            _emailNotifierMock = new Mock<IEmailNotifier>();
        }

        [Test]
        public async Task FavoriteBasketsWatcher_When_OneBasketIsAvailableAndAnotherIsNot_Should_OnlyNotifyAvailableBasket()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                {
                    new Basket
                    {
                        DisplayName = "Basket N°1",
                        ItemsAvailable = 1,
                        Item = new Item
                        {
                            ItemId = 1
                        }
                    },
                    new Basket
                    {
                        DisplayName = "Basket N°2",
                        ItemsAvailable = 0,
                        Item = new Item
                        {
                            ItemId = 2
                        }
                    }
                }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBaskets()).Returns(Task.FromResult(getBasketsResponse));

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _tooGoodToGoServiceMock.Object, _emailNotifierMock.Object);
            await favoriteBasketsWatcher.Invoke();

            // Only one basket should have been notified, the basket N°1 that has 1 available item.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);
        }

        [Test]
        public async Task FavoriteBasketsWatcher_When_BasketIsSeenTwoTimesAsAvailable_Should_NotifyOnce()
        {
            var getBasketsResponse = new GetBasketsResponse
            {
                Items = new List<Basket>
                {
                    new Basket
                    {
                        DisplayName = "Basket N°1",
                        ItemsAvailable = 1,
                        Item = new Item
                        {
                            ItemId = 1
                        }
                    }
                }
            };

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBaskets()).Returns(Task.FromResult(getBasketsResponse));

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _tooGoodToGoServiceMock.Object, _emailNotifierMock.Object);
            await favoriteBasketsWatcher.Invoke();

            // Notify should have been called only once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);
        }

        [Test]
        public async Task FavoriteBasketsWatcher_When_BasketIsSeenAsAvailable_Then_SeenAsNotAvailable_Then_SeenAsAvailable_Should_NotifyTwice()
        {
            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBaskets()).Returns(Task.FromResult(GetBasketsResponse(1)));

            var favoriteBasketsWatcher = new FavoriteBasketsWatcher(_loggerMock.Object, _tooGoodToGoServiceMock.Object, _emailNotifierMock.Object);
            await favoriteBasketsWatcher.Invoke();

            // Notify should be called once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBaskets()).Returns(Task.FromResult(GetBasketsResponse(0)));
            await favoriteBasketsWatcher.Invoke();

            // Notify should still have been called once.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Once);

            _tooGoodToGoServiceMock.Setup(x => x.GetFavoriteBaskets()).Returns(Task.FromResult(GetBasketsResponse(1)));
            await favoriteBasketsWatcher.Invoke();

            // Notify should have been called a second time.
            _emailNotifierMock.Verify(x => x.Notify(It.Is<List<Basket>>(x => x.Count == 1 && x[0].Item.ItemId == 1)), Times.Exactly(2));

            static GetBasketsResponse GetBasketsResponse(int availableItems)
            {
                return new GetBasketsResponse
                {
                    Items = new List<Basket>
                    {
                        new Basket
                        {
                            DisplayName = "Basket N°1",
                            ItemsAvailable = availableItems,
                            Item = new Item
                            {
                                ItemId = 1
                            }
                        }
                    }
                };
            }
        }
    }
}

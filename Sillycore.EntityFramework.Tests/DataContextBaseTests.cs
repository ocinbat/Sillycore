using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Sillycore.EntityFramework.Extensions;
using Sillycore.EntityFramework.Tests.Stubs;

namespace Sillycore.EntityFramework.Tests
{
    public class DataContextBaseTests
    {
        [Test]
        public void Ctor_WhenDefaultOptionsUsed_ShouldAddDefaultEventListeners()
        {
            //Arrange
            var contextOptions = CreateInMemoryDatabaseOptions();

            //Act
            DataContextBase sut = new StubDataContextBase(contextOptions);

            //Verify
            sut.EventListeners.Should().HaveCount(2);
            sut.EventListeners.Any(el => el.GetType().IsAssignableFrom(typeof(AuditEventListener))).Should().BeTrue();
            sut.EventListeners.Any(el => el.GetType().IsAssignableFrom(typeof(SoftDeleteEventListener))).Should().BeTrue();
        }

        [Test]
        public void Ctor_WhenDefaultEventListenersNotRequired_ShouldNotAddAnyEventListeners()
        {
            //Arrange
            var contextOptions = CreateInMemoryDatabaseOptions();

            var sillycoreOptions = new SillycoreDataContextOptions()
            {
                UseDefaultEventListeners = false
            };

            //Act
            DataContextBase sut = new StubDataContextBase(contextOptions, sillycoreOptions);

            //Verify
            sut.EventListeners.Should().HaveCount(0);
        }

        [Test]
        public void SubscribeListener_ShouldAddNewListener()
        {
            //Arrange
            var contextOptions = CreateInMemoryDatabaseOptions();

            var sillycoreOptions = new SillycoreDataContextOptions()
            {
                UseDefaultEventListeners = false
            };

            DataContextBase sut = new StubDataContextBase(contextOptions, sillycoreOptions);
            Mock<IEntityEventListener> mockListener1 = new Mock<IEntityEventListener>();
            Mock<IEntityEventListener> mockListener2 = new Mock<IEntityEventListener>();
            Mock<IEntityEventListener> mockListener3 = new Mock<IEntityEventListener>();

            //Act
            sut.SubscribeListener(mockListener1.Object);
            sut.SubscribeListener(mockListener2.Object);
            sut.SubscribeListener(mockListener3.Object);
            sut.SubscribeListener(mockListener1.Object);

            //Verify
            sut.EventListeners.Should().HaveCount(3);
        }

        [Test]
        public void UnsubscribeListener_ShouldRemoveExistingListener()
        {
            //Arrange
            var contextOptions = CreateInMemoryDatabaseOptions();

            var sillycoreOptions = new SillycoreDataContextOptions()
            {
                UseDefaultEventListeners = false
            };

            DataContextBase sut = new StubDataContextBase(contextOptions, sillycoreOptions);
            Mock<IEntityEventListener> mockListener1 = new Mock<IEntityEventListener>();
            Mock<IEntityEventListener> mockListener2 = new Mock<IEntityEventListener>();
            Mock<IEntityEventListener> mockListener3 = new Mock<IEntityEventListener>();

            sut.SubscribeListener(mockListener1.Object);
            sut.SubscribeListener(mockListener2.Object);
            sut.SubscribeListener(mockListener3.Object);

            //Act
            sut.UnsubscribeListener(mockListener1.Object);
            sut.UnsubscribeListener(mockListener2.Object);
            sut.UnsubscribeListener(mockListener2.Object);

            //Verify
            sut.EventListeners.Should().HaveCount(1);
        }

        [Test]
        public void SaveChanges_ShouldNotifyListeners()
        {
            //Arrange
            var contextOptions = CreateInMemoryDatabaseOptions();

            var sillycoreOptions = new SillycoreDataContextOptions()
            {
                UseDefaultEventListeners = false
            };

            DataContextBase sut = new StubDataContextBase(contextOptions, sillycoreOptions);
            Mock<IEntityEventListener> mockListener = new Mock<IEntityEventListener>();
            sut.SubscribeListener(mockListener.Object);

            //Act
            sut.SaveChanges();

            //Verify
            mockListener.Verify(l => l.NotifyBeforeSaveChanges(sut), Times.Once);
        }

        private static DbContextOptions<StubDataContextBase> CreateInMemoryDatabaseOptions()
        {
            var contextOptions = new DbContextOptionsBuilder<StubDataContextBase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("d"))
                .Options;
            return contextOptions;
        }

    }
}
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Sillycore.EntityFramework.Extensions;
using Sillycore.EntityFramework.Tests.Stubs;
using System;
using System.Security.Principal;

namespace Sillycore.EntityFramework.Tests
{
    public class AuditEventListenerTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            if (SillycoreApp.Instance == null)
            {
                SillycoreAppBuilder.Instance.UseLocalTimes().Build();
            }
        }

        [TearDown]
        public void TearDown()
        {
            System.Threading.Thread.CurrentPrincipal = null;
        }

        private static DbContextOptions<StubDataContextBase> CreateInMemoryDatabaseOptions()
        {
            var contextOptions = new DbContextOptionsBuilder<StubDataContextBase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("d"))
                .Options;

            return contextOptions;
        }

        private static void SetPrinciple(string identityName)
        {
            var identity = new GenericIdentity(identityName);
            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Role2" });
        }

        [Test]
        public void NotifyBeforeSaveChanges_WhenNewEntityIsAdded_ShouldSetCreatedPropertiesOfAuditableEntity()
        {
            //Arrange
            var identityName = _fixture.Create<string>();
            SetPrinciple(identityName);

            DbContextOptions<StubDataContextBase> contextOptions = CreateInMemoryDatabaseOptions();
            var dataContext = new StubDataContextBase(contextOptions, new SillycoreDataContextOptions { UseDefaultEventListeners = false });

            var stubEntity = _fixture.Build<StubEntity>()
                .Without(x => x.CreatedBy)
                .Without(x => x.CreatedOn)
                .Without(x => x.UpdatedBy)
                .Without(x => x.UpdatedOn)
                .Create();
            dataContext.StubEntities.Add(stubEntity);

            //Act
            var sut = new AuditEventListener();
            sut.NotifyBeforeSaveChanges(dataContext);

            //Verify
            stubEntity.CreatedOn.Should().NotBe(DateTime.MinValue);
            stubEntity.CreatedBy.Should().Be(identityName);
        }

        [Test]
        public void NotifyBeforeSaveChanges_WhenNewEntityIsAdded_And_SetUpdatedOnSameAsCreatedOnForNewObjectsIsTrue_ShouldSetUpdatedPropertiesOfAuditableEntity()
        {
            //Arrange
            var identityName = _fixture.Create<string>();
            SetPrinciple(identityName);

            DbContextOptions<StubDataContextBase> contextOptions = CreateInMemoryDatabaseOptions();
            var dataContext = new StubDataContextBase(contextOptions, new SillycoreDataContextOptions
            {
                UseDefaultEventListeners = false
            });

            var stubEntity = _fixture.Build<StubEntity>()
                .Without(x => x.CreatedBy)
                .Without(x => x.CreatedOn)
                .Without(x => x.UpdatedBy)
                .Without(x => x.UpdatedOn)
                .Create();

            dataContext.StubEntities.Add(stubEntity);

            //Act
            var sut = new AuditEventListener();
            sut.NotifyBeforeSaveChanges(dataContext);

            //Verify
            stubEntity.UpdatedOn.Should().NotBe(DateTime.MinValue);
            stubEntity.UpdatedBy.Should().Be(identityName);
        }

        [Test]
        public void NotifyBeforeSaveChanges_WhenNewEntityIsAdded_And_SetUpdatedOnSameAsCreatedOnForNewObjectsIsFalse_ShouldSetNotUpdatedPropertiesOfAuditableEntity()
        {
            //Arrange
            var identityName = _fixture.Create<string>();
            SetPrinciple(identityName);

            DbContextOptions<StubDataContextBase> contextOptions = CreateInMemoryDatabaseOptions();
            var dataContext = new StubDataContextBase(contextOptions, new SillycoreDataContextOptions
            {
                UseDefaultEventListeners = false
            });

            var stubEntity = _fixture.Build<StubEntity>()
                .Without(x => x.CreatedBy)
                .Without(x => x.CreatedOn)
                .Without(x => x.UpdatedBy)
                .Without(x => x.UpdatedOn)
                .Create();

            dataContext.StubEntities.Add(stubEntity);

            //Act
            var sut = new AuditEventListener();
            sut.NotifyBeforeSaveChanges(dataContext);

            //Verify
            stubEntity.UpdatedOn.Should().BeNull();
            stubEntity.UpdatedBy.Should().BeNull();
        }

        [Test]
        public void NotifyBeforeSaveChanges_WhenEntityIsUpdated_ShouldSetUpdatedPropertiesOfAuditableEntity()
        {
            //Arrange
            var identityName = _fixture.Create<string>();
            SetPrinciple(identityName);

            DbContextOptions<StubDataContextBase> contextOptions = CreateInMemoryDatabaseOptions();
            var dataContext = new StubDataContextBase(contextOptions, new SillycoreDataContextOptions
            {
                UseDefaultEventListeners = false
            });

            var stubEntity = _fixture.Build<StubEntity>()
                .Without(x => x.UpdatedBy)
                .Without(x => x.UpdatedOn)
                .Create();

            dataContext.StubEntities.Add(stubEntity);
            dataContext.MarkAsModified(stubEntity);

            //Act
            var sut = new AuditEventListener();
            sut.NotifyBeforeSaveChanges(dataContext);

            //Verify
            stubEntity.UpdatedOn.Should().NotBe(DateTime.MinValue);
            stubEntity.UpdatedBy.Should().Be(identityName);
        }
    }
}
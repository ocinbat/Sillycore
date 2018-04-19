using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using Sillycore.EntityFramework.Utils;

namespace Sillycore.EntityFramework.Tests
{
    public class SimpleQueryBuilderTests
    {
        private Fixture _fixture;
        private Generator<int> _numberGenerator;
        private FakeRepository _fakeRepository;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _numberGenerator = _fixture.Create<Generator<int>>();
            _fakeRepository = new FakeRepository();
        }

        [Test]
        public void For_ShouldCreateNewInstance()
        {
            var sut = SimpleQueryBuilder.For(_fakeRepository.GetFakes());

            sut.Should().NotBeNull();
        }

        #region Equals

        [Test]
        public void Equals_IfValueIsNotNull_ShouldFindEqualMatches()
        {
            //Arrange
            long? value = 1;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Equals(f => f.Id, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void Equals_IfValueIsNotNullAndMemberIsNullable_ShouldFindEqualMatches()
        {
            //Arrange
            long? value = 3;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Equals(f => f.MissingQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void Equals_IfStringIsNotNullOrEmpty_ShouldFindEqualMatches()
        {
            //Arrange
            var value = "Namex";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Equals(f => f.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void Equals_WhenMemberIsAChildEntity_ShouldFindEqualMatches()
        {
            //Arrange
            var value = "Childo";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakesWithChildren());
            var queryable = sut.Equals(f => f.Child.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        #endregion

        #region String Functions

        [Test]
        public void StartsWith_IfStringIsNotNullOrEmpty_ShouldFindStartingWithMatches()
        {
            //Arrange
            var value = "Name";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.StartsWith(f => f.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(2);
        }

        [Test]
        public void StartWith_WhenMemberIsAChildEntity_ShouldFindStartingWithMatches()
        {
            //Arrange
            var value = "Child";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakesWithChildren());
            var queryable = sut.StartsWith(f => f.Child.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(2);
        }

        [Test]
        public void Like_IfStringHasWildCardInTheEnd_ShouldFindStartingWithMatches()
        {
            //Arrange
            var value = "Name*";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Like(f => f.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(2);
        }

        [Test]
        public void Like_IfStringHasWildCardInTheBeginning_ShouldFindEndingWithMatches()
        {
            //Arrange
            var value = "*ame";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Like(f => f.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void Like_IfStringHasWildCardBothInTheBeginningAndEnd_ShouldFindContainingMatches()
        {
            //Arrange
            var value = "*ame*";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.Like(f => f.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(4);
        }

        [Test]
        public void Like_IfMemberIsAChildEntity_ShouldFindContainingMatches()
        {
            //Arrange
            var value = "*ild*";

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakesWithChildren());
            var queryable = sut.Like(f => f.Child.Name, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(2);
        }

        #endregion

        #region GreaterThan

        [Test]
        public void GreaterThanOrEquals_IfHasValue_ShouldFindGreaterOrEqualMatches()
        {
            //Arrange
            var value = 4;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.GreaterThanOrEquals(f => f.AvailableQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void GreaterThanOrEquals_IfHasValueAndMemberIsNullable_ShouldFindGreaterOrEqualMatches()
        {
            //Arrange
            var value = 3;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.GreaterThanOrEquals(f => f.MissingQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }


        [Test]
        public void GreaterThan_IfHasValueAndMember_ShouldFindGreaterMatches()
        {
            //Arrange
            var value = 3;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.GreaterThan(f => f.AvailableQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void GreaterThan_IfHasValueAndMemberIsNullable_ShouldFindGreaterMatches()
        {
            //Arrange
            var value = 1;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.GreaterThan(f => f.MissingQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void GreaterThan_IfMemberIsAChildEntity_ShouldFindGreaterMatches()
        {
            //Arrange
            var value = 11;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakesWithChildren());
            var queryable = sut.GreaterThan(f => f.Child.Id, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        #endregion

        #region LessThan

        [Test]
        public void LessThanOrEquals_IfHasValue_ShouldFindLessOrEqualMatches()
        {
            //Arrange
            var value = 1;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.LessThanOrEquals(f => f.AvailableQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void LessThanOrEquals_IfHasValueAndMemberIsNullable_ShouldFindLessOrEqualMatches()
        {
            //Arrange
            var value = 1;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.LessThanOrEquals(f => f.MissingQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }


        [Test]
        public void LessThan_IfHasValueAndMember_ShouldFindLessMatches()
        {
            //Arrange
            var value = 2;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.LessThan(f => f.AvailableQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void LessThan_IfHasValueAndMemberIsNullable_ShouldFindLessMatches()
        {
            //Arrange
            var value = 3;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakes());
            var queryable = sut.LessThan(f => f.MissingQuantity, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        [Test]
        public void LessThan_IfMemberIsAChildEntity_ShouldFindLessMatches()
        {
            //Arrange
            var value = 12;

            //Act
            var sut = new SimpleQueryBuilder<Fake>(_fakeRepository.GetFakesWithChildren());
            var queryable = sut.LessThan(f => f.Child.Id, value).Queryable();
            var result = queryable.ToList();

            //Verify
            result.Should().HaveCount(1);
        }

        #endregion

        private class FakeRepository
        {
            public IQueryable<Fake> GetFakes()
            {
                var list = new List<Fake>
                {

                    new Fake() {Id = 1, Name = "Namex", Quantity = 101, MissingQuantity = 1, AvailableQuantity = 1},
                    new Fake() {Id = 2, Name = "Namey", Quantity = 102, MissingQuantity = null, AvailableQuantity = 2},
                    new Fake() {Id = 3, Name = "Anamet", Quantity = 103, MissingQuantity = 3, AvailableQuantity = 3 },
                    new Fake() {Id = 4, Name = "Aname", Quantity = 104,  MissingQuantity = null,AvailableQuantity = 4}
                };
                return list.AsQueryable();
            }

            public IQueryable<Fake> GetFakesWithChildren()
            {
                var list = new List<Fake>
                {
                    new Fake() { Id = 1, Name = "Namex", Quantity = 101, MissingQuantity = 1, AvailableQuantity = 1, Child = new FakeChild()
                        {
                            Id = 11,
                            Name = "Childo"
                        }
                    },
                    new Fake() { Id = 2, Name = "Namey", Quantity = 102, MissingQuantity = null, AvailableQuantity = 2, Child = new FakeChild()
                        {
                            Id = 12,
                            Name = "Childi"
                        }
                    }
                };
                return list.AsQueryable();
            }
        }

        private class Fake
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public long? MissingQuantity { get; set; }
            public long AvailableQuantity { get; set; }
            public FakeChild Child { get; set; }
        }

        private class FakeChild
        {
            public long Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

    }
}
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sillycore.EntityFramework.Extensions;

namespace Sillycore.EntityFramework.Tests
{
    public class LinqExtensionTests
    {
        private const string Term = "code";
        private const string EndsWithTerm = "prefix-code";
        private const string StartsWithTerm = "code-suffix";
        private const string ContainsTerm = "prefix-code-suffix";

        private List<Fake> _testData;

        [SetUp]
        public void SetUp()
        {
            _testData = new List<Fake>()
            {
                new Fake() {Code = Term},
                new Fake() {Code = EndsWithTerm},
                new Fake() {Code = ContainsTerm},
                new Fake() {Code = StartsWithTerm},
            };
        }

        [Test]
        public void Like_IfPhraseDoesntHaveWildCard_ShouldFilterByEquals()
        {
            //Act
            var searchFor = Term;
            var result = _testData.AsQueryable().Like(f => f.Code, searchFor).ToList();

            //Verify
            result.Should().HaveCount(1);
            result.Should().ContainSingle(f => f.Code.Equals(Term));
        }

        [Test]
        public void Like_IfPhraseEndsWithWildCard_ShouldFilterByStartsWith()
        {
            //Act
            var searchFor = $"{Term}*";
            var result = _testData.AsQueryable().Like(f => f.Code, searchFor).ToList();

            //Verify
            result.Should().HaveCount(2);
            result.Should().ContainSingle(f => f.Code.Equals(StartsWithTerm));
            result.Should().ContainSingle(f => f.Code.Equals(Term));
        }

        [Test]
        public void Like_IfPhraseStartWithWildCard_ShouldFilterByEndsWith()
        {
            //Act
            var searchFor = $"*{Term}";
            var result = _testData.AsQueryable().Like(f => f.Code, searchFor).ToList();

            //Verify
            result.Should().HaveCount(2);
            result.Should().ContainSingle(f => f.Code.Equals(EndsWithTerm));
            result.Should().ContainSingle(f => f.Code.Equals(Term));
        }

        [Test]
        public void Like_IfPhraseStartAndEndsWithWildCard_ShouldFilterByContains()
        {
            //Act
            var searchFor = $"*{Term}*";
            var result = _testData.AsQueryable().Like(f => f.Code, searchFor).ToList();

            //Verify
            result.Should().HaveCount(4);
        }

        private class Fake
        {
            public string Code { get; set; }
        }
    }
}
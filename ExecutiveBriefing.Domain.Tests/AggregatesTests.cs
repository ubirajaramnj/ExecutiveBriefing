using Xunit;
using FluentAssertions;
using ExecutiveBriefing.Domain.Aggregates;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Domain.Tests
{
    public class AggregatesTests
    {
        [Fact]
        public void Briefing_Should_CreateCorrectly_When_ValidParameters()
        {
            // Arrange
            var companyName = new CompanyName("Tesla");
            var market = "US";
            var website = "tesla.com";
            var irPage = "ir.tesla.com";

            // Act
            var briefing = Briefing.Create(companyName, market, website, irPage);

            // Assert
            briefing.Id.Should().NotBeNull();
            briefing.CompanyName.Should().Be(companyName);
            briefing.Market.Should().Be(market);
            briefing.WebsiteUrl.Should().Be(website);
            briefing.IRPageUrl.Should().Be(irPage);
            briefing.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(5));
            briefing.Sections.Should().BeEmpty();
            briefing.Sources.Should().BeEmpty();
        }

        [Fact]
        public void AddSection_Should_AppendSection_When_UniqueTitle()
        {
            // Arrange
            var briefing = Briefing.Create(new CompanyName("Tesla"), "US", "tesla.com", "ir.tesla.com");
            var section = BriefingSection.Create("Overview", "Content here", 1);

            // Act
            briefing.AddSection(section);

            // Assert
            briefing.Sections.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(section);
        }

        [Fact]
        public void AddSection_Should_ThrowArgumentException_When_DuplicateTitle()
        {
            // Arrange
            var briefing = Briefing.Create(new CompanyName("Tesla"), "US", "tesla.com", "ir.tesla.com");
            var section1 = BriefingSection.Create("Overview", "Content 1", 1);
            var section2 = BriefingSection.Create("Overview", "Content 2", 2);

            briefing.AddSection(section1);

            // Act
            var action = () => briefing.AddSection(section2);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("A section with title 'Overview' already exists.*");
        }
    }
}

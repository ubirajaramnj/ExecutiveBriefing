using Xunit;
using FluentAssertions;
using ExecutiveBriefing.Domain.ValueObjects;

namespace ExecutiveBriefing.Domain.Tests
{
    public class ValueObjectsTests
    {
        [Fact]
        public void CompanyName_Should_ThrowArgumentException_When_ValueIsEmpty()
        {
            // Act
            var action = () => new CompanyName("");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Company name cannot be empty.*");
        }

        [Fact]
        public void CompanyName_Should_ThrowArgumentException_When_ValueIsNull()
        {
            // Act
            var action = () => new CompanyName(null!);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Company name cannot be empty.*");
        }

        [Fact]
        public void CompanyName_Should_ThrowArgumentException_When_ValueIsTooLong()
        {
            // Arrange
            var longName = new string('A', 151);

            // Act
            var action = () => new CompanyName(longName);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Company name cannot exceed 150 characters.*");
        }

        [Fact]
        public void CompanyName_Should_Create_When_ValueIsValid()
        {
            // Arrange
            var nameStr = "Google Inc.";

            // Act
            var companyName = new CompanyName(nameStr);

            // Assert
            companyName.Value.Should().Be(nameStr);
        }

        [Fact]
        public void BriefingId_Should_GenerateNew_When_Instantiated()
        {
            // Act
            var id1 = new BriefingId();
            var id2 = new BriefingId();

            // Assert
            id1.Value.Should().NotBeEmpty();
            id2.Value.Should().NotBeEmpty();
            id1.Should().NotBe(id2);
        }

        [Fact]
        public void SourceMaterial_Should_ThrowArgumentException_When_ReferenceNameIsEmpty()
        {
            // Act
            var action = () => SourceMaterial.Create(SourceType.WebPage, "", "Content");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Reference name cannot be empty.*");
        }

        [Fact]
        public void SourceMaterial_Should_ThrowArgumentException_When_ContentIsEmpty()
        {
            // Act
            var action = () => SourceMaterial.Create(SourceType.Upload, "file.pdf", "");

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Source content cannot be empty.*");
        }
    }
}

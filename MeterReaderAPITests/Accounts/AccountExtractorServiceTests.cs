using MeterReaderAPI.Accounts;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterReaderAPITests.Accounts;

public class AccountExtractorServiceTests {
    [Fact]
    public void ExtractAccounts_WithValidCsv_ShouldReturnAccounts() {
        var sut = new AccountExtractorService(Mock.Of<ILogger<AccountExtractorService>>());

        // Act
        var accounts = sut.ExtractAccounts().ToList();

        // Assert
        Assert.Equal(27, accounts.Count);
    }
}
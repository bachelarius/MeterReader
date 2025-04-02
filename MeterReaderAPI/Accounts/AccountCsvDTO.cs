using FileHelpers;

namespace MeterReaderAPI.Accounts {
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    [IgnoreEmptyLines]
    public record AccountCsvDTO {
        public int AccountId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public Account ToDomain() => new Account {
            AccountId = AccountId,
            FirstName = FirstName,
            LastName = LastName
        };
    }
}

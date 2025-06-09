namespace AdventureWorksAPIs.DTO
{
    public class UserInfoDTO
    {
        public int BusinessEntityID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PersonType { get; set; } = null!;
        public string? EmailAddress { get; set; }
    }
}

namespace AdventureWorksAPIs.DTO
{
    public class PersonWithPasswordDTO
    {
        public int BusinessEntityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PersonType { get; set; }
    }
}

namespace Chamran.Deed.People.Dtos
{
    public class AdminInformationDto
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public string NationalId { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public long UserId { get; set; }
    }
}
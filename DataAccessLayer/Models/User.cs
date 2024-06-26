namespace DataAccessLayer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

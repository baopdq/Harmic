namespace Harmic.Models
{
    public class LoginRegister
    {
        public string LoginUsername { get; set; }
        public string LoginPassword { get; set; }

        // For Register
        public TbAccount RegisterAccount { get; set; } = new TbAccount();
    }
}

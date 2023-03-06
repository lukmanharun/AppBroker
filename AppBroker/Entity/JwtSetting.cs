namespace AppBroker.Entity
{
    public class JwtSetting
    {
        public string SecretKey { get; set; }
        public string lifetimeToken { get; set; }
        public string IsUser { get; set; }
        public string Audience { get; set; }
    }
    
}

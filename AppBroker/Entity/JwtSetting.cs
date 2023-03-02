namespace AppBroker.Entity
{
    public class JwtSetting
    {
        public string SecretKey { get; set; }
    }
    public class Car
    {
        public string Name { get; set; }
        public virtual string GetNo()
        {
            return "Car";
        }
    }
    public class SparePart:Car
    {
        public override string GetNo()
        {
            return "Oil";
        }
    }

}

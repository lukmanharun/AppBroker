namespace AppBroker.Interfaces
{
    public interface IHelperService
    {
        Task UploadFile(IFormFileCollection files);
    }
}

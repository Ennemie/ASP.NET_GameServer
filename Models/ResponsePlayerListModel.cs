namespace Minecraft.Models
{
    public class ResponsePlayerListModel<T>
    {
        public bool IsSuccess { get; set; }
        public string Notification { get; set; }
        public T Data { get; set; }
    }
}

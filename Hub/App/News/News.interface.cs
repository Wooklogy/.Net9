namespace Hub.App.News;

public interface INewsHub
{
    Task OnReceiveNews();
    Task OnSubscribed(string topic, string message);
}
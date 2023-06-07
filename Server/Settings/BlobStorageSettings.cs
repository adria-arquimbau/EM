namespace EventsManager.Server.Settings;

public class BlobStorageSettings
{
    public string ConnectionString { get; set; }
    public string ProfileImageContainerName { get; set; }
    public string EventImageContainerName { get; set; }
}

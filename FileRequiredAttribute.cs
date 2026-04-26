namespace YoutubeDownloader;

/// <summary>
/// Marks a property as a required file path.
/// On config load it checks that the field is set and the file exists.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FileRequiredAttribute : Attribute { }
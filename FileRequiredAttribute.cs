namespace YoutubeDownloader;

/// <summary>
/// Помечает свойство как обязательный путь к файлу.
/// При загрузке конфига проверяется, что поле заполнено и файл существует.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FileRequiredAttribute : Attribute { }
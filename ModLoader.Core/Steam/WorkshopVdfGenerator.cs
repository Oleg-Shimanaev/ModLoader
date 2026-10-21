using System.Text;
using ModLoader.Core.Models;

namespace ModLoader.Core.Steam;

/// <summary>
/// Генерирует VDF для steamcmd workshop_build_item.
/// </summary>
public sealed class WorkshopVdfGenerator
{
    public string Generate(UploadParameters parameters, ulong publishedFileId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\"workshopitem\"");
        sb.AppendLine("{");
        sb.AppendLine($"\t\"appid\"\t\t\"{parameters.AppId}\"");
        sb.AppendLine($"\t\"publishedfileid\"\t\t\"{publishedFileId}\"");
        sb.AppendLine($"\t\"contentfolder\"\t\t\"{Escape(parameters.ModFolder)}\"");
        sb.AppendLine("\t\"visibility\"\t\t\"0\"");
        sb.AppendLine($"\t\"title\"\t\t\"{Escape(parameters.Title)}\"");
        sb.AppendLine($"\t\"description\"\t\t\"{Escape(parameters.Description)}\"");
        sb.AppendLine("\t\"changenote\"\t\t\"Uploaded via ModLoader\"");

        if (!string.IsNullOrWhiteSpace(parameters.PreviewImagePath))
            sb.AppendLine($"\t\"previewfile\"\t\t\"{Escape(parameters.PreviewImagePath)}\"");

        sb.AppendLine("}");
        return sb.ToString();
    }

    public string WriteToTempFile(UploadParameters parameters, ulong publishedFileId)
    {
        var path = Path.Combine(Path.GetTempPath(), $"modloader_workshop_{Guid.NewGuid():N}.vdf");
        File.WriteAllText(path, Generate(parameters, publishedFileId), Encoding.UTF8);
        return path;
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

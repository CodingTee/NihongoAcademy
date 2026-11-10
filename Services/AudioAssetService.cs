using NihongoAcademy.Models;

namespace NihongoAcademy.Services
{
    public record AudioSourceResult(string? Url, string ExpectedUrl, bool Exists, bool IsExternal);

    public class AudioAssetService
    {
        private readonly IWebHostEnvironment _environment;

        public AudioAssetService(IWebHostEnvironment environment) => _environment = environment;

        public AudioSourceResult Resolve(LessonActivity activity, string speed)
        {
            var configured = speed == "slow" ? activity.SlowAudioUrl : activity.NaturalAudioUrl;
            var expected = $"/audio/lessons/{activity.LessonId}/activity-{activity.Id}-{speed}.mp3";
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var external = Uri.TryCreate(configured, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                return new AudioSourceResult(configured, expected, external || LocalFileExists(configured), external);
            }

            return LocalFileExists(expected)
                ? new AudioSourceResult(expected, expected, true, false)
                : new AudioSourceResult(null, expected, false, false);
        }

        public void ApplyConventionSources(LessonActivity activity)
        {
            activity.SlowAudioUrl ??= Resolve(activity, "slow").Url;
            activity.NaturalAudioUrl ??= Resolve(activity, "natural").Url;
        }

        private bool LocalFileExists(string url)
        {
            if (string.IsNullOrWhiteSpace(_environment.WebRootPath) || string.IsNullOrWhiteSpace(url)) return false;
            var relative = url.Split('?', '#')[0].TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relative));
            var webRoot = Path.GetFullPath(_environment.WebRootPath) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath);
        }
    }
}

using System;

namespace AlbedoBot.Services
{
    public static class YouTubeService
    {
        public static string GetThumbnail(string url)
        {
            string thumbnail = string.Empty;
            if (url.Equals(string.Empty))
            {
                return thumbnail;
            }

            if (url.IndexOf("=", StringComparison.Ordinal) > 0)
            {
                thumbnail = url.Split('=')[1];
            }
            else if (url.IndexOf("/v/", StringComparison.Ordinal) > 0)
            {
                string strVideoCode = url.Substring(url.IndexOf("/v/", StringComparison.Ordinal) + 3);
                int ind = strVideoCode.IndexOf("?", StringComparison.Ordinal);
                thumbnail = strVideoCode.Substring(0, ind == -1 ? strVideoCode.Length : ind);
            }
            else if (url.IndexOf("/", StringComparison.Ordinal) < 6)
            {
                thumbnail = url.Split("/")[3];
            }
            else if (url.IndexOf("/", StringComparison.Ordinal) > 6)
            {
                thumbnail = url.Split("/")[1];
            }

            return "https://img.youtube.com/vi/" + thumbnail + "/mqdefault.jpg";
        }
    }
}
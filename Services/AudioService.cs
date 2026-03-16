using System.Media;
using System.IO;

namespace ClockSystem.Services
{
    public class AudioService
    {
        public void PlayAudio(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                using (var player = new SoundPlayer(filePath))
                {
                    player.PlaySync();
                }
            }
            catch
            {
                // 播放失败，忽略错误
            }
        }
    }
}
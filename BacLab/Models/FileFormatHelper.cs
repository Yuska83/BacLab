namespace BacLab.Models
{
    // Додайте цей допоміжний клас у ваш файл або окремо
    public static class FileFormatHelper
    {
        // Перевірка чи це PDF (сигнатура %PDF-)
        public static bool IsPdf(byte[] data)
        {
            if (data == null || data.Length < 5) return false;
            return data[0] == 0x25 && // '%'
                   data[1] == 0x50 && // 'P'
                   data[2] == 0x44 && // 'D'
                   data[3] == 0x46 && // 'F'
                   data[4] == 0x2D;   // '-'
        }

        // Перевірка чи це DOCX (сигнатура ZIP: PK\x03\x04)
        public static bool IsDocx(byte[] data)
        {
            if (data == null || data.Length < 4) return false;
            return data[0] == 0x50 && // 'P'
                   data[1] == 0x4B && // 'K'
                   data[2] == 0x03 &&
                   data[3] == 0x04;
        }
    }
}

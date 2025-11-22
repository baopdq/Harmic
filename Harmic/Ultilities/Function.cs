namespace Harmic.Ultilities
{
    public class Function
    {
        public static int _AccountID = 0;
        public static string _Username = string.Empty;
        public static string _Email = string.Empty;
        public static string _Message = string.Empty;
        public static string _MessageEmail = string.Empty;
        public static string TitleSlugGenerationAlias(string title)
        {
            return SlugGenerator.SlugGenerator.GenerateSlug(title);
        }
        public static bool IsLogin()
        {
            if(string.IsNullOrEmpty(_Username) || _AccountID == 0)
            {
                return false;
            }
            return true;
        }
    }
}

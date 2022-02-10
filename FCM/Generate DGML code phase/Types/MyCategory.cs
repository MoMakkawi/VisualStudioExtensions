namespace FCM
{
    public class MyCategory
    {
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        private string background;

        public string Background
        {
            get { return background; }
            set { background = value; }
        }
        public MyCategory(string id, string background)
        {
            Id = id;
            Background = background;
        }
    }
}

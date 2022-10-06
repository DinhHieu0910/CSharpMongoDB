namespace DemoMongoDB.Dto
{
    public class PageResultBookDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 100;

        private string _keyword;
        public string Keyword 
        { 
            get => _keyword; 
            set => _keyword = value?.Trim(); 
        }

        private string _category;
        public string Category
        {
            get => _category;
            set => _category = value?.Trim();
        }
        public int Skip
        {
            get
            {
                int skip = PageSize * (PageNumber - 1);
                if (skip < 0)
                {
                    skip = 0;
                }
                return skip;
            }
        }
    }
}

namespace Chamran.Deed.Info.Dtos
{
    public class DashboardViewCategoryInfo
    {

        public DashboardViewCategoryInfo(int categoryId,string categoryName, int countOfCategory)
        {
            this.CategoryId = categoryId;
            this.CategoryName = categoryName;
            this.CountOfCategory = countOfCategory;
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CountOfCategory { get; set; }
    }
}

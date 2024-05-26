namespace Chamran.Deed.Info.Dtos
{
    public class DashboardViewDate
    {
        private string v1;
        private int v2;

        public DashboardViewDate(string dateOfCount, int count)
        {
            DateOfCount = dateOfCount;
            Count = count;
        }

        public int Count { get; set; }
        public string DateOfCount { get; set; }
    }
}

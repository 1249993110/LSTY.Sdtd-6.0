namespace LSTY.Sdtd.Shared.Models
{
    public class InvItem
    {
        public string ItemName { get; set; }
        public int Count { get; set; }
        public int Quality { get; set; }
        public string Icon { get; set; }
        public string Iconcolor { get; set; }
        public int MaxUseTimes { get; set; }
        public float UseTimes { get; set; }
        public InvItem[] Parts { get; set; }
    }
}

namespace IntelligentDemo.Models
{
    public class MusicNote
    {
        public byte Note { get; set; }
        public byte Velocity { get; set; }
        public byte Position { get; set; }
        public byte Duration { get; set; }
        public bool IsRepaired { get; set; }
    }
}

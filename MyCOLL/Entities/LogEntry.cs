namespace MyCOLL.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = "";   
        public string Acao { get; set; } = "";  
        public string? Nome { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.Now;
    }
}

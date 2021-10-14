namespace Algo.Abstracts.Models
{
    public class Connection
    {
        public ConnectionState ConnectionState { get; set; }
        public ConnectionType ConnectionType { get; set; }

        public override string ToString()
        {
            return $"{ConnectionType} {ConnectionState}";
        }
    }
}
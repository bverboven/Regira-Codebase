namespace Regira.Entities.Models
{
    public class EntityInputException<T> : Exception
    {
        public T? Item { get; set; }
        public IDictionary<string, string> InputErrors { get; set; }
        public EntityInputException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
            InputErrors = new Dictionary<string, string>();
        }
    }
}

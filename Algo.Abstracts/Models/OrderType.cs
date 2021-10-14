namespace Algo.Abstracts.Models
{
    public enum OrderType
    {
        
        /// <summary>
        /// Limit
        /// </summary>
        Limit,
        /// <summary>
        /// Market
        /// </summary>
        Market,
        /// <summary>
        /// Conditional (stop-loss, take-profit). 
        /// </summary>
        Conditional
    }
}
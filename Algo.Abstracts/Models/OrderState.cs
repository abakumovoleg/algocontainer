namespace Algo.Abstracts.Models
{
    public enum OrderState
    {
        /// <summary>
        /// Not sent to the trading system.
        /// </summary>
        None,
        /// <summary>
        /// The order is accepted by the exchange and is active.
        /// </summary>
        Active,
        /// <summary>
        /// The order is no longer active on an exchange (it was fully matched or cancelled).
        /// </summary>
        Done,
        /// <summary>
        /// The order is not accepted by the trading system.
        /// </summary>
        Failed,
        /// <summary>
        /// Pending registration. 
        /// </summary>
        Pending
    }
}
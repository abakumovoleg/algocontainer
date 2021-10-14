namespace Algo.Abstracts.Models
{
    public enum TimeInForce
    {
        /// <summary>
        /// Put in queue.
        /// </summary>
        PutInQueue,

        /// <summary>
        /// Fill Or Kill.
        /// </summary>
        MatchOrCancel,

        /// <summary>
        /// Immediate Or Cancel.
        /// </summary>
        CancelBalance
    }
}